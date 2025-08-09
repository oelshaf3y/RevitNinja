using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class GetLinkLocation : IExternalCommand
    {
        UIDocument uidoc;
        Document doc, Link;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            List<string> names = new List<string>();
            List<double> values = new List<double>();
            UIApplication uiapp = uidoc.Application;
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.ShowDialog();
                OpenOptions openOptions = new OpenOptions();
                openOptions.DetachFromCentralOption = DetachFromCentralOption.DoNotDetach;
                Link = uiapp.Application.OpenDocumentFile(dialog.FileName);
                //List<ProjectLocation> locations = new List<ProjectLocation>();
                StringBuilder sb = new StringBuilder();
                ProjectPosition pos = doc.ActiveProjectLocation.GetProjectPosition(uidoc.Selection.PickPoint());
                XYZ origin = new XYZ(pos.EastWest, pos.NorthSouth, pos.Elevation);
                foreach (ProjectLocation location in Link.ProjectLocations)
                {
                    XYZ p = new XYZ(location.GetProjectPosition(XYZ.Zero).EastWest, location.GetProjectPosition(XYZ.Zero).NorthSouth, location.GetProjectPosition(XYZ.Zero).Elevation);
                    double d = p.DistanceTo(origin);
                    values.Add(d);
                    names.Add(location.Name);
                }
                Link.Close();
                doc.print($"nearest location is:\n{names.ElementAt(values.IndexOf(values.Min()))}");
            }
            catch (Exception ex)
            {
                doc.print(ex.Message);
            }
            return Result.Succeeded;
        }
    }
}
