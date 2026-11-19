using System.Drawing;
using System.Windows.Forms;

namespace BimCheck.UI;

/// <summary>Main window. Phase 1: empty shell only; scan and fix come in later tickets.</summary>
public class MainForm : Form
{
    public MainForm()
    {
        Text = "BIM Check";
        ClientSize = new Size(640, 420);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9f);
    }
}
