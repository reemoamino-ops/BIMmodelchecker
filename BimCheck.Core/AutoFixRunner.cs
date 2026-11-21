using System;
using System.Collections.Generic;

namespace BimCheck.Core;

public static class AutoFixRunner
{
    public sealed record FixOutcome(int Fixed, int Skipped);

    /// <summary>
    /// Applies planned marks in one transaction. Re-checks each door just before writing, so a door
    /// that gained a mark since the scan is skipped. Any failure rolls back every change.
    /// </summary>
    public static FixOutcome Run(IDoorMarkStore store, IReadOnlyList<DoorRecord> doors)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));
        if (doors == null) throw new ArgumentNullException(nameof(doors));

        var plan = AutoFixPlanner.Plan(doors);
        if (plan.Count == 0) return new FixOutcome(0, 0);

        int fixedCount = 0, skipped = 0;
        store.BeginTransaction("BIM Check: Auto-Fix Missing Marks");
        try
        {
            foreach (var (elementId, mark) in plan)
            {
                var state = store.GetDoorState(elementId);

                // Never overwrite: skip doors that are gone, read-only, or no longer blank.
                if (state == null || state.IsReadOnly || !MarkChecker.IsMissing(state.Mark))
                {
                    skipped++;
                    continue;
                }

                store.SetMark(elementId, mark);
                fixedCount++;
            }

            store.Commit();
        }
        catch
        {
            try { store.RollBack(); } catch { /* transaction already finished */ }
            throw;
        }

        return new FixOutcome(fixedCount, skipped);
    }
}
