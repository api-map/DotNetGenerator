using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using Apimap.DotnetGenerator.Core.Model.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;

namespace Apimap.DotnetGenerator.Core
{
    public class TypeGenerator
    {
        private const string VersionInfo =
            "\n[assembly: AssemblyVersion(\"1.0.0.0\")]\n[assembly: AssemblyFileVersion(\"1.0.0.0\")]\n";

        public CodeGenerationResult Generate(string jsonSchema, string jsonFileName, string targetNamespace)
        {
            if (string.IsNullOrEmpty(jsonSchema))
            {
                throw new ArgumentException("json schema was not provided", nameof(jsonSchema));
            }

            var rootname = GetDefaultRootItemNameFromFileName(jsonFileName);
            var typeNameGen = new CustomTypeNameGenerator(rootname);
            var schema = JsonSchema4.FromJson(jsonSchema);
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings() {Namespace = targetNamespace, ClassStyle = CSharpClassStyle.Poco, RequiredPropertiesMustBeDefined = false, TypeNameGenerator = typeNameGen, ArrayType = "List" });

            var result = new CodeGenerationResult {Code = generator.GenerateFile() };

            AddAssemblyVersionAttributes(result, targetNamespace);

            var st = CSharpSyntaxTree.ParseText(result.Code);

            var fileName = rootname;
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            CSharpCompilation compilation = CSharpCompilation.Create(
               fileName,
               syntaxTrees: new[] { st },
               references: CreateMetadataReferences(),
               options: options);

            result.AssemblyBytes = new MemoryStream();
            EmitResult emitted = compilation.Emit(result.AssemblyBytes);

            if (!emitted.Success)
            {
                result.Errors = new List<string>();

                IEnumerable<Diagnostic> failures = emitted.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                foreach (Diagnostic diagnostic in failures)
                {
                    result.Errors.Add(string.Format("{0}: {1}", diagnostic.Id, diagnostic.GetMessage()));
                }
            }
            else
            {
                result.AssemblyBytes.Seek(0, SeekOrigin.Begin);
                result.Assembly = Assembly.Load(result.AssemblyBytes.ToArray());
                result.AssemblyBytes.Seek(0, SeekOrigin.Begin);
            }

            result.RootTypeName = typeNameGen.AssignedRootTypeName;
            result.AssemblyName = fileName + ".dll";

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
