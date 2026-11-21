using System.Collections.Generic;

namespace BimCheck.Core;

/// <summary>
/// Read-only view of the doors in a model. Revit implements it in BimCheck.Revit; tests use a fake.
/// It has no write methods, so scanning cannot change the model.
/// </summary>
public interface IDoorSource
{
    IReadOnlyList<DoorRecord> ReadDoors();
}
