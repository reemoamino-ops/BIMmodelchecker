using System;
using System.Collections.Generic;
using System.Linq;

namespace BimCheck.Core;

public static class MarkChecker
{
    /// <summary>Missing = null, empty or whitespace-only.</summary>
    public static bool IsMissing(string? mark) => string.IsNullOrWhiteSpace(mark);

    /// <summary>Analyses doors without mutating the input. Problem rows keep input order.</summary>
    public static CheckResult Check(IEnumerable<DoorRecord> doors)
    {
        if (doors == null) throw new ArgumentNullException(nameof(doors));
        var list = doors.ToList();

        var groupSizes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in list)
        {
            if (IsMissing(d.Mark)) continue;
            var key = d.Mark!.Trim();
            groupSizes[key] = groupSizes.TryGetValue(key, out var n) ? n + 1 : 1;
        }

        var problems = new List<ProblemRow>();
        int missing = 0;
        foreach (var d in list)
        {
            if (IsMissing(d.Mark))
            {
                missing++;
                problems.Add(new ProblemRow(d.ElementId, d.Mark, ErrorType.Missing));
            }
            else if (groupSizes[d.Mark!.Trim()] > 1)
            {
                problems.Add(new ProblemRow(d.ElementId, d.Mark, ErrorType.Duplicate));
            }
        }

        // Count distinct duplicated mark values, not doors.
        int duplicateValues = groupSizes.Values.Count(n => n > 1);

        return new CheckResult(list.Count, missing, duplicateValues, problems);
    }
}
