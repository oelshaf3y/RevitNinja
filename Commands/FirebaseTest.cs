using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FirebaseAdmin;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Google.Apis.Auth.OAuth2;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class FirebaseTest : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        IFirebaseConfig config;
        IFirebaseClient client;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            string jsonPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk\\Revit\\Addins\\2022\\fbadmin.json");
            //doc.print(jsonPath);
            //try
            //{
            //    JsonCredentialParameters jp = new JsonCredentialParameters()
            //    {
            //        Path = jsonPath,
            //        Scopes = new[] { "https://www.googleapis.com/auth/firebase.database" }

            //    };
            //    FirebaseApp.Create(new AppOptions()
            //    {
            //        Credential = GoogleCredential.FromJsonParameters(),
            //        ProjectId = "revitninjadb",
            //    });
            //}
            //catch (Exception ex)
            //{
            //    //doc.print(ex.Message);
            //}

            
            //var boo= client.Get("status");
            //PushResponse response = await client.PushAsync("todos/push", todo);
            //doc.print(boo.Body);

            return Result.Succeeded;
        }
    }
}
