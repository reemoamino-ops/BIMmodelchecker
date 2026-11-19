using System.Collections.Generic;

namespace BimCheck.Core;

public enum ErrorType
{
    Missing,
    Duplicate
}

public sealed record ProblemRow(long ElementId, string? Mark, ErrorType ErrorType);

public sealed class CheckResult
{
    public CheckResult(int totalDoors, int missingCount, int duplicateCount, IReadOnlyList<ProblemRow> problems)
    {
        TotalDoors = totalDoors;
        MissingCount = missingCount;
        DuplicateCount = duplicateCount;
        Problems = problems;
    }

    public int TotalDoors { get; }
    /// <summary>Number of doors with a missing mark.</summary>
    public int MissingCount { get; }
    /// <summary>Number of distinct duplicated mark values (e.g. D-001 used twice counts as 1).</summary>
    public int DuplicateCount { get; }
    public IReadOnlyList<ProblemRow> Problems { get; }
}
