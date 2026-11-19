# Ticket #6 verification: Auto-Fix Missing Marks (WRITE PATH)

**Status: not compiled, not run in Revit. Write path NOT proven.** The read-only proofs for other tickets do not cover this one.

## What is proven, and only by reading
The decision logic is in `AutoFixPlanner` (Core, pure C#). Its unit tests in `AutoFixPlannerTests.cs` cover:
- The spec example: D-001, empty, D-003, empty plans D-002 and D-004 for doors 2 and 4.
- Existing marks, including whitespace-only and case variants, are never planned.
- Duplicates are never planned.
- Planned marks are unique and skip marks already in use.

These tests have **not been run** (no .NET SDK here).

## What is NOT proven (this is the risk)
`DoorMarkWriter.AutoFixMissing` changes the model. Its claims are:
1. It sets `ALL_MODEL_MARK` only on doors that are still blank when re-checked inside the transaction.
2. It never overwrites an existing mark.
3. If any step fails, `RollBack()` runs and the model is unchanged.

None of these has been executed. A mock of `Document`, `Transaction`, and `Parameter` would be needed to show claims 1 to 3 without Revit. That mock does not exist yet.

## Required before this ticket closes
- Owner runs the Core tests on the .NET 10 SDK.
- Owner tests in Revit 2027 on a copy of a sample project:
  - Existing marks unchanged after Auto-Fix.
  - Only blank doors gain a mark.
  - Forcing a failure (e.g. a read-only Mark) rolls back and leaves the model unchanged, verified by comparing marks before and after.
- Do not run Auto-Fix on a project without a backup.
