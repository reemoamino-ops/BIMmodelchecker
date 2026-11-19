using System;
using System.Collections.Generic;

namespace BimCheck.Core;

public static class MarkGenerator
{
    /// <summary>
    /// Returns <paramref name="count"/> new marks D-001, D-002, ... skipping any in
    /// <paramref name="existingMarks"/> (trimmed, case-insensitive). Never returns duplicates.
    /// </summary>
    public static IReadOnlyList<string> Generate(IEnumerable<string?> existingMarks, int count)
    {
        if (existingMarks == null) throw new ArgumentNullException(nameof(existingMarks));
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in existingMarks)
        {
            if (!string.IsNullOrWhiteSpace(m)) used.Add(m.Trim());
        }

        var result = new List<string>(count);
        long n = 1;
        while (result.Count < count)
        {
            var candidate = "D-" + n.ToString("D3");
            n++;
            if (used.Add(candidate)) result.Add(candidate);
        }
        return result;
    }
}
