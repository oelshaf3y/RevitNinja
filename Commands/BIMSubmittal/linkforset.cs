using Autodesk.Revit.DB;

namespace Revit_Ninja.Commands.BIMSubmittal
{
    public class linkforset
    {
        public RevitLinkInstance RLI;
        public bool IsChecked { get; set; }
        public string Name { get; set; }
        public bool create3D { get; set; }
        public bool deleteWIP { get; set; }
        public bool removeCad { get; set; }
        public bool removeLinks { get; set; }
        public bool populateSections { get; set; }
        public bool purgeFilters { get; set; }
        public bool purgeSets { get; set; }
        public bool resetBrowser { get; set; }
        public bool exportNwc { get; set; }
        public bool exportIFC { get; set; }
        public bool exportDWFx { get; set; }
        public bool saveLocal { get; set; }
        

        public linkforset(RevitLinkInstance rli)
        {
            RLI = rli;
            Name = rli.Name.Split(':').First();
            create3D = false;
            deleteWIP = false;
            removeCad = false;
            removeLinks = false;
            populateSections = false;
            purgeFilters = false;
            purgeSets = false;
            resetBrowser = false;
            exportNwc = false;
            exportIFC = false;
            exportDWFx = false;
            saveLocal = false;

        }

    }
}
