using System;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using BimCheck.Core;
using BimCheck.Revit;

namespace BimCheck.UI;

/// <summary>
/// Main window: overview counts, Scan Model, and a results table.
/// Scanning only reads the model. Auto-Fix is added in a later ticket.
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
    private readonly DataGridView _grid = new();

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

        _summaryLabel.Dock = DockStyle.Bottom;
        _summaryLabel.AutoSize = false;
        _summaryLabel.Height = 32;
        _summaryLabel.Padding = new Padding(12, 8, 12, 0);
        _summaryLabel.Text = "Press Scan Model to check the doors in this project.";

        // Add the fill control first so the docked top and bottom panels keep their space.
        Controls.Add(_grid);
        Controls.Add(_summaryLabel);
        Controls.Add(overview);

        ShowCounts(0, 0, 0);
    }

    private void OnScanClicked(object? sender, EventArgs e)
    {
        try
        {
            var doors = DoorCollector.Collect(_doc);
            var result = MarkChecker.Check(doors);
            ShowResult(result);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "The scan failed. Nothing in the model was changed.\n\n" + ex.Message,
                "BIM Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowResult(CheckResult result)
    {
        ShowCounts(result.TotalDoors, result.MissingCount, result.DuplicateCount);

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
            ? $"No issues found in {result.TotalDoors} doors."
            : $"{issueDoors} doors have issues. Corrected: 0.";
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
