using System;
using System.Collections.Generic;
using System.Linq;
using BimCheck.Core;
using Xunit;

namespace BimCheck.Tests;

/// <summary>
/// In-memory store that models a Revit transaction: writes are staged, Commit publishes them,
/// RollBack discards them. Lets the write logic run without Revit.
/// </summary>
internal sealed class FakeStore : IDoorMarkStore
{
    private readonly Dictionary<long, DoorState> _committed = new();
    private readonly Dictionary<long, string?> _staged = new();
    private bool _inTx;

    public int BeginCalls, CommitCalls, RollBackCalls;
    public List<(long Id, string Mark)> SetCalls { get; } = new();
    public Func<long, string, bool>? FailOn { get; set; }

    public FakeStore(params (long Id, string? Mark, bool ReadOnly)[] doors)
    {
        foreach (var d in doors) _committed[d.Id] = new DoorState(d.Mark, d.ReadOnly);
    }

    public string? Committed(long id) => _committed[id].Mark;
    public bool HasOpenTransaction => _inTx;

    public DoorState? GetDoorState(long elementId)
    {
        if (!_committed.TryGetValue(elementId, out var state)) return null;
        if (_staged.TryGetValue(elementId, out var staged)) return new DoorState(staged, state.IsReadOnly);
        return state;
    }

    public void SetMark(long elementId, string mark)
    {
        if (!_inTx) throw new InvalidOperationException("Set outside a transaction.");
        if (FailOn != null && FailOn(elementId, mark)) throw new InvalidOperationException("boom");
        SetCalls.Add((elementId, mark));
        _staged[elementId] = mark;
    }

    public void BeginTransaction(string name) { BeginCalls++; _inTx = true; }

    public void Commit()
    {
        CommitCalls++;
        foreach (var kv in _staged)
            _committed[kv.Key] = new DoorState(kv.Value, _committed[kv.Key].IsReadOnly);
        _staged.Clear();
        _inTx = false;
    }

    public void RollBack()
    {
        RollBackCalls++;
        _staged.Clear();
        _inTx = false;
    }
}

public class AutoFixRunnerTests
{
    private static List<DoorRecord> Scan(FakeStore store, params long[] ids) =>
        ids.Select(id => new DoorRecord(id, store.GetDoorState(id)!.Mark)).ToList();

    [Fact]
    public void SpecExample_OnlyBlankDoorsChanged_ExistingMarksUntouched()
    {
        var store = new FakeStore((1, "D-001", false), (2, "", false), (3, "D-003", false), (4, "", false));
        var result = AutoFixRunner.Run(store, Scan(store, 1, 2, 3, 4));

        Assert.Equal(new AutoFixRunner.FixOutcome(2, 0), result);
        Assert.Equal("D-002", store.Committed(2));
        Assert.Equal("D-004", store.Committed(4));
        Assert.Equal("D-001", store.Committed(1));
        Assert.Equal("D-003", store.Committed(3));
        Assert.Equal(1, store.BeginCalls);
        Assert.Equal(1, store.CommitCalls);
        Assert.Equal(0, store.RollBackCalls);
        Assert.DoesNotContain(store.SetCalls, c => c.Id == 1 || c.Id == 3);
    }

    [Fact]
    public void DoorThatGainedAMarkSinceScan_IsSkippedNotOverwritten()
    {
        var store = new FakeStore((1, "", false), (2, "", false));
        var scanned = Scan(store, 1, 2);

        // Door 2 is filled after the scan but before the write.
        store.BeginTransaction("someone else's edit");
        store.SetMark(2, "KEEP-ME");
        store.Commit();

        var result = AutoFixRunner.Run(store, scanned);

        Assert.Equal(new AutoFixRunner.FixOutcome(1, 1), result);
        Assert.Equal("KEEP-ME", store.Committed(2));
        Assert.Equal("D-001", store.Committed(1));
    }

    [Fact]
    public void ReadOnlyBlankDoor_IsSkipped()
    {
        var store = new FakeStore((1, "", true), (2, "", false));
        var result = AutoFixRunner.Run(store, Scan(store, 1, 2));

        Assert.Equal(new AutoFixRunner.FixOutcome(1, 1), result);
        Assert.Equal("", store.Committed(1));
        Assert.Equal("D-001", store.Committed(2));
    }

    [Fact]
    public void FailureDuringWrite_RollsBackEverything_AndRethrows()
    {
        var store = new FakeStore((1, "", false), (2, "", false), (3, "", false));
        store.FailOn = (id, _) => id == 2;

        Assert.Throws<InvalidOperationException>(() => AutoFixRunner.Run(store, Scan(store, 1, 2, 3)));

        Assert.Equal(1, store.RollBackCalls);
        Assert.Equal(0, store.CommitCalls);
        Assert.False(store.HasOpenTransaction);
        // Door 1 was staged before the failure but must not be published.
        Assert.Equal("", store.Committed(1));
        Assert.Equal("", store.Committed(2));
        Assert.Equal("", store.Committed(3));
    }

    [Fact]
    public void FailureAtCommit_RollsBack()
    {
        var store = new FakeStore((1, "", false));
        var failing = new CommitFailingStore(store);

        Assert.Throws<InvalidOperationException>(() => AutoFixRunner.Run(failing, Scan(store, 1)));

        Assert.Equal(1, store.RollBackCalls);
        Assert.Equal("", store.Committed(1));
    }

    [Fact]
    public void NothingToFix_OpensNoTransaction()
    {
        var store = new FakeStore((1, "D-001", false));
        var result = AutoFixRunner.Run(store, Scan(store, 1));

        Assert.Equal(new AutoFixRunner.FixOutcome(0, 0), result);
        Assert.Equal(0, store.BeginCalls);
        Assert.Empty(store.SetCalls);
    }

    [Fact]
    public void DeletedDoor_IsSkipped()
    {
        var store = new FakeStore((1, "", false), (2, "", false));
        var scanned = Scan(store, 1, 2);
        var gone = new GoneStore(store, goneId: 2);

        var result = AutoFixRunner.Run(gone, scanned);

        Assert.Equal(new AutoFixRunner.FixOutcome(1, 1), result);
        Assert.Equal("D-001", store.Committed(1));
    }
}

/// <summary>Wraps a store and fails at Commit, to test rollback on commit failure.</summary>
internal sealed class CommitFailingStore : IDoorMarkStore
{
    private readonly FakeStore _inner;
    public CommitFailingStore(FakeStore inner) => _inner = inner;
    public DoorState? GetDoorState(long id) => _inner.GetDoorState(id);
    public void SetMark(long id, string mark) => _inner.SetMark(id, mark);
    public void BeginTransaction(string name) => _inner.BeginTransaction(name);
    public void Commit() => throw new InvalidOperationException("commit refused");
    public void RollBack() => _inner.RollBack();
}

/// <summary>Wraps a store and reports one door as deleted.</summary>
internal sealed class GoneStore : IDoorMarkStore
{
    private readonly FakeStore _inner;
    private readonly long _goneId;
    public GoneStore(FakeStore inner, long goneId) { _inner = inner; _goneId = goneId; }
    public DoorState? GetDoorState(long id) => id == _goneId ? null : _inner.GetDoorState(id);
    public void SetMark(long id, string mark) => _inner.SetMark(id, mark);
    public void BeginTransaction(string name) => _inner.BeginTransaction(name);
    public void Commit() => _inner.Commit();
    public void RollBack() => _inner.RollBack();
}
