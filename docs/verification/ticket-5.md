# Ticket #5 verification: main window, Scan Model, results table

**Status: not compiled, not run in Revit.**

## Scope: reads only
Scan Model calls `DoorCollector.Collect` and `MarkChecker.Check`. Neither creates a transaction or sets a parameter. Only reads are involved, so this write-up covers reads only. Auto-Fix is not in this ticket.

## What was checked (by reading the code)
- Counts come from `CheckResult`, so the overview matches the core logic that is unit tested.
- Missing rows show "(empty)" when the Mark is blank, so an empty value is never shown as a blank cell.
- Missing and Duplicate rows have different text and different background colours (MistyRose and LightYellow), so the two error types are distinguishable without colour alone.
- Scan is inside a try/catch. A failure shows an error message and leaves the grid unchanged.

## Deviations from the ticket
- The ticket listed `MainForm.Designer.cs`. The layout is written in code, so there is no designer file.
- The summary says "Corrected: 0" until Auto-Fix exists.

## Not yet proven
Nothing here runs. A mock of `Document` (used by `DoorCollector`) would be needed to exercise the scan path. That mock is not written yet.

## To confirm on the owner's machine
1. Build succeeds on .NET 10 with Revit 2027 references.
2. Scan Model on a sample project: counts match the doors you expect, and each problem row has the right type and colour.
3. Re-scanning works, and the model is unchanged afterwards.

## Update: scan logic now mock-tested
Scan Model calls `ScanService.Scan(new RevitDoorSource(...))`. The counts and rows come from the mock-tested path in `ScanServiceTests`. The WinForms layout and the colours are still untested and need a check in Revit.
