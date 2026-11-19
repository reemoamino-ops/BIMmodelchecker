using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using BimCheck.UI;

namespace BimCheck.Commands;

[Transaction(TransactionMode.Manual)]
public class BimCheckCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
    {
        var uiDoc = commandData.Application.ActiveUIDocument;
        if (uiDoc == null)
        {
            message = "Open a Revit project first.";
            return Result.Failed;
        }

        using var form = new MainForm(uiDoc.Document);
        form.ShowDialog();
        return Result.Succeeded;
    }
}
