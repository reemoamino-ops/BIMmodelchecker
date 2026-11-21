namespace BimCheck.Core;

/// <summary>Current state of one door, read from the store at write time.</summary>
public sealed record DoorState(string? Mark, bool IsReadOnly);

/// <summary>
/// Everything the Auto-Fix logic needs from the model. Revit implements this in BimCheck.Revit;
/// tests implement it with a fake, so the write logic can run without Revit.
/// </summary>
public interface IDoorMarkStore
{
    /// <summary>Current state of the door, or null if it no longer exists.</summary>
    DoorState? GetDoorState(long elementId);

    /// <summary>Stage a new Mark value. Throws if the value cannot be set.</summary>
    void SetMark(long elementId, string mark);

    void BeginTransaction(string name);

    /// <summary>Commit staged changes. Throws if the commit fails.</summary>
    void Commit();

    /// <summary>Discard staged changes.</summary>
    void RollBack();
}
