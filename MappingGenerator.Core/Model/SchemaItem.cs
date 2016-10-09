using System.Collections.Generic;
using Newtonsoft.Json;

namespace Apimap.DotnetGenerator.Core.Model
{
    public class SchemaItem
    {
        public string title { get; set; }

        public int key { get; set; }

        public string type { get; set; }

        public Occurrance Occurrance { get; set; }

        public string ExampleValue { get; set; }

        public List<SchemaItem> children { get; set; }

        public string description { get; set; }

        public SchemaItem Parent { get; set; }

        public bool IsRoot { get { return Parent == null; } }

        [JsonIgnore]
        public List<SchemaItem> Path
        {
            get
            {
                var path = new List<SchemaItem>();
                AppendPath(path);
                path.Reverse();
                return path;
            }
        }

        private void AppendPath(List<SchemaItem> path)
        {
            path.Add(this);
            if (this.Parent != null)
            {
                Parent.AppendPath(path);
            }
        }

        public int? SchemaId { get; set; }
    }


    public class Occurrance
    {
        public int Min { get; set; }
        public int? Max { get; set; } 

        public string Description()
        {
            var desc = Min + " to ";
            if (Max == null)
            {
                desc += " many";
            }
            else
            {
                if (Max == 1 && Min == 1)
                {
                    desc = "mandatory";
                }
                else if (Max == 1 && Min == 0)
                {
                    desc = "optional";
                }
                else
                {
                    desc += Max;
                }
            }

            return desc;
        }
    }
}
