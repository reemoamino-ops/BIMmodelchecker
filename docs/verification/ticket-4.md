# Ticket #4 verification: collect door instances and read Marks

**Status: not compiled, not run in Revit.** No Revit or .NET SDK in the build environment.

## Scope: reads only
`DoorCollector.Collect` only reads. It creates no transaction, sets no parameter, and does not modify the model. The only write-path risk is none, so this write-up covers reads only.

## What was checked (by reading the code)
- `FilteredElementCollector` filters by `BuiltInCategory.OST_Doors` and `WhereElementIsNotElementType()`, so door types are excluded and only door instances are collected.
- `get_Parameter(BuiltInParameter.ALL_MODEL_MARK)` is read with `AsString()`. A missing parameter becomes a null Mark, which the core checker treats as missing. Nothing throws on a door without the parameter.
- `ElementId.Value` returns `long`, matching `DoorRecord.ElementId`.

## Mock-based check not yet done
A mock of the Revit `Document` and `Element` types would prove the mapping without Revit. It has not been written yet. Until it is, this ticket's read logic is unproven.

## To confirm on the owner's machine
1. The project builds with the .NET 10 SDK and Revit 2027 references.
2. Scanning a sample project returns one record per door instance, with no entries for door types.
