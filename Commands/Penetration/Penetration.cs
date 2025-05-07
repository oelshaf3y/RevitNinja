using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands.Penetration
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Penetration : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        Autodesk.Revit.Creation.Application app;
        IList<Level> levels;
        IList<Element> AllLinks, allLinksUnique, aLLWalls, ALLBEAMS, selectedLinks, ductsCablesConduitsPipes, ducts, cables, conduits, pipes, floors,
             nativeDucts, nativePipes, nativeCableTrays, nativeConduits, nativeFloors, nativeWalls, nativeBeams;
        IList<HostElement> hostElements;
        List<string> familysymbolnamesuniq, selDocsNames;
        List<double> diasDR;
        IList<Reference> references;
        StringBuilder sb;
        Options optt;
        int FSCount;
        FilteredWorksetCollector nativeWorkSets, LinkedWorksets;
        IList<Workset> worksets, nativeWorkset;
        IList<PenetratingElement> penetratingElements;
        WorksetsForm wsform;
        FilteredElementCollector allsymbols;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet element)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            //if (!doc.getAccess()) return Result.Failed; 

            app = commandData.Application.Application.Create;
            diasDR = new List<double>() { 15, 20, 25, 32, 40, 50, 75, 110, 160, 200, 250, 315, 355 };
            sb = new StringBuilder();
            optt = app.NewGeometryOptions();
            optt.ComputeReferences = true;
            allsymbols = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericModel);
            nativeDucts = new FilteredElementCollector(doc).OfClass(typeof(Duct)).ToList();
            nativePipes = new FilteredElementCollector(doc).OfClass(typeof(Pipe)).ToList();
            nativeCableTrays = new FilteredElementCollector(doc).OfClass(typeof(CableTray)).ToList();
            nativeConduits = new FilteredElementCollector(doc).OfClass(typeof(Conduit)).ToList();
            nativeFloors = new FilteredElementCollector(doc).OfClass(typeof(Floor)).ToList();
            nativeWalls = new FilteredElementCollector(doc).OfClass(typeof(Wall)).Where(x => ((Wall)x).Width >= (50 / 304.8)).ToList();
            nativeBeams = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).Where(x => x.Category.Name == "Structural Framing").ToList();
            nativeWorkSets = new FilteredWorksetCollector(doc);
            nativeWorkset = nativeWorkSets.ToList();


            //init lists
            floors = new List<Element>();
            aLLWalls = new List<Element>();
            ALLBEAMS = new List<Element>();
            ducts = new List<Element>();
            cables = new List<Element>();
            pipes = new List<Element>();
            conduits = new List<Element>();



            familysymbolnamesuniq = allsymbols.Cast<FamilySymbol>().Select(fi => fi.Family.Name).ToList();

            levels = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().OrderBy(x => x.Elevation).ToList();
            AllLinks = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).ToList();


            if (AllLinks.Count == 0)
            {
                doc.print("No Links Loaded!!");
                //return Result.Failed;
            }
            PenetrationForm form = new PenetrationForm(AllLinks.Select(x => doc.GetElement(x.GetTypeId()).Name).Distinct().ToList(),
                allsymbols.Cast<FamilySymbol>().ToList(), familysymbolnamesuniq, doc);

            allLinksUnique = new List<Element>();
            foreach (Element link in AllLinks)
            {
                if (!allLinksUnique.Contains(link))
                {
                    allLinksUnique.Add(link);
                }
            }
            form.ShowDialog();

            if (form.cont == false) { return Result.Cancelled; }
            hostElements = new List<HostElement>();
            ductsCablesConduitsPipes = new List<Element>();
            if (form.checkBox2.IsChecked == true)
            {
                foreach (Element elem in nativeFloors)
                {
                    hostElements.Add(new HostElement(elem, null));
                }
                foreach (Element elem in nativeWalls)
                {
                    hostElements.Add(new HostElement(elem, null));
                }
                foreach (Element elem in nativeBeams)
                {
                    hostElements.Add(new HostElement(elem, null));
                }
                //native structural elements collection
            }
            if (form.checkBox3.IsChecked == true)
            {
                foreach (Element elem in nativeDucts) ductsCablesConduitsPipes.Add(elem);
                foreach (Element elem in nativePipes) ductsCablesConduitsPipes.Add(elem);
                foreach (Element elem in nativeConduits) ductsCablesConduitsPipes.Add(elem);
                foreach (Element elem in nativeCableTrays) ductsCablesConduitsPipes.Add(elem);


            }
            bool byworkset = form.radioButton1.IsChecked == true ? true : false;

            selectedLinks = new List<Element>();
            selDocsNames = new List<string>();
            foreach (int i in form.linksinds)
            {
                string linkPath = null;
                linkPath = ((RevitLinkInstance)allLinksUnique[i]).GetLinkDocument()?.PathName?.Trim();
                if (linkPath == null) continue;
                selectedLinks.Add(allLinksUnique[i]);
                selDocsNames.Add(linkPath);
            }

            Type type = null;
            worksets = nativeWorkSets.ToList().Concat(getlinkedWorksets()).ToList();
            if (byworkset)
            {
                wsform = new WorksetsForm(worksets, allsymbols.Cast<FamilySymbol>().ToList());
                wsform.ShowDialog();
                if (!wsform.state) return Result.Cancelled;
            }
            else
            {
                string elementType = form.comboBox3.SelectedItem.ToString();
                switch (elementType)
                {
                    case "Pipe":
                        type = typeof(Pipe);
                        break;
                    case "Duct":
                        type = typeof(Duct);
                        break;
                    case "Cable Tray":
                        type = typeof(CableTray);
                        break;
                    case "Conduit":
                        type = typeof(Conduit);
                        break;
                }
            }

            if (form.sel)
            {
                try
                {
                    if (byworkset)
                    {
                        references = uidoc.Selection.PickObjects(ObjectType.LinkedElement, new selectionFilter(), "Select Elements in RVT Link");
                    }
                    else
                    {
                        references = uidoc.Selection.PickObjects(ObjectType.LinkedElement, new selectionFilter(x => x.GetType() == type), "Select Elements in RVT Link");

                    }

                }
                catch
                {
                    return Result.Failed;
                }

                foreach (Reference reference in references)
                {
                    Document linkedDoc = ((RevitLinkInstance)doc.GetElement(reference.ElementId)).GetLinkDocument();
                    if (!selDocsNames.Contains(linkedDoc.PathName.Trim())) continue;
                    Element linkedElement = linkedDoc.GetElement(reference.LinkedElementId);
                    string linkedTypeName = doc.GetElement(((RevitLinkInstance)doc.GetElement(reference.ElementId)).GetTypeId()).Name;
                    if (linkedElement is Pipe || linkedElement is Duct || linkedElement is CableTray || linkedElement is Conduit)
                    {
                        if (!byworkset)
                        {
                            if (linkedElement.GetType() != type) continue;
                            ductsCablesConduitsPipes.Add(linkedElement);
                        }
                        else
                        {
                            ductsCablesConduitsPipes.Add(linkedElement);
                        }
                    }
                    else if (linkedElement is Floor || linkedElement is Wall || linkedElement.Category.Name == "Structural Framing" || linkedElement.Category.Name == "Structural Columns")
                    {
                        RevitLinkInstance rli = (RevitLinkInstance)doc.GetElement(reference.ElementId);
                        hostElements.Add(new HostElement(linkedElement, rli));
                    }
                }
            }
            else
            {
                getLinkedElements();

            }
            #region getting element information
            penetratingElements = new List<PenetratingElement>();
            for (int i = 0; i < ductsCablesConduitsPipes.Count; i++)
            {
                Element elem = ductsCablesConduitsPipes.ElementAt(i); //element
                LocationCurve elementLocationCurve = ((LocationCurve)elem.Location);
                Curve elementCurve = null;
                if (elementLocationCurve != null) { elementCurve = elementLocationCurve.Curve; }
                else { continue; }
                double elementWidth = 0, elementHeight = 0, insulationThickness = 0;
                double diameter = 0;


                if (elem is Pipe || elem is Conduit)
                {
                    double outsideDiam = 0;
                    if (elem is Pipe)
                    {
                        outsideDiam = elem.LookupParameter("Outside Diameter").AsDouble() + 2 * (elem.LookupParameter("Insulation Thickness").AsDouble());
                    }
                    else
                    {
                        outsideDiam = elem.LookupParameter("Outside Diameter").AsDouble();
                    }

                    // loop in steel pipes 
                    for (int k = 0; k < diasDR.Count; k++)
                    {
                        double d = diasDR[k];
                        if (Math.Round(outsideDiam * 304.8) <= d)
                        {
                            if (Math.Round(outsideDiam * 304.8) == d)
                            {
                                if (k != diasDR.Count - 1)
                                {
                                    diameter = diasDR[k + 1] / 304.8;
                                }
                                else
                                {
                                    diameter = d / 304.8;
                                }
                            }
                            else
                            {
                                diameter = d / 304.8;
                            }
                            break;
                        }
                    }
                    penetratingElements.Add(new PenetratingElement(elem, null, diameter, diameter, 0, elementCurve, null));

                }
                else
                {
                    try
                    {
                        elementWidth = elem.LookupParameter("Width").AsDouble();
                        elementHeight = elem.LookupParameter("Height").AsDouble();
                    }
                    catch { continue; }
                    try
                    {
                        insulationThickness = elem.LookupParameter("Insulation Thickness").AsDouble();
                    }
                    catch { }

                    penetratingElements.Add(new PenetratingElement(elem, null, elementWidth, elementHeight, insulationThickness, elementCurve, null));
                }
                //if (familySymbol == null)
                //{
                //    familySymbol = FSS[0];//familysymbol
                //}
            }

            #endregion

            if (byworkset)
            {
                foreach (PenetratingElement penElement in penetratingElements)
                {
                    Tuple<Workset, FamilySymbol> tup = wsform.worksetCollection.Where(x => x.Item1.Id == penElement.element.WorksetId).FirstOrDefault();
                    if (tup != null)
                    {
                        penElement.workset = tup.Item1;
                        penElement.familySymbol = tup.Item2;
                    }
                }
            }
            else
            {
                uidoc.Selection.SetElementIds(penetratingElements.Select(x => x.element.Id).ToList());
                //doc.print(penetratingElements.Count());
                //return Result.Succeeded;
                string famName = form.comboBox1.SelectedItem.ToString();
                string symbName = form.comboBox2.SelectedItem.ToString();
                FamilySymbol famSymbol = allsymbols.Cast<FamilySymbol>().Where(x => (x.Family.Name == famName) && (x.Name == symbName)).FirstOrDefault();
                foreach (PenetratingElement penElement in penetratingElements.Where(x => x.element.GetType() == type).ToList())
                {
                    penElement.familySymbol = famSymbol;
                }
            }
            //td(penetratingElements.Count.ToString());
            //return Result.Succeeded;
            return penetrate();

        }

        Result penetrate()
        {
            //ProgressBar progressBarForm = new ProgressBar();
            //progressBarForm.progresBarRatio.Minimum = 0;
            //progressBarForm.progresBarRatio.Maximum = penetratingElements.Count;
            //progressBarForm.progresBarRatio.Step = 1;
            //progressBarForm.Lb.Text = "0 / " + penetratingElements.Count;
            //progressBarForm.Show();

            List<XYZ> locs = new List<XYZ>();

            #region transaction group
            TransactionGroup tg = new TransactionGroup(doc);
            tg.Start("Penetration");

            #region transaction
            using (Transaction tr = new Transaction(doc, "Pentration"))
            {
                tr.Start();
                #region activate family symbol
                foreach (FamilySymbol famSymb in penetratingElements.Select(x => x.familySymbol).ToList())
                {
                    famSymb?.Activate();
                }
                #endregion

                foreach (PenetratingElement penetratingElement in penetratingElements)
                {
                    if (penetratingElement.familySymbol == null)
                    {

                        continue;
                    }
                    foreach (HostElement hostElement in hostElements)
                    {
                        Element elem = hostElement.element;

                        Solid so = getSolid(elem);


                        //#region progress bar update
                        //if (so == null)
                        //{
                        //    progressBarForm.Lb.Text = $"Progress: {hostElements.IndexOf(hostElement)}  / {hostElements.Count}(Task {penetratingElements.IndexOf(penetratingElement)} of {penetratingElements.Count})";
                        //    progressBarForm.progresBarRatio.PerformStep();
                        //    progressBarForm.progresBarRatio.Refresh();
                        //    progressBarForm.Refresh();
                        //    continue;
                        //}
                        //#endregion

                        XYZ sleeveDir = null;
                        Face face = null;
                        double depth = 0;
                        XYZ locationPt = null;
                        XYZ dir = null;
                        SolidCurveIntersectionOptions scio = new SolidCurveIntersectionOptions();
                        scio.ResultType = SolidCurveIntersectionMode.CurveSegmentsInside;
                        SolidCurveIntersection solidIntersectCurve = null;
                        try
                        {
                            solidIntersectCurve = so.IntersectWithCurve(penetratingElement.axis, scio);
                        }
                        catch { }
                        if (solidIntersectCurve == null) { continue; }
                        else
                        {
                            if (solidIntersectCurve.SegmentCount > 0)
                            {
                                #region get depth of penetration
                                Curve cc = solidIntersectCurve.GetCurveSegment(0);
                                if (elem is Wall)
                                {
                                    depth = ((Wall)elem).Width; //depth
                                }
                                else if (elem is Floor)
                                {
                                    depth = elem.LookupParameter("Thickness").AsDouble();//depth
                                }
                                else
                                {
                                    depth = cc.Length;//depth
                                }
                                if (cc is Line)
                                {
                                    if (Math.Round(Math.Abs(((Line)cc).Direction.Normalize().Z), 1) == 1) //Floor
                                    {
                                        sleeveDir = XYZ.BasisX;//sleeveDir
                                    }
                                    else //Wall
                                    {
                                        sleeveDir = XYZ.BasisZ;//sleeveDir
                                    }
                                }
                                #endregion

                                #region direction of penetration
                                XYZ intersectionStartPt = solidIntersectCurve.GetCurveSegment(0).GetEndPoint(0);
                                XYZ intersectionEndPt = solidIntersectCurve.GetCurveSegment(0).GetEndPoint(1);
                                XYZ curveStartPt = penetratingElement.axis.GetEndPoint(0);
                                XYZ curveEndPt = penetratingElement.axis.GetEndPoint(1);
                                if (intersectionStartPt.X != curveStartPt.X && intersectionStartPt.Y != curveStartPt.Y
                                    && intersectionStartPt.X != curveEndPt.X && intersectionStartPt.Y != curveEndPt.Y)
                                {
                                    locationPt = intersectionStartPt;//locationPoint
                                    dir = Line.CreateBound(intersectionStartPt, intersectionEndPt).Direction.Normalize();
                                }
                                else
                                {
                                    locationPt = intersectionEndPt;//locationPoint
                                    dir = Line.CreateBound(intersectionEndPt, intersectionStartPt).Direction.Normalize();
                                }
                                #endregion

                                #region get intersection face
                                foreach (Face f in so.Faces)
                                {
                                    IntersectionResultArray ira = new IntersectionResultArray();
                                    SetComparisonResult scr = f.Intersect(penetratingElement.axis, out ira);
                                    if (ira != null)
                                    {
                                        if (!ira.IsEmpty)
                                        {
                                            face = f;
                                            break;
                                        }
                                    }
                                }
                                #endregion

                            }
                        }

                        if (face != null && penetratingElement.familySymbol != null)
                        {
                            if (face.Reference != null)
                            {
                                Reference faceRef = null;
                                if (hostElement.rli != null)
                                {

                                    faceRef = face.Reference.CreateLinkReference(hostElement.rli);//reference
                                }
                                else
                                {
                                    faceRef = face.Reference;
                                }
                                try
                                {
                                    bool found = false;
                                    foreach (XYZ point in locs)
                                    {
                                        if (Math.Round(point.X, 2) == Math.Round(locationPt.X, 2)
                                            && Math.Round(point.Y, 2) == Math.Round(locationPt.Y, 2) & Math.Round(point.Z, 2) == Math.Round(locationPt.Z, 2))
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (!found)
                                    {
                                        FamilyInstance familyInstance = doc.Create.NewFamilyInstance(faceRef, locationPt, sleeveDir, penetratingElement.familySymbol);
                                        if (familyInstance != null)
                                        {
                                            if (penetratingElement.element is Pipe || penetratingElement.element is Conduit)
                                            {
                                                familyInstance.LookupParameter("Diameter").Set(penetratingElement.width);
                                            }
                                            else
                                            {
                                                familyInstance.LookupParameter("b").Set(penetratingElement.width + 2 * (penetratingElement.insulationThickness + (50 / 304.8)));
                                                familyInstance.LookupParameter("h").Set(penetratingElement.height + 2 * (penetratingElement.insulationThickness + (50 / 304.8)));
                                                familyInstance.Location.Rotate(((Line)penetratingElement.axis), Math.PI / 2);
                                            }
                                            FSCount++;
                                            locs.Add(locationPt);
                                            familyInstance.LookupParameter("Depth").Set(depth + (50 / 304.8));
                                            familyInstance.LookupParameter("Schedule Level").Set(ElevationSleeve(locationPt).Id);
                                            familyInstance.LookupParameter("Comments").Set(ElevationSleeve(locationPt).Name.ToString());
                                            WorksetTable wst = doc.GetWorksetTable();
                                            if (nativeWorkset.Where(x => x.Id.IntegerValue == penetratingElement.workset.Id.IntegerValue)?.FirstOrDefault() != null)
                                            {
                                                Workset ws = nativeWorkset.Where(x => x.Id.IntegerValue == penetratingElement.workset.Id.IntegerValue)?.FirstOrDefault();
                                                if (ws.Name == penetratingElement.workset.Name)
                                                {
                                                    if (wst.GetActiveWorksetId() != penetratingElement.workset.Id) wst.SetActiveWorksetId(penetratingElement.workset.Id);
                                                    familyInstance.LookupParameter("Workset").Set(penetratingElement.workset.Id.IntegerValue);
                                                }
                                                else
                                                {
                                                    Workset ws2 = Workset.Create(doc, wsform.worksetCollection.Where(x => x.Item1.Id == penetratingElement.workset.Id).ToList().Select(x => x.Item1).FirstOrDefault().Name);
                                                    if (wst.GetActiveWorksetId() != ws2.Id) wst.SetActiveWorksetId(ws2.Id);
                                                    familyInstance.LookupParameter("Workset").Set(ws2.Id.IntegerValue);
                                                    nativeWorkset.Add(ws2);
                                                    doc.print($"created workset {ws2.Name}");
                                                }
                                            }
                                            else
                                            {
                                                Workset ws = Workset.Create(doc, wsform.worksetCollection.Where(x => x.Item1.Id == penetratingElement.workset.Id).ToList().Select(x => x.Item1).FirstOrDefault().Name);
                                                if (wst.GetActiveWorksetId() != ws.Id) wst.SetActiveWorksetId(ws.Id);
                                                familyInstance.LookupParameter("Workset").Set(ws.Id.IntegerValue);
                                                nativeWorkset.Add(ws);
                                                doc.print($"created workset {ws.Name}");
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                    //doc.print(ex.Message);
                                }
                            }
                        }
                        //progressBarForm.Lb.Text = $"Progress: {hostElements.IndexOf(hostElement)}  / {hostElements.Count}(Task {penetratingElements.IndexOf(penetratingElement)} of {penetratingElements.Count})";
                        //progressBarForm.progresBarRatio.PerformStep();
                        //progressBarForm.progresBarRatio.Refresh();
                        //progressBarForm.Refresh();
                    }
                }
                tr.Commit();
            }

            #endregion

            //progressBarForm.Close();
            doc.print("Finished !!" + "\n" + "All voids count is:  " + FSCount);

            tg.Assimilate();
            #endregion

            return Result.Succeeded;

        }

        private List<Workset> getlinkedWorksets()
        {
            List<Workset> worksets = new List<Workset>();
            foreach (Element linkInstance in selectedLinks)
            {
                RevitLinkInstance rli = linkInstance as RevitLinkInstance;
                Document linkedDoc = rli.GetLinkDocument();
                LinkedWorksets = new FilteredWorksetCollector(linkedDoc);
                if (worksets.Count == 0)
                {
                    worksets = LinkedWorksets.ToList();
                    //td(worksets.Count.ToString());
                }
                else
                {
                    worksets = worksets.Concat(LinkedWorksets.ToList()).ToList();
                }
            }
            //td(worksets.Count.ToString());
            return worksets;
        }



        private void getLinkedElements()
        {
            foreach (Element linkInstance in selectedLinks)
            {
                RevitLinkInstance rli = linkInstance as RevitLinkInstance;
                Document linkedDoc = rli.GetLinkDocument();
                if (linkedDoc != null)
                {

                    new FilteredElementCollector(linkedDoc).OfClass(typeof(Floor)).ToList().ForEach(x => floors.Add(x));
                    new FilteredElementCollector(linkedDoc).OfClass(typeof(Wall)).Where(x => ((Wall)x).Width >= (50 / 304.8)).ToList().ForEach(x => aLLWalls.Add(x));
                    new FilteredElementCollector(linkedDoc).OfClass(typeof(FamilyInstance)).Where(x => x.Category.Name == "Structural Framing").ToList().ForEach(x => ALLBEAMS.Add(x));
                    new FilteredElementCollector(linkedDoc).OfClass(typeof(Duct)).ToList().ForEach(x => ducts.Add(x));
                    new FilteredElementCollector(linkedDoc).OfClass(typeof(CableTray)).ToList().ForEach(x => cables.Add(x));
                    new FilteredElementCollector(linkedDoc).OfClass(typeof(Conduit)).ToList().ForEach(x => conduits.Add(x));
                    new FilteredElementCollector(linkedDoc).OfClass(typeof(Pipe)).ToList().ForEach(x => pipes.Add(x));
                    foreach (Element e in ducts)
                    {
                        ductsCablesConduitsPipes.Add(e);
                    }
                    foreach (Element e in cables)
                    {
                        ductsCablesConduitsPipes.Add(e);
                    }
                    foreach (Element e in pipes)
                    {
                        ductsCablesConduitsPipes.Add(e);
                    }
                    foreach (Element e in conduits)
                    {
                        ductsCablesConduitsPipes.Add(e);
                    }
                    foreach (Element el in aLLWalls)
                    {
                        hostElements.Add(new HostElement(el, rli));
                    }
                    foreach (Element el in ALLBEAMS)
                    {
                        hostElements.Add(new HostElement(el, rli));
                    }
                    foreach (Element el in floors)
                    {
                        hostElements.Add(new HostElement(el, rli));
                    }
                }
                else
                {
                    doc.print($"linked document {linkedDoc.PathName} is not loaded");
                }
            }
        }


        Level ElevationSleeve(XYZ Z)
        {
            int ilvl = 0; Level lv = null;
            foreach (Level lvl in levels)
            {
                if (lvl.Elevation - Z.Z >= 0)
                {
                    if (ilvl == 0)
                    {
                        lv = levels[0];
                        break;
                    }
                    else
                    {
                        lv = levels[ilvl - 1];
                        break;
                    }
                }
                ilvl++;
            }
            if (lv == null) { lv = levels.Last(); }
            return lv;
        }

        bool isIntersected(IList<PenetratingElement> elements, Element penetratedElement)
        {
            Solid so = getSolid(penetratedElement);
            if (so == null) return false;
            SolidCurveIntersectionOptions scio = new SolidCurveIntersectionOptions();
            scio.ResultType = SolidCurveIntersectionMode.CurveSegmentsInside;
            SolidCurveIntersection solidIntersectCurve = null;
            foreach (PenetratingElement element in elements)
            {

                try
                {
                    solidIntersectCurve = so.IntersectWithCurve(element.axis, scio);
                }
                catch { return false; }
                if (solidIntersectCurve != null)
                {
                    if (solidIntersectCurve.SegmentCount > 0)
                    {
                        Curve cc = solidIntersectCurve.GetCurveSegment(0);
                        return true;
                    }
                }
            }
            return false;
        }
        private Solid getSolid(Element elem)
        {
            GeometryElement gele = elem.get_Geometry(optt);
            foreach (GeometryObject geo in gele)
            {
                Solid g = geo as Solid;
                if (g != null && g.Volume != 0)
                {
                    return g;
                }
            }
            return null;
        }
    }

}
