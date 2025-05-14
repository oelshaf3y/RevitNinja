using System.Reflection;
using Newtonsoft.Json;

namespace Revit_Ninja.Commands.ReviztoIssues
{
    public class Issue
    {
        public string Id { get; set; }
        public string SnapshotLink { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string Stamp { get; set; }
        public string Level { get; set; }
        public string GridLocation { get; set; }
        public string Zone { get; set; }
        public string StampTitle { get; set; }
        public string Position { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            PropertyInfo[] properties = typeof(Issue).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(this);
                dict.Add(property.Name, value ?? string.Empty); // Handle null values
            }

            return dict;
        }
        public string ToJson()
        {
            var dict = this.ToDictionary();
            return JsonConvert.SerializeObject(dict, Formatting.Indented); // or Formatting.None
        }
    }

    public class Comment
    {
        public string Content { get; set; }
        public string Provider { get; set; }
        public string Date { get; set; }

        public Comment(string cont,string provider,string date)
        {
            this.Content = cont;
            this.Provider=provider;
            this.Date = date;

        }
        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            PropertyInfo[] properties = typeof(Comment).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(this);
                dict.Add(property.Name, value ?? string.Empty); // Handle null values
            }

            return dict;
        }
        public string ToJson()
        {
            var dict = this.ToDictionary();
            return JsonConvert.SerializeObject(dict, Formatting.Indented); // or Formatting.None
        }
        public static List<Comment> fromJson(string json)
        {
            var li = JsonConvert.DeserializeObject<List<Comment>>(json);
            //var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return li;
        }

    }
}
