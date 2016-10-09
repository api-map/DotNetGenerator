using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Apimap.DotnetGenerator.Core.Model
{
    public class SchemaItemMapping
    {
        public int? Id { get; set; }
        public int TargetSchemaItemId { get; set; }
        public int? SourceSchemaItemId { get; set; }
        public int Rank { get; set; }
        public string Description { get; set; }
        public string Condition { get; set; }
        public SchemaItemMappingAction Action { get; set; }
        public DateTime Created { get; set; }

        [JsonIgnore]
        public List<PropertyInfo> SourcePath { get; set; }

        public PropertyInfo SourceProperty
        {
            get { return SourcePath != null && SourcePath.Any() ? SourcePath.Last() : null; } 
        }

        [JsonIgnore]
        public SchemaItem SourceSchemaItem { get; set; }

        [JsonIgnore]
        public SchemaItem TargetSchemaItem { get; set; }
    }

    public class SchemaItemMappingAction
    {
        public string Description { get; set; }
        public MappingAction Id { get; set; }
    }

    public enum MappingAction
    {
        DirectMap = 1,
        Transform = 3
    }
}
