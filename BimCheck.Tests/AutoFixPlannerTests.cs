using System.Linq;
using BimCheck.Core;
using Xunit;

namespace BimCheck.Tests;

public class AutoFixPlannerTests
{
    private static DoorRecord[] Doors(params string?[] marks) =>
        marks.Select((m, i) => new DoorRecord(i + 1, m)).ToArray();

    [Fact]
    public void SpecExample_AssignsNextFreeMarksToMissingOnly()
    {
        var plan = AutoFixPlanner.Plan(Doors("D-001", "", "D-003", ""));
        Assert.Equal(new[] { (2L, "D-002"), (4L, "D-004") }, plan);
    }

    [Fact]
    public void ExistingMarks_AreNeverPlanned()
    {
        var plan = AutoFixPlanner.Plan(Doors("D-001", "D-002", "  ", "d-003"));
        Assert.Single(plan);
        Assert.Equal(3L, plan[0].ElementId);
        Assert.DoesNotContain(plan, p => p.ElementId == 1 || p.ElementId == 2 || p.ElementId == 4);
    }

    [Fact]
    public void WhitespaceAndNull_AreTreatedAsMissing()
    {
        var plan = AutoFixPlanner.Plan(Doors("   ", null));
        Assert.Equal(new[] { (1L, "D-001"), (2L, "D-002") }, plan);
    }

    [Fact]
    public void NoMissingMarks_PlansNothing()
    {
        Assert.Empty(AutoFixPlanner.Plan(Doors("D-001", "D-002")));
    }

    [Fact]
    public void NewMarks_AreUniqueAcrossAllPlannedDoors()
    {
        var plan = AutoFixPlanner.Plan(Doors("", "", "", "", "D-002"));
        var marks = plan.Select(p => p.Mark).ToList();
        Assert.Equal(marks.Count, marks.Distinct().Count());
        Assert.DoesNotContain("D-002", marks);
        Assert.Equal(new[] { "D-001", "D-003", "D-004", "D-005" }, marks);
    }

    [Fact]
    public void DuplicatesAreNotChanged()
    {
        // Duplicate marks are reported only, so no door in a duplicate group is planned.
        Assert.Empty(AutoFixPlanner.Plan(Doors("D-001", "D-001")));
    }
}
