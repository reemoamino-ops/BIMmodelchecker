using System;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using BimCheck.Core;
using BimCheck.Revit;

namespace BimCheck.UI;

/// <summary>
/// Main window: overview counts, Scan Model (read-only), Auto-Fix Missing Marks (writes, with confirmation), and a results table.
/// </summary>
public class MainForm : Form
{
    private static readonly System.Drawing.Color MissingRowColor = System.Drawing.Color.MistyRose;
    private static readonly System.Drawing.Color DuplicateRowColor = System.Drawing.Color.LightYellow;

    private readonly Document _doc;
    private readonly Label _totalLabel = new();
    private readonly Label _missingLabel = new();
    private readonly Label _duplicateLabel = new();
    private readonly Label _summaryLabel = new();
    private readonly Button _fixButton = new();
    private readonly DataGridView _grid = new();

    private CheckResult? _lastResult;
    private int _corrected;

    public MainForm(Document doc)
    {
        _doc = doc ?? throw new ArgumentNullException(nameof(doc));

        Text = "BIM Check";
        ClientSize = new Size(720, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9f);
        MinimumSize = new Size(560, 380);

        var overview = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 4,
            Padding = new Padding(12),
        };
        var scanButton = new Button { Text = "Scan Model", AutoSize = true, Padding = new Padding(8, 4, 8, 4) };
        scanButton.Click += OnScanClicked;
        overview.Controls.Add(_totalLabel, 0, 0);
        overview.Controls.Add(_missingLabel, 1, 0);
        overview.Controls.Add(_duplicateLabel, 2, 0);
        overview.Controls.Add(scanButton, 3, 0);
        overview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
        overview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
        overview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17f));
        overview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17f));

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.Columns.Add("ElementId", "Element ID");
        _grid.Columns.Add("Mark", "Current Mark");
        _grid.Columns.Add("ErrorType", "Error Type");

        _fixButton.Text = "Auto-Fix Missing Marks";
        _fixButton.AutoSize = true;
        _fixButton.Padding = new Padding(8, 4, 8, 4);
        _fixButton.Enabled = false;
        _fixButton.Click += OnFixClicked;

        _summaryLabel.AutoSize = true;
        _summaryLabel.Text = "Press Scan Model to check the doors in this project.";

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            Padding = new Padding(12, 8, 12, 8),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
        };
        bottom.Controls.Add(_fixButton);
        bottom.Controls.Add(_summaryLabel);

        // Add the fill control first so the docked top and bottom panels keep their space.
        Controls.Add(_grid);
        Controls.Add(bottom);
        Controls.Add(overview);

        ShowCounts(0, 0, 0);
    }

    private void OnScanClicked(object? sender, EventArgs e)
    {
        try
        {
            RunScan();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "The scan failed. Nothing in the model was changed.\n\n" + ex.Message,
                "BIM Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnFixClicked(object? sender, EventArgs e)
    {
        if (_lastResult == null || _lastResult.MissingCount == 0) return;

        var answer = MessageBox.Show(this,
            $"Assign new marks to {_lastResult.MissingCount} doors that have no Mark?\n\n" +
            "Doors with existing marks will not change.",
            "BIM Check", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes) return;

        try
        {
            var outcome = DoorMarkWriter.AutoFixMissing(_doc, DoorCollector.Collect(_doc));
            _corrected = outcome.Fixed;

            string skippedNote = outcome.Skipped > 0
                ? $"\n{outcome.Skipped} doors were skipped because they changed or cannot be edited."
                : string.Empty;
            MessageBox.Show(this, $"Assigned marks to {outcome.Fixed} doors.{skippedNote}",
                "BIM Check", MessageBoxButtons.OK, MessageBoxIcon.Information);

            RunScan();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                "Auto-fix failed. The changes were rolled back, so the model is unchanged.\n\n" + ex.Message,
                "BIM Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RunScan()
    {
        var result = ScanService.Scan(new RevitDoorSource(_doc));
        _lastResult = result;
        ShowResult(result);
    }

    private void ShowResult(CheckResult result)
    {
        ShowCounts(result.TotalDoors, result.MissingCount, result.DuplicateCount);
        _fixButton.Enabled = result.MissingCount > 0;

        _grid.Rows.Clear();
        foreach (var problem in result.Problems)
        {
            bool missing = problem.ErrorType == ErrorType.Missing;
            int rowIndex = _grid.Rows.Add(
                problem.ElementId.ToString(),
                string.IsNullOrWhiteSpace(problem.Mark) ? "(empty)" : problem.Mark,
                missing ? "Missing" : "Duplicate");
            _grid.Rows[rowIndex].DefaultCellStyle.BackColor = missing ? MissingRowColor : DuplicateRowColor;
        }

        int issueDoors = result.Problems.Count;
        _summaryLabel.Text = issueDoors == 0
            ? $"No issues found in {result.TotalDoors} doors. Corrected: {_corrected}."
            : $"{issueDoors} doors have issues. Corrected: {_corrected}.";
    }

    private void ShowCounts(int total, int missing, int duplicates)
    {
        _totalLabel.Text = $"Total doors\n{total}";
        _missingLabel.Text = $"Missing marks\n{missing}";
        _duplicateLabel.Text = $"Duplicate mark values\n{duplicates}";
        foreach (var label in new[] { _totalLabel, _missingLabel, _duplicateLabel })
        {
            label.AutoSize = true;
            label.Margin = new Padding(0, 0, 12, 0);
        }
    }
}
