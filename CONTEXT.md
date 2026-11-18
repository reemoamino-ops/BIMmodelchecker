# BIM Check — Project Context

Revit model-quality add-in. Source of truth: `BIM_CHECKER_SPEC_FILE.docx` (v1.0 MVP).

## Stack
- C#, .NET 8.0, Windows Forms
- Autodesk Revit 2027 API (RevitAPI.dll, RevitAPIUI.dll)
- Visual Studio 2022; manifest installed to `%AppData%\Autodesk\Revit\Addins\2027`

## Standing rules
1. v1.0 checks **doors only** (`OST_Doors`, door instances) via `FilteredElementCollector`.
2. Mark = `BuiltInParameter.ALL_MODEL_MARK`.
3. Missing mark = empty or whitespace-only.
4. Duplicate = same non-empty mark, compared case-insensitively and trimmed.
5. Scanning must **never** modify the model.
6. Auto-fix touches **only** doors with missing marks. Existing marks are never overwritten. Duplicates are reported, never changed.
7. New marks use `D-001`, `D-002`, … and skip numbers already in use (case-insensitive check).
8. All model changes happen inside a Revit `Transaction`, rolled back safely on failure. Confirm with the user before modifying.
9. UI: simple Windows Forms window — overview, Scan Model / Auto-Fix buttons, results table (Element ID, Mark, Error type, visually distinct), summary.
10. Out of scope for v1.0: other categories, duplicate auto-fix, geometry/clash/sheet checks, AI, cloud, advanced exports.

## Environment limits
Revit and Visual Studio are not available in the cloud workspace. Code is written here but compiled and tested by the owner on their own machine. Revit-API logic needs a mock-based verification write-up before its ticket can close.

## Workflow
ai-kaderskill ticket cycle; tracker = GitHub Issues; labels: `needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`.
