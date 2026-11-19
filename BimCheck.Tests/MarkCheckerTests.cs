using System.Linq;
using BimCheck.Core;
using Xunit;

namespace BimCheck.Tests;

public class MarkCheckerTests
{
    private static DoorRecord[] Doors(params string?[] marks) =>
        marks.Select((m, i) => new DoorRecord(i + 1, m)).ToArray();

    [Fact]
    public void SpecExample_MissingMarks()
    {
        var r = MarkChecker.Check(Doors("D-001", "", "D-003", ""));
        Assert.Equal(4, r.TotalDoors);
        Assert.Equal(2, r.MissingCount);
        Assert.Equal(0, r.DuplicateCount);
        Assert.Equal(new long[] { 2, 4 }, r.Problems.Select(p => p.ElementId));
        Assert.All(r.Problems, p => Assert.Equal(ErrorType.Missing, p.ErrorType));
    }

    [Fact]
    public void SpecExample_DuplicateMarks_AllGroupMembersReturned()
    {
        var r = MarkChecker.Check(Doors("D-001", "D-002", "D-001", "D-004"));
        Assert.Equal(4, r.TotalDoors);
        Assert.Equal(0, r.MissingCount);
        Assert.Equal(1, r.DuplicateCount);
        Assert.Equal(new long[] { 1, 3 }, r.Problems.Select(p => p.ElementId));
        Assert.All(r.Problems, p => Assert.Equal(ErrorType.Duplicate, p.ErrorType));
    }

    [Fact]
    public void CaseAndTrim_AreDuplicates()
    {
        var r = MarkChecker.Check(Doors("d-001", "D-001 "));
        Assert.Equal(1, r.DuplicateCount);
        Assert.Equal(2, r.Problems.Count);
    }

    [Fact]
    public void WhitespaceOnlyAndNull_AreMissing()
    {
        var r = MarkChecker.Check(Doors("   ", "\t", null));
        Assert.Equal(3, r.MissingCount);
        Assert.Equal(0, r.DuplicateCount);
    }

    [Fact]
    public void EmptyMarks_AreNeverDuplicates()
    {
        var r = MarkChecker.Check(Doors("", "", "  "));
        Assert.Equal(3, r.MissingCount);
        Assert.Equal(0, r.DuplicateCount);
    }

    [Fact]
    public void AllValid_NoProblems()
    {
        var r = MarkChecker.Check(Doors("D-001", "D-002", "D-003"));
        Assert.Equal(3, r.TotalDoors);
        Assert.Equal(0, r.MissingCount);
        Assert.Equal(0, r.DuplicateCount);
        Assert.Empty(r.Problems);
    }

    [Fact]
    public void EmptyList_AllZero()
    {
        var r = MarkChecker.Check(new DoorRecord[0]);
        Assert.Equal(0, r.TotalDoors);
        Assert.Equal(0, r.MissingCount);
        Assert.Equal(0, r.DuplicateCount);
        Assert.Empty(r.Problems);
    }

    [Fact]
    public void DoesNotMutateInput()
    {
        var input = Doors("D-001 ", "", "d-001");
        var copy = input.ToArray();
        MarkChecker.Check(input);
        Assert.Equal(copy, input);
        Assert.Equal("D-001 ", input[0].Mark);
    }
}
