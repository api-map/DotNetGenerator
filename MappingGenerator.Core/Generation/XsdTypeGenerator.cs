using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization;
using Apimap.DotnetGenerator.Core.Model;
using Apimap.DotnetGenerator.Core.Model.CodeGeneration;
using Microsoft.CodeAnalysis;

namespace Apimap.DotnetGenerator.Core.Generation
{
    public class XsdTypeGenerator : ITypeGenerator
    {
        public CodeGenerationResult Generate(PhysicalSchema schema /* Maybe this should take a logical schema instead to help work out the root elements? */, string targetNamespace)
        {
            var code = CreateCodeNamespace(schema, targetNamespace);

            var provider = new Microsoft.CSharp.CSharpCodeProvider();

            var sb = new StringBuilder();

            using (StringWriter sw = new StringWriter(sb))
            {
                provider.CreateGenerator().GenerateCodeFromNamespace(code.Code, sw, new CodeGeneratorOptions());
            }

            var result = new CodeGenerationResult() {Code = sb.ToString()};

            TypeGenerator.BuildGeneratedCode(result, schema.Files[0].FileName /* TODO */, new MetadataReference[0]);
            result.RootTypeName = code.RootElementName;
            return result;
        }

        internal static CodeNamespaceResult CreateCodeNamespace(PhysicalSchema schema, string targetNamespace)
        {
            XmlSchemaSet xset = new XmlSchemaSet();

            foreach (var file in schema.Files)
            {
                var sr = new StringReader(file.Content);
                xset.Add(XmlSchema.Read(sr, null));
            }
            
            xset.Compile();
            XmlSchemas schemas = new XmlSchemas();
            foreach (XmlSchema xmlSchema in xset.Schemas())
            {
                schemas.Add(xmlSchema);
            }

            XmlSchemaImporter importer = new XmlSchemaImporter(schemas);
            
            var ns = new CodeNamespace(targetNamespace);
            var exporter = new XmlCodeExporter(ns);
            
            var result = new CodeNamespaceResult();
            foreach (XmlSchemaElement element in xset.GlobalElements.Values)
            {
                XmlTypeMapping mapping = importer.ImportTypeMapping(element.QualifiedName);

                if (string.IsNullOrEmpty(result.RootElementName))
                {
                    result.RootElementName = mapping.TypeName;
                }

                exporter.ExportTypeMapping(mapping);
            }

            result.Code = ns;
            return result;
        }

        internal class CodeNamespaceResult
        {
            internal CodeNamespace Code;
            internal string RootElementName;
        }
    }
}
