using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]  
    internal class Firebase : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            IFirebaseConfig config = new FirebaseConfig
            {
                AuthSecret = "BOclA-K_R4anRz3PX_nc7yd3V5Mzy7ic0CGA98LodIwr_KqrCnURU3am6GoJlUlIZHGQnZglVAKzT8hS2HCNhOg",
                BasePath= "https://revitninja-default-rtdb.firebaseio.com/"
            };

            IFirebaseClient client = new FirebaseClient(config);
            //var boo= client.Get("status");
            //PushResponse response = await client.PushAsync("todos/push", todo);
            //doc.print(boo.Body);

            return Result.Succeeded;
        }
    }
}
