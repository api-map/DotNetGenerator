using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Apimap.DotnetGenerator.Core.Model;
using Apimap.DotnetGenerator.Core.Model.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Apimap.DotnetGenerator.Core.Generation
{
    public class TypeGenerator : ITypeGenerator
    {
        private ITypeGenerator jsonGenerator = new JsonTypeGenerator();
        private ITypeGenerator xsdGenerator = new XsdTypeGenerator();

        public CodeGenerationResult Generate(PhysicalSchema schema, string targetNamespace)
        {
            if (schema == null) { throw new ArgumentNullException(nameof(schema));}

            if (string.IsNullOrEmpty(targetNamespace)) { throw new ArgumentException("Target namespace not provided"); }

            switch (schema.DefinitionType)
            {
                case DefinitionType.Unknown:
                    throw new ArgumentException(
                        "Unable to generate schema since the schema type is not known. This may be due to no schema files being present in the schema definition.");
                    break;

                case DefinitionType.Json:
                    return jsonGenerator.Generate(schema, targetNamespace);
                    break;

                case DefinitionType.Xsd:
                    return xsdGenerator.Generate(schema, targetNamespace);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("Schema type was unexpected");
            }
        }

        internal static CodeGenerationResult BuildGeneratedCode(CodeGenerationResult result, string assemblyName, MetadataReference[] references)
        {
            var st = CSharpSyntaxTree.ParseText(result.Code);

            var fileName = assemblyName;
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            CSharpCompilation compilation = CSharpCompilation.Create(
                fileName,
                syntaxTrees: new[] { st },
                references: references,
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


            result.AssemblyName = fileName + ".dll";
            return result;
        }
    }
}
