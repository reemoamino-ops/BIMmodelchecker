using System.Collections.Generic;
using System.Linq;

namespace BimCheck.Core;

/// <summary>
/// Decides which doors get which new marks. Pure logic, no Revit references, so it can be unit tested.
/// Only doors with a missing Mark are planned. Existing marks are never included.
/// </summary>
public static class AutoFixPlanner
{
    public static IReadOnlyList<(long ElementId, string Mark)> Plan(IEnumerable<DoorRecord> doors)
    {
        if (doors == null) throw new System.ArgumentNullException(nameof(doors));
        var list = doors.ToList();

        var missingIds = list.Where(d => MarkChecker.IsMissing(d.Mark))
                             .Select(d => d.ElementId)
                             .ToList();
        if (missingIds.Count == 0) return new List<(long, string)>();

        // Marks already in use across all doors, so new ones never collide with them.
        var newMarks = MarkGenerator.Generate(list.Select(d => d.Mark), missingIds.Count);

        return missingIds.Select((id, i) => (id, newMarks[i])).ToList();
    }
}
