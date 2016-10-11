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
        public PropertyTraversalPath SourcePath { get; set; }

        public PropertyInfo SourceProperty
        {
            get { return SourcePath?.Property; } 
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

    public class PropertyTraversalPath
    {
        public Type RootType { get; set; }
        public List<PropertyTraversal> Path { get; set; }
        public PropertyInfo Property => Path != null && Path.Any() ? Path.Last().Property : null;

        public List<PropertyTraversal> SubPath(PropertyTraversalPath super)
        {
            if (super.Path == null || super.Path.Count == 0)
            {
                return Path;
            }

            return Path.Skip(super.Path.Count).ToList();
        }

        public string Name
        {
            get
            {
                if (Property == null)
                {
                    return "Source";
                }
                return Property.Name;
            }
        }

        public string TypeName
        {
            get
            {
                if (Property == null)
                {
                    return RootType.FullName;
                }
                return Property.PropertyType.FullName;
            }
        }

        public Type Type
        {
            get
            {
                if (Property == null)
                {
                    return RootType;
                }
                return Property.PropertyType;
            }
        }
    }

    public class PropertyTraversal
    {
        public PropertyInfo Property { get; set; }
        public bool IsArray { get; set; }
    }
}
