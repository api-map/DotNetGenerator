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
                Parameters = new List<Arg>() {new Arg() {Name = "source", Type = rootSourceType } }
            };

            tm.MappingMethod.AppendMethodBodyCode($"var target = new {rootTargetType}();");

            IterateOverChildren(rootTargetItem, typeMappings);

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
                            throw new InvalidOperationException("Unless the method is direct assignment there should be a method body. ");
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

        private void IterateOverChildren(SchemaItem item, Dictionary<int, TypeMapping> typeMappings)
        {
            foreach (var child in item.children)
            {
                var tm = typeMappings[item.key];
                GenerateMappingForItem(typeMappings, child, tm.MappingMethod);
            }
        }

        private void GenerateMappingForItem(Dictionary<int, TypeMapping> typeMappings, SchemaItem targetItem, GeneratedMethod parentMethod)
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
                        var propPath = BuildPropertyPath(tm.Mappings.First().SourcePath);
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
                        var parameters = string.Join(", ", tm.Mappings.Select(a => BuildPropertyPath(a.SourcePath)));
                        parentMethod.AppendMethodBodyCode($"target.{tm.TargetProperty.Name} = {tm.MappingMethod.Name}({parameters});");
                        // TODO - generate method body here?
                    }

                }

                IterateOverChildren(targetItem, typeMappings);
            }
            else
            {
                // a child might be mapped but not the parent - TODO
            }
        }

        private List<Arg> GetMappingParameters(TypeMapping tm)
        {
            // TODO - need to do something aobut the case where the source of one of the mappings is the source root - there will be no property path
            return tm.Mappings.Where(b => b.SourceProperty != null).Select(a => new Arg() {Name = a.SourceProperty.Name, Type = a.SourceProperty.PropertyType}).ToList();
        }

        private string BuildPropertyPath(List<PropertyInfo> sourcePath)
        {
            if (sourcePath == null)
            {
                return "source";
            }

            // TODO - will need to be greatly expanded to handle arrays/collections/choice etc
            return "source?." + string.Join("?.", sourcePath.Select(a => a.Name));
        }

        private bool CanBeDirectAssignment(TypeMapping tm)
        {
            if (tm.Mappings.Count > 1 || tm.Mappings.Any(a => a.Action.Id == MappingAction.Transform))
            {
                return false;
            }
            var source = tm.Mappings.FirstOrDefault(a => a.SourcePath != null);
            return source != null && tm.TargetProperty.PropertyType.IsAssignableFrom(source.SourceProperty.PropertyType);
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
