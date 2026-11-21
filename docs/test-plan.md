# BIM Check v1.0: Revit 2027 test plan (ticket #7)

Run this on a **backup copy** of a sample project. Auto-Fix changes the model, so never run it on a live project.

## 1. Before you start
- [ ] Revit 2027 installed.
- [ ] The .NET 10 SDK installed. Visual Studio must be a version that supports it.
- [ ] Core and test projects pass: in `BimCheck.Tests`, run `dotnet test`. Every test should pass. If any fail, stop and report them.
- [ ] Build the add-in: in `BimCheck`, run `dotnet build -c Release`. The build should succeed with no errors.

## 2. Install
1. Copy `BimCheck.dll` (from `BimCheck/bin/Release/net10.0-windows/`) into `%AppData%\Autodesk\Revit\Addins\2027\BimCheck\`.
2. Copy `BimCheck.addin` into `%AppData%\Autodesk\Revit\Addins\2027\`.
3. Start Revit 2027. Open the sample project.
4. **Expected:** "BIM Check" appears under the Add-ins tab. Clicking it opens the main window.

If it doesn't appear, check that the `Assembly` path in `BimCheck.addin` is `BimCheck\BimCheck.dll`, and that the DLL is in that folder.

## 3. Sample project
Prepare a copy with doors covering each case:
- Doors with correct, unique marks (e.g. D-001, D-002).
- Doors with empty marks.
- Doors with whitespace-only marks.
- Two or more doors sharing a mark.
- Two doors whose marks differ only by case (e.g. `d-005` and `D-005`).
- A mix of existing and missing marks.
- Door types with no instances (these must not be counted).

Write down the expected counts before you scan.

## 4. Test cases

| ID | Steps | Expected result |
|---|---|---|
| TC-01 | Click **Scan Model**. | Total doors equals the number of door instances. Door types are not counted. |
| TC-02 | Compare the Missing count with your list. | Empty and whitespace-only marks are counted. Valid marks are not. |
| TC-03 | Compare the Duplicate count with your list. | Duplicate **mark values** are counted once each (two doors sharing D-005 = 1). |
| TC-04 | Look at the results table. | One row per problem door. Missing rows say "(empty)" for blank marks. Missing and Duplicate rows have different colours and text. |
| TC-05 | Click **Scan Model** again. | Results are the same. The model is unchanged. |
| TC-06 | Click **Auto-Fix Missing Marks**. Click **No** in the confirmation. | Nothing changes. |
| TC-07 | Click **Auto-Fix Missing Marks**. Click **Yes**. | Only blank doors get marks. Each new mark is unique and in D-001 form. |
| TC-08 | Check the marks after TC-07. | Existing marks are exactly as before. Numbers already in use are skipped (e.g. if D-002 exists, the next new mark is not D-002). |
| TC-09 | Check the success message and the results. | The message says how many doors were fixed. The re-scan shows the new counts. |
| TC-10 | Force a failure (see section 5). | An error message appears. Every mark is unchanged. The model is not left half-changed. |
| TC-11 | Close and reopen Revit. Run the add-in again. | It still loads and works. |

## 5. Rollback test (TC-10)
Make one blank door fail: for example, set its Mark parameter to read-only in the sample project, or change the model so one door cannot be edited.

1. Note every door's Mark before the run (a screenshot or a list).
2. Run Auto-Fix.
3. **Expected:** the planned changes are rolled back and the marks match your notes exactly.

Note: the code skips read-only doors rather than failing on them, so this may not trigger an error. If it doesn't, report what happened and we will design a test that forces a real failure.

## 6. Success criteria (from `specs/bim-check-v1.md`)
- [ ] The plugin loads in Revit 2027. (TC-01, TC-11)
- [ ] It collects all door instances. (TC-01)
- [ ] It correctly finds missing marks. (TC-02)
- [ ] It correctly finds duplicate marks. (TC-03)
- [ ] It generates unique marks for missing values. (TC-07)
- [ ] It preserves all existing marks. (TC-08)
- [ ] The results are understandable. (TC-04)
- [ ] It causes no unintended model changes. (TC-05, TC-06, TC-10)

## 7. Reporting problems
Open a GitHub issue for each failure. Include:
- The test case ID.
- What you did.
- What you expected.
- What happened, with a screenshot if possible.
- The Revit build number.

Label it `needs-triage`. Include the sample project's door counts if you can.
