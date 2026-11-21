using System;
using System.Collections.Generic;
using System.Linq;
using BimCheck.Core;
using Xunit;

namespace BimCheck.Tests;

internal sealed class FakeSource : IDoorSource
{
    private readonly IReadOnlyList<DoorRecord> _doors;
    private readonly Exception? _toThrow;
    public int ReadCalls { get; private set; }

    public FakeSource(IReadOnlyList<DoorRecord> doors, Exception? toThrow = null)
    {
        _doors = doors;
        _toThrow = toThrow;
    }

    public IReadOnlyList<DoorRecord> ReadDoors()
    {
        ReadCalls++;
        if (_toThrow != null) throw _toThrow;
        return _doors.ToList();
    }
}

public class ScanServiceTests
{
    [Fact]
    public void SpecExample_ReadsOnceAndReportsCounts()
    {
        var source = new FakeSource(new[]
        {
            new DoorRecord(1, "D-001"), new DoorRecord(2, ""),
            new DoorRecord(3, "D-003"), new DoorRecord(4, ""),
        });

        var result = ScanService.Scan(source);

        Assert.Equal(1, source.ReadCalls);
        Assert.Equal(4, result.TotalDoors);
        Assert.Equal(2, result.MissingCount);
        Assert.Equal(0, result.DuplicateCount);
    }

    [Fact]
    public void DuplicatesFromSource_AreReported()
    {
        var source = new FakeSource(new[]
        {
            new DoorRecord(1, "D-001"), new DoorRecord(2, "D-002"),
            new DoorRecord(3, "d-001 "), new DoorRecord(4, "D-004"),
        });

        var result = ScanService.Scan(source);

        Assert.Equal(1, result.DuplicateCount);
        Assert.Equal(new long[] { 1, 3 }, result.Problems.Select(p => p.ElementId));
    }

    [Fact]
    public void EmptySource_ReturnsZeros()
    {
        var result = ScanService.Scan(new FakeSource(new DoorRecord[0]));

        Assert.Equal(0, result.TotalDoors);
        Assert.Empty(result.Problems);
    }

    [Fact]
    public void SourceFailure_PropagatesAndDoesNotReturnPartialResult()
    {
        var source = new FakeSource(new DoorRecord[0], new InvalidOperationException("no document"));

        Assert.Throws<InvalidOperationException>(() => ScanService.Scan(source));
    }

    [Fact]
    public void NullSource_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ScanService.Scan(null!));
    }
}
