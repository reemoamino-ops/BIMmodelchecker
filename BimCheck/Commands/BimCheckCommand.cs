using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using BimCheck.UI;

namespace BimCheck.Commands;

[Transaction(TransactionMode.Manual)]
public class BimCheckCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
    {
        using var form = new MainForm();
        form.ShowDialog();
        return Result.Succeeded;
    }
}
