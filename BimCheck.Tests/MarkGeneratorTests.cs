using System.Collections.Generic;
using System.Linq;
using BimCheck.Core;
using Xunit;

namespace BimCheck.Tests;

public class MarkGeneratorTests
{
    [Fact]
    public void SkipsUsedMarks()
    {
        Assert.Equal(new[] { "D-002", "D-004" }, MarkGenerator.Generate(new[] { "D-001", "D-003" }, 2));
    }

    [Fact]
    public void NoneExisting_StartsAtD001()
    {
        Assert.Equal(new[] { "D-001", "D-002", "D-003" }, MarkGenerator.Generate(new string[0], 3));
    }

    [Fact]
    public void AllExisting_ContinuesAfter()
    {
        Assert.Equal(new[] { "D-004" }, MarkGenerator.Generate(new[] { "D-001", "D-002", "D-003" }, 1));
    }

    [Fact]
    public void FillsGaps()
    {
        Assert.Equal(new[] { "D-002", "D-003", "D-005" }, MarkGenerator.Generate(new[] { "D-001", "D-004", "D-006" }, 3));
    }

    [Fact]
    public void MixedCaseAndWhitespace_CountAsUsed()
    {
        Assert.Equal(new[] { "D-003" }, MarkGenerator.Generate(new[] { "d-001", "  D-002  " }, 1));
    }

    [Fact]
    public void NullAndBlankExisting_AreIgnored()
    {
        Assert.Equal(new[] { "D-001" }, MarkGenerator.Generate(new string?[] { null, "", "  " }, 1));
    }

    [Fact]
    public void ZeroCount_ReturnsEmpty()
    {
        Assert.Empty(MarkGenerator.Generate(new[] { "D-001" }, 0));
    }

    [Fact]
    public void PastD999_DoesNotBreak()
    {
        var all = MarkGenerator.Generate(new string[0], 1001);
        Assert.Equal("D-999", all[998]);
        Assert.Equal("D-1000", all[999]);
        Assert.Equal("D-1001", all[1000]);
    }

    [Fact]
    public void LargeCount_UniqueAndUnused()
    {
        var existing = new HashSet<string>(new[] { "D-005", "D-1000" });
        var r = MarkGenerator.Generate(existing, 1500);
        Assert.Equal(1500, r.Count);
        Assert.Equal(1500, r.Select(x => x.ToUpperInvariant()).Distinct().Count());
        Assert.DoesNotContain("D-005", r);
        Assert.DoesNotContain("D-1000", r);
        Assert.Contains("D-1001", r);
        Assert.Equal("D-001", r[0]);
    }

    [Fact]
    public void DoesNotMutateInput()
    {
        var existing = new List<string> { "D-001" };
        MarkGenerator.Generate(existing, 3);
        Assert.Equal(new[] { "D-001" }, existing);
    }
}
