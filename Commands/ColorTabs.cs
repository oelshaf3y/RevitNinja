using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using UIFramework;
using Xceed.Wpf.AvalonDock.Controls;
using Autodesk.Revit.Attributes;
using Grid = System.Windows.Controls.Grid;
using Color = System.Windows.Media.Color;
using Application = Autodesk.Revit.ApplicationServices.Application;
using RevitNinja.Utils;
using System.Text.Json;
using System.IO;

namespace Revit_Ninja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class ColorTabs : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        private Grid RootGrid;
        public static List<List<Color>> colors = new List<List<Color>>()
        {

            new List<Color>()
            {
                Color.FromRgb(244,244,248),Color.FromRgb(0,0,0)
            },

            new List<Color>()
            {
                Color.FromRgb(111,111,133),Color.FromRgb(255,255,255)
            },

            new List<Color>()
            {
                Color.FromRgb(134,115,70),Color.FromRgb(0,0,0)
            },

            new List<Color>()
            {
                Color.FromRgb(254,215,102),Color.FromRgb(0, 0, 0)
            },

            new List<Color>()
            {
                Color.FromRgb(84,160,91),Color.FromRgb(0,0,0)
            },

            new List<Color>()
            {
                Color.FromRgb(42,183,202),Color.FromRgb(0,0,0)
            },

            new List<Color>()
            {
                Color.FromRgb(53,101,110),Color.FromRgb(255,255,255)
            },

            new List<Color>()
            {
                Color.FromRgb(99,84,91),Color.FromRgb(255,255,255)
            },

            new List<Color>()
            {
                Color.FromRgb(156,68,71),Color.FromRgb(255,255,255)
            },

            new List<Color>()
            {
                Color.FromRgb(254,74,73),Color.FromRgb(0,0,0)
            },
        };
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string outputContent = File.ReadAllText(Ninja.dbfile);
            Dictionary<string, object> db = JsonSerializer.Deserialize<Dictionary<string, object>>(outputContent);
            db.TryGetValue("RevitNinja", out object rn);
            Dictionary<string, object> revitninja = new Dictionary<string, object>();
            if (rn is JsonElement rn2)
            {
                revitninja = JsonSerializer.Deserialize<Dictionary<string, object>>(rn2.GetRawText());
            }
            revitninja.TryGetValue("color", out object colorize);
            bool isColored = true;
            if (colorize.ToString().ToLower() == "true")
            {
                revitninja["color"] = false;
                db["RevitNinja"] = revitninja;
                isColored = false;
            }
            else
            {
                revitninja["color"] = true;
                db["RevitNinja"] = revitninja;
            }
            File.WriteAllText(Ninja.dbfile, JsonSerializer.Serialize(db));
            Ninja.ColorTabs();
            RevitNinja.Application.Instance.ToggleColor(isColored);
            return Result.Succeeded;
        }

    }
}
