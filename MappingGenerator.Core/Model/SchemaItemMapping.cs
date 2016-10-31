using System;
using System.Collections;
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
                    return "Source"; // what about when it is to the target?
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
                if (IsArray)
                {
                    Type enumType = null;
                    // IEnumerable<T> will be safest?
                    if (Property.PropertyType.IsGenericType)
                    {
                        enumType = Property.PropertyType.GenericTypeArguments.First();
                    }
                    if (Property.PropertyType.IsArray)
                    {
                        enumType = Property.PropertyType.GetElementType();
                    }
                    return $"IEnumerable<{enumType.FullName}>";
                }
                else
                {
                    return Property.PropertyType.FullName;
                }
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

        public bool IsArray
        {
            get
            {
                if (Path == null || Path.Count == 0)
                {
                    return false;
                }

                return Path.Last().IsArray;
            }
        }
    }

    public class PropertyTraversal
    {
        public PropertyInfo Property { get; set; }

        public bool IsArray
        {
            get
            {
                if (Property == null)
                {
                    return false;
                }

                return Property.PropertyType.IsArray ||
                       (Property.PropertyType.IsGenericType && typeof(IList).IsAssignableFrom(Property.PropertyType));
            }
        }
    }
}
