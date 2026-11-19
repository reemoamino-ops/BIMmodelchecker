using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using BimCheck.Core;

namespace BimCheck.Revit;

/// <summary>Read-only: collects door instances and their Mark values. Never changes the model.</summary>
public static class DoorCollector
{
    public static List<DoorRecord> Collect(Document doc)
    {
        if (doc == null) throw new ArgumentNullException(nameof(doc));

        var records = new List<DoorRecord>();
        var doors = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Doors)
            .WhereElementIsNotElementType();

        foreach (Element door in doors)
        {
            // A missing parameter is reported as a null Mark, which the checker treats as missing.
            Parameter? markParam = door.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
            string? mark = markParam?.AsString();
            records.Add(new DoorRecord(door.Id.Value, mark));
        }

        return records;
    }
}
