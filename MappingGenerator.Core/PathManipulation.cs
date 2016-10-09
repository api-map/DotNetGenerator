using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Apimap.DotnetGenerator.Core.Model;

namespace Apimap.DotnetGenerator.Core
{
    public class PathManipulation
    {
        public static List<PropertyInfo> GetClrPropertyPathFromPath(Type type, List<SchemaItem> path, int index)
        {
            return GetClrPropertyPathFromPathInternal(type, path, index, new List<PropertyInfo>());
        }

        private static List<PropertyInfo> GetClrPropertyPathFromPathInternal(Type type, List<SchemaItem> path, int index, List<PropertyInfo> properties)
        {
            Log("Working out path " + PathAsString(path));

            var item = path[index];
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            Log("Looking for property named " + item.title);

            var prop = props.FirstOrDefault(a => a.Name.Equals(item.title, StringComparison.InvariantCultureIgnoreCase));
            if (prop != null)
            {
                properties.Add(prop);

                if (index == path.Count - 1)
                {
                    return properties;
                }
                else
                {
                    return GetClrPropertyPathFromPathInternal(prop.PropertyType, path, index + 1, properties);
                }
            }

            Log("No Property Found");
            return null; // TODO

            // TODO - special case 'choice'
        }

        private static string PathAsString(List<SchemaItem> path)
        {
            return string.Join(" / ", path.Select(a => a.title));
        }

        private static void Log(string info)
        {
            System.Diagnostics.Debug.WriteLine(info);
        }
    }
}
