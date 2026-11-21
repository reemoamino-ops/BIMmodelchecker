# Ticket #6 verification: Auto-Fix Missing Marks (WRITE PATH)

**Status: not compiled, not run. Write logic is covered by mock-based tests, but the real Revit write is NOT proven.**

## Layers
1. **Planner** (`AutoFixPlanner`, Core): chooses which blank doors get which marks. Tests: `AutoFixPlannerTests.cs`.
2. **Runner** (`AutoFixRunner`, Core): applies the plan through `IDoorMarkStore`. Re-checks each door just before writing, skips doors that gained a mark, are read-only or are gone, and rolls back on any failure. Tests: `AutoFixRunnerTests.cs`, using `FakeStore`, which models staged writes, commit and rollback.
3. **Revit adapter** (`RevitDoorMarkStore`, BimCheck/Revit): maps each store call to the Revit API. **Not mocked.** It is a thin mapping, but it is the one part that touches the real model.

## What the mock tests prove (once run)
- Only blank doors are written; existing marks are never changed.
- A door filled after the scan is skipped, not overwritten.
- A read-only or deleted door is skipped.
- A failure during writes, or at commit, rolls back. Nothing is published.
- No transaction is opened when nothing needs fixing.

## What they do NOT prove
- That `ParameterSet` / `Transaction` in Revit behave as the adapter assumes.
- That Revit's rollback actually restores the model.

## Still required before this ticket closes
- Owner runs the Core and Tests projects with the .NET 10 SDK (`dotnet test` in `BimCheck.Tests`).
- Owner tests in Revit 2027 on a **backup copy** of a sample project:
  - Existing marks unchanged after Auto-Fix.
  - Only blank doors gain a mark.
  - Forcing a failure (e.g. a read-only Mark) rolls back, verified by comparing marks before and after.
