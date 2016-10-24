using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Apimap.DotnetGenerator.Core.Model;
using Apimap.DotnetGenerator.Core.Model.CodeGeneration;
using NJsonSchema.CodeGeneration;

namespace Apimap.DotnetGenerator.Core
{
    public class Generator
    {

        public void Generate(TextWriter output, Dictionary<int, TypeMapping> typeMappings, Type rootSourceType, Type rootTargetType, SchemaItem rootTargetItem, Mapping mapping)
        {
            var tm = typeMappings[rootTargetItem.key];
            tm.MappingMethod = new GeneratedMethod()
            {
                Name = "Map",
                ReturnType = rootTargetType,
                Parameters = GetMappingParameters(tm)
            };

            tm.MappingMethod.AppendMethodBodyCode($"var target = new {rootTargetType}();");

            IterateOverChildren(rootTargetItem, typeMappings, rootSourceType, tm.MappingMethod);

            WriteClassBeginning(tm, output);

            tm.MappingMethod.Render(output, "\t");

            foreach (var typeMappingsValue in typeMappings.Values)
            {
                if (typeMappingsValue != tm)
                {
                    if (typeMappingsValue.MappingMethod != null)
                    {
                        typeMappingsValue.MappingMethod.Render(output, "\t");
                    }
                    else
                    {
                        if (!CanBeDirectAssignment(typeMappingsValue))
                        {
                            throw new InvalidOperationException($"Unless the method is direct assignment there should be a method body. No body found for {typeMappingsValue.TargetItem.title}");
                        }
                    }
                }
            }

            WriteClassEnding(output);
        }

        

        private void WriteClassBeginning(TypeMapping tm, TextWriter writer)
        {
            writer.WriteLine("public class Mapper");
            writer.WriteLine("{");
        }

        private void WriteClassEnding(TextWriter output)
        {
            output.WriteLine("}");
        }

        private void IterateOverChildren(SchemaItem item, Dictionary<int, TypeMapping> typeMappings, Type rootSourceType, GeneratedMethod parentMethod)
        {
            foreach (var child in item.children)
            {
                GenerateMappingForItem(typeMappings, child, parentMethod, rootSourceType);
            }
        }

        private void GenerateMappingForItem(Dictionary<int, TypeMapping> typeMappings, SchemaItem targetItem, GeneratedMethod parentMethod, Type rootSourceType)
        {
            if (typeMappings.ContainsKey(targetItem.key))
            {
                var tm = typeMappings[targetItem.key];

                if (IsSimpleDefault(tm))
                {
                    // item is defaulted - generate defaulting method
                    tm.MappingMethod = new GeneratedMethod()
                    {
                        Name = GenerateDefaultingMethodName(tm),
                        ReturnType = tm.TargetProperty.PropertyType
                    };

                    parentMethod.AppendMethodBodyCode($"target.{tm.TargetProperty.Name} = {tm.MappingMethod.Name}();");
                }
                else
                {
                    if (CanBeDirectAssignment(tm))
                    {
                        var propPath = BuildPropertyPath(tm.Mappings.First().SourcePath, parentMethod.Parameters); // first is valid here, because to be a valid direct assignment (like highlander) there can be only one
                        parentMethod.AppendMethodBodyCode($"target.{tm.TargetProperty.Name} = {propPath};");
                    }
                    else
                    {
                        tm.MappingMethod = new GeneratedMethod()
                        {
                            Name = GenerateMappingMethodName(tm),
                            Parameters = GetMappingParameters(tm),
                            ReturnType = tm.TargetProperty.PropertyType
                        };
                        tm.MappingMethod.AppendMethodBodyCode($"var target = new {tm.TargetProperty.PropertyType}();");
                        var parameters = string.Join(", ", tm.Mappings.Select(a => BuildPropertyPath(a.SourcePath, parentMethod.Parameters)));
                        parentMethod.AppendMethodBodyCode($"target.{tm.TargetProperty.Name} = {tm.MappingMethod.Name}({parameters});");
                    }

                }

                IterateOverChildren(targetItem, typeMappings, rootSourceType, tm.MappingMethod);
            }
            else
            {
                // a child might be mapped but not the parent - TODO
                IterateOverChildren(targetItem, typeMappings, rootSourceType, parentMethod);
            }
        }

        private List<PropertyTraversalPath> GetMappingParameters(TypeMapping tm)
        {
            return tm.Mappings.Select(a => a.SourcePath).ToList();
        }

        private string BuildPropertyPath(PropertyTraversalPath sourcePath, List<PropertyTraversalPath> parameters)
        {
            if (sourcePath == null)
            {
                return "source";
            }

            var parameter = GetParameterThatIsSubPath(sourcePath, parameters);

            var subPath = sourcePath.SubPath(parameter);

            // TODO - will need to be greatly expanded to handle arrays/collections/choice etc

            if (subPath == null || !subPath.Any())
            {
                return parameter.Name;
            }

            if (subPath.Any(a => a.IsArray))
            {
                // TODO
                return null;
            }

            return  parameter.Name + "?." + string.Join("?.", subPath.Select(a => a.Property.Name));
        }

        private PropertyTraversalPath GetParameterThatIsSubPath(PropertyTraversalPath sourcePath, List<PropertyTraversalPath> parameters)
        {
            var ordered = parameters.Where(a => a.Path != null).OrderByDescending(a => a.Path.Count);
            foreach (var parameter in ordered)
            {
                if (parameter.RootType == sourcePath.RootType && sourcePath.Path.Any(a => a.Property == parameter.Property))
                {
                    return parameter;
                }
            }

            return parameters.FirstOrDefault(a => a.RootType == sourcePath.RootType);
        }

        private bool CanBeDirectAssignment(TypeMapping tm)
        {
            if (tm.Mappings.Count > 1 || tm.Mappings.Any(a => a.Action.Id == MappingAction.Transform))
            {
                return false;
            }
            var source = tm.Mappings.FirstOrDefault(a => a.SourcePath != null);
            return source != null && (source.SourcePath.Path == null || !source.SourcePath.Path.Any(a => a.IsArray)) && tm.TargetPath.Type.IsAssignableFrom(source.SourcePath.Type);
        }

        private bool IsSimpleDefault(TypeMapping tm)
        {
            return tm.Mappings.All(a => a.SourceSchemaItem == null);
        }

        private string GenerateMappingMethodName(TypeMapping tm)
        {
            var startPart = string.Join("And", tm.Mappings.Select(a => GetClrItemName(a.SourceSchemaItem.title)));
            return "Map" + startPart + "To" + GetClrItemName(tm.TargetItem.title); 
        }

        private string GenerateDefaultingMethodName(TypeMapping tm)
        {
            return "Default" + GetClrItemName(tm.TargetItem.title);
        }

        private string GetClrItemName(string schemaItemTitle)
        {
            if (string.IsNullOrEmpty(schemaItemTitle) || schemaItemTitle == "(no title)")
            {
                return "NoTitle";
            }

            return ConversionUtilities.ConvertToUpperCamelCase(schemaItemTitle, true);
        }

    }
}
