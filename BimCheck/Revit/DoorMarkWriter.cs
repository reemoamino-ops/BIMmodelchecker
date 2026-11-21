using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using BimCheck.Core;

namespace BimCheck.Revit;

/// <summary>Thin Revit adapter for <see cref="IDoorMarkStore"/>. Maps each call to the Revit API.</summary>
public sealed class RevitDoorMarkStore : IDoorMarkStore
{
    private readonly Document _doc;
    private Transaction? _tx;

    public RevitDoorMarkStore(Document doc)
    {
        _doc = doc ?? throw new ArgumentNullException(nameof(doc));
    }

    public DoorState? GetDoorState(long elementId)
    {
        var door = _doc.GetElement(new ElementId(elementId));
        if (door == null) return null;

        Parameter? param = door.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
        if (param == null) return new DoorState(null, IsReadOnly: true);
        return new DoorState(param.AsString(), param.IsReadOnly);
    }

    public void SetMark(long elementId, string mark)
    {
        var door = _doc.GetElement(new ElementId(elementId))
            ?? throw new InvalidOperationException($"Door {elementId} no longer exists.");
        var param = door.get_Parameter(BuiltInParameter.ALL_MODEL_MARK)
            ?? throw new InvalidOperationException($"Door {elementId} has no Mark parameter.");

        if (!param.Set(mark))
            throw new InvalidOperationException($"Could not set Mark on door {elementId}.");
    }

    public void BeginTransaction(string name)
    {
        _tx = new Transaction(_doc, name);
        _tx.Start();
    }

    public void Commit()
    {
        if (_tx == null) throw new InvalidOperationException("No transaction is open.");
        if (_tx.Commit() != TransactionStatus.Committed)
            throw new InvalidOperationException("The Auto-Fix transaction did not commit.");
        _tx.Dispose();
        _tx = null;
    }

    public void RollBack()
    {
        if (_tx == null) return;
        try { _tx.RollBack(); }
        finally { _tx.Dispose(); _tx = null; }
    }
}

public static class DoorMarkWriter
{
    /// <summary>Runs Auto-Fix against the live Revit document.</summary>
    public static AutoFixRunner.FixOutcome AutoFixMissing(Document doc, IReadOnlyList<DoorRecord> doors)
        => AutoFixRunner.Run(new RevitDoorMarkStore(doc), doors);
}
