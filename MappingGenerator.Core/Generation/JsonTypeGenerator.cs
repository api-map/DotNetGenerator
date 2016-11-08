using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Apimap.DotnetGenerator.Core.Model;
using Apimap.DotnetGenerator.Core.Model.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;

namespace Apimap.DotnetGenerator.Core.Generation
{
    public class JsonTypeGenerator : ITypeGenerator
    {
        private const string VersionInfo =
    "\n[assembly: AssemblyVersion(\"1.0.0.0\")]\n[assembly: AssemblyFileVersion(\"1.0.0.0\")]\n";

        public CodeGenerationResult Generate(PhysicalSchema schema, string targetNamespace)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            if (string.IsNullOrEmpty(targetNamespace))
            {
                throw new ArgumentException(nameof(targetNamespace));
            }

            if (schema.Files == null || schema.Files.Count < 1)
            {
                throw new ArgumentException("json schema file was not provided", nameof(schema));
            }

            return Generate(schema.Files[0].Content, schema.Files[0].FileName, targetNamespace);
        }

        public CodeGenerationResult Generate(string jsonSchema, string jsonFileName, string targetNamespace)
        {
            if (string.IsNullOrEmpty(jsonSchema))
            {
                throw new ArgumentException("json schema was not provided", nameof(jsonSchema));
            }

            var rootname = GetDefaultRootItemNameFromFileName(jsonFileName);
            var typeNameGen = new CustomTypeNameGenerator(rootname);
            var schema = JsonSchema4.FromJson(jsonSchema);
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings() { Namespace = targetNamespace, ClassStyle = CSharpClassStyle.Poco, RequiredPropertiesMustBeDefined = false, TypeNameGenerator = typeNameGen, ArrayType = "List" });

            var result = new CodeGenerationResult { Code = generator.GenerateFile() };

            AddAssemblyVersionAttributes(result, targetNamespace);

            // this calling back to the TypeGenerator sure gives these a 'bas type/sub-type' kind of vibe.
            TypeGenerator.BuildGeneratedCode(result, rootname, CreateMetadataReferences());

            result.RootTypeName = typeNameGen.AssignedRootTypeName;

            return result;
        }



        private void AddAssemblyVersionAttributes(CodeGenerationResult result, string targetNamespace)
        {
            var ns = "namespace " + targetNamespace;
            result.Code = result.Code.Replace(ns, VersionInfo + ns); // pretty horrible
        }

        private string GetDefaultRootItemNameFromFileName(string jsonFileName)
        {
            if (string.IsNullOrEmpty(jsonFileName) || jsonFileName.IndexOf(".json") < 0)
            {
                return null;
            }

            var partial = Path.GetFileName(jsonFileName);

            if (partial.ToLowerInvariant().EndsWith(".json"))
            {
                return partial.Substring(0, partial.Length - 5);
            }

            return null;
        }

        private static MetadataReference[] CreateMetadataReferences()
        {
            MetadataReference[] references = {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(JsonSerializer).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(GeneratedCodeAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(RequiredAttribute).Assembly.Location), // only reason this is needed is because of 'Required' attributes which we probably won't use anyway
            };
            return references;
        }

        internal class CustomTypeNameGenerator : ITypeNameGenerator
        {
            private string defaultRootName = null;
            private string assignedRootTypeName = null;

            public CustomTypeNameGenerator(string defaultRootName)
            {
                this.defaultRootName = defaultRootName;
            }

            public string AssignedRootTypeName
            {
                get { return assignedRootTypeName; }
            }

            public string Generate(JsonSchema4 schema)
            {
                var result = GenerateInternal(schema);

                if (schema.ParentSchema == null)
                {
                    assignedRootTypeName = result;
                }

                return result;
            }

            private string GenerateInternal(JsonSchema4 schema)
            {
                if (!string.IsNullOrEmpty(schema.TypeNameRaw))
                {
                    return ConversionUtilities.ConvertToUpperCamelCase(schema.TypeNameRaw, true);
                }

                if (schema.ExtensionData != null && schema.ExtensionData.Any() && schema.ExtensionData.ContainsKey("typeName"))
                {
                    return ConversionUtilities.ConvertToUpperCamelCase(schema.ExtensionData["typeName"].ToString(), true);
                }

                if (!string.IsNullOrEmpty(schema.Title))
                {
                    return ConversionUtilities.ConvertToUpperCamelCase(schema.Title, true);
                }

                if (schema.ParentSchema == null)
                {
                    return defaultRootName;
                }

                return null;
            }
        }
    }
}
