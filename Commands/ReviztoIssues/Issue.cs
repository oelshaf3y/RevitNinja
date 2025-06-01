using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Autodesk.Revit.UI;

namespace Revit_Ninja.Commands.ReviztoIssues
{
    public class Issue
    {
        public string Id { get; set; }
        public string SnapshotLink { get; set; }
        public string Date { get; set; }
        public string Reporter { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string Stamp { get; set; }
        public string Level { get; set; }
        public string GridLocation { get; set; }
        public string Zone { get; set; }
        public string StampTitle { get; set; }
        public string Position { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();

        [JsonConstructor]
        public Issue(string Id, string SnapshotLink, string Date, string Reporter, string Status, string Title, string Stamp,
            string Level, string GridLocation, string Zone, string StampTitle, string Position = null, List<Comment> Comments = null)
        {
            this.Id = Id;
            this.SnapshotLink = SnapshotLink;
            this.Date = Date;
            this.Reporter = Reporter;
            this.Status = Status;
            this.Title = Title;
            this.Stamp = Stamp;
            this.Level = Level;
            this.GridLocation = GridLocation;
            this.Zone = Zone;
            this.StampTitle = StampTitle;
            this.Position = Position;
            if (Comments != null) this.Comments = Comments;
            else this.Comments = new List<Comment>();
        }
        public string ToJson()
        {
            //var dict = this.ToDictionary();
            return JsonSerializer.Serialize(this); // or Formatting.None
        }
        public static Issue fromJson(string json)
        {
            return JsonSerializer.Deserialize<Issue>(json);
        }
    }

    public class Comment
    {
        public string Content { get; set; }
        public string Provider { get; set; }
        public string Date { get; set; }

        [JsonConstructor]
        public Comment(string Content, string Provider, string Date)
        {
            this.Content = Content;
            this.Provider = Provider;
            this.Date = Date;
        }
        public string ToJson()
        {
            return JsonSerializer.Serialize(this); // or Formatting.None
        }
        public static List<Comment> fromJson(string json)
        {
            return JsonSerializer.Deserialize<List<Comment>>(json);
        }

    }
}
