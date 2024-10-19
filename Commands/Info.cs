using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;
using RevitNinja.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RevitNinja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class Info : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
                string assemblyName = Assembly.GetExecutingAssembly().Location;

                ////System.IO.Stream s = a.GetManifestResourceStream();
                //SoundPlayer player = new SoundPlayer(PathToFIle);
                //player.Play();

                InfoView infoView = new InfoView();
                infoView.Show();
            }
            catch (Exception ex)
            {
                commandData.Application.ActiveUIDocument.Document.print(ex);
                System.Windows.Clipboard.SetText(ex.ToString());
            }
            return Result.Succeeded;
        }
    }
}
