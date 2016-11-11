using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Apimap.DotnetGenerator.Core.Model;

namespace Apimap.DotnetGenerator.Core
{
    public class PathManipulation
    {
        public static PropertyTraversalPath GetClrPropertyPathFromPath(Type type, List<SchemaItem> path, int index)
        {
            return GetClrPropertyPathFromPathInternal(type, path, index, new PropertyTraversalPath() {Path = new List<PropertyTraversal>(), RootType = type});
        }

        private static PropertyTraversalPath GetClrPropertyPathFromPathInternal(Type type, List<SchemaItem> path, int index, PropertyTraversalPath traversalPath)
        {
            Log("Working out path " + PathAsString(path));

            var item = path[index];

            var parent = path[index - 1];
            if (parent.Occurrance.Max == null)
            {
                if (index == path.Count - 1)
                {
                    return traversalPath;
                }
                else
                {
                    if (type.IsArray)
                    {
                        return GetClrPropertyPathFromPathInternal(type.GetElementType(), path, index + 1, traversalPath);
                    }
                    else
                    {
                        // assume a collection
                        return GetClrPropertyPathFromPathInternal(type.GetGenericArguments().First(), path, index + 1, traversalPath);
                    }
                }
            }
            else
            {
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Log("Looking for property named " + item.title); // "choose one of" == choice. Need to 

                var prop = props.FirstOrDefault(a => a.Name.Equals(item.title, StringComparison.InvariantCultureIgnoreCase));
                if (prop != null)
                {
                    return AddTraversal(path, index, traversalPath, prop);
                }

                // if there are clashes NJsonSchema adds a '1' etc to the end of the type name
                // TODO - consider searching for more numbers?
                prop = props.FirstOrDefault(a => a.Name.Equals(item.title + "1", StringComparison.InvariantCultureIgnoreCase));
                if (prop != null)
                {
                    return AddTraversal(path, index, traversalPath, prop);
                }

                throw new InvalidOperationException("traversal not found");

            }

            
        }

        private static PropertyTraversalPath AddTraversal(List<SchemaItem> path, int index, PropertyTraversalPath traversalPath,
            PropertyInfo prop)
        {
            traversalPath.Path.Add(new PropertyTraversal() {Property = prop});

            if (index == path.Count - 1)
            {
                return traversalPath;
            }
            else
            {
                return GetClrPropertyPathFromPathInternal(prop.PropertyType, path, index + 1, traversalPath);
            }
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
