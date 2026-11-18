# BIM Check v1.0 — User Stories

Source: `BIM_CHECKER_SPEC_FILE.docx`. Platform: Revit 2027, C#, .NET 8, Windows Forms.

## Roles
- **Modeler** — architect / BIM modeler using the plugin day to day.

## Stories

### US-1 Install and launch
As a modeler, I want BIM Check to appear in Revit's Add-ins tab after I install it, so that I can open it from any project.
- Acceptance: a `.addin` manifest in `%AppData%\Autodesk\Revit\Addins\2027` registers one external command; clicking it opens the main window.

### US-2 Scan doors
As a modeler, I want to click **Scan Model** to collect all door instances in the active project, so that I can see the state of their Mark data.
- Acceptance: all door instances (`OST_Doors`) are read; each door's Mark (`ALL_MODEL_MARK`) is captured; the model is **not modified** during a scan.

### US-3 Overview counts
As a modeler, I want to see the total doors, missing marks and duplicate marks, so that I understand the model's health at a glance.
- Acceptance: the three counts update after every scan.

### US-4 Detect missing marks
As a modeler, I want doors with empty or whitespace-only Marks flagged as "Missing", so that I know which doors lack identification.
- Acceptance: empty and whitespace-only values are both flagged; valid marks are not.

### US-5 Detect duplicate marks
As a modeler, I want all doors that share a Mark flagged as "Duplicate", so that I can fix conflicting identification.
- Acceptance: comparison ignores case and surrounding whitespace; every member of a duplicate group is listed; empty marks are never treated as duplicates; duplicates are reported only, never changed.

### US-6 Results table
As a modeler, I want a table of Element ID, current Mark and error type, with error types visually distinct, so that I can find and fix problem doors quickly.
- Acceptance: one row per problem door; Missing and Duplicate are clearly distinguishable (e.g. colour plus text).

### US-7 Auto-fix missing marks
As a modeler, I want to click **Auto-Fix Missing Marks** to assign unique sequential marks (D-001, D-002, …) to doors lacking one, so that I don't number them by hand.
- Acceptance: only missing-mark doors change; existing marks are never overwritten; numbers already in use are skipped (e.g. D-001 and D-003 exist → next is D-002); generated marks are verified unique before assignment.

### US-8 Safe modification
As a modeler, I want a confirmation before any change and a safe rollback on failure, so that I never get unintended model changes.
- Acceptance: confirmation dialog before modifying; all changes in one Revit `Transaction`; on any failure the transaction is rolled back and an error message is shown; success shows a confirmation with the count fixed.

### US-9 Summary and re-scan
As a modeler, I want a summary of total issues detected and elements corrected, and to be able to re-scan after fixing, so that I can verify the result.
- Acceptance: summary shows issues found and elements corrected; after auto-fix the results refresh (or re-scan is available) and show updated counts.

### US-10 Clear messages
As a modeler, I want clear success and failure messages and labels, so that I always understand what happened.

## Out of scope (v1.0)
Other categories, auto-fixing duplicates, geometry/clash checks, view/sheet checks, AI, cloud/database, advanced report export, configurable prefix/start number.

## Success criteria
Loads in Revit 2027; collects all doors; correctly finds missing and duplicate marks; generates unique marks; preserves existing marks; understandable results; no unintended model changes.
