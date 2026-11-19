# Ticket #1 verification: scaffold, add-in manifest, empty window

**Status: not compiled, not run in Revit.** Neither the .NET 10 SDK nor Revit 2027 is available in the build environment.

## What was checked (by reading, not running)
- `BimCheck.addin` names `BimCheck.Commands.BimCheckCommand`, which matches the class namespace and name.
- The `Assembly` path is relative (`BimCheck\BimCheck.dll`), so the DLL and manifest go in the same `Addins\2027` tree.
- `BimCheckCommand` has `[Transaction(TransactionMode.Manual)]`, opens `MainForm`, and returns `Result.Succeeded`.

## Read path vs write path
This ticket reads nothing from the model and writes nothing to it. The only risk is load failure or the window not opening.

## Still to confirm on the owner's machine
1. `dotnet build` succeeds with the .NET 10 SDK and Revit 2027 installed.
2. Copy `BimCheck.dll` and `BimCheck.addin` to `%AppData%\Autodesk\Revit\Addins\2027\`, then start Revit 2027.
3. "BIM Check" appears under Add-ins, and clicking it opens an empty window.
