using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using BimCheck.Core;

namespace BimCheck.Revit;

/// <summary>
/// The only class that changes the model. Applies planned marks inside one transaction.
/// Rolls back everything if any step fails.
/// </summary>
public static class DoorMarkWriter
{
    public sealed record FixOutcome(int Fixed, int Skipped);

    public static FixOutcome AutoFixMissing(Document doc, IReadOnlyList<DoorRecord> doors)
    {
        if (doc == null) throw new ArgumentNullException(nameof(doc));
        if (doors == null) throw new ArgumentNullException(nameof(doors));

        var plan = AutoFixPlanner.Plan(doors);
        if (plan.Count == 0) return new FixOutcome(0, 0);

        int fixedCount = 0, skipped = 0;
        using var tx = new Transaction(doc, "BIM Check: Auto-Fix Missing Marks");
        tx.Start();
        try
        {
            foreach (var (elementId, mark) in plan)
            {
                var door = doc.GetElement(new ElementId(elementId));
                Parameter? param = door?.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);

                // Re-check inside the transaction. Never overwrite a mark that exists now.
                if (door == null || param == null || param.IsReadOnly
                    || !string.IsNullOrWhiteSpace(param.AsString()))
                {
                    skipped++;
                    continue;
                }

                if (!param.Set(mark))
                    throw new InvalidOperationException($"Could not set Mark on door {elementId}.");
                fixedCount++;
            }

            if (tx.Commit() != TransactionStatus.Committed)
                throw new InvalidOperationException("The Auto-Fix transaction did not commit.");
        }
        catch
        {
            try { tx.RollBack(); } catch { /* already finished */ }
            throw;
        }

        return new FixOutcome(fixedCount, skipped);
    }
}
