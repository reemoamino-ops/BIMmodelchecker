namespace BimCheck.Core;

/// <summary>Plain snapshot of a door: its element id and its Mark value (may be null).</summary>
public sealed record DoorRecord(long ElementId, string? Mark);
