using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
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
                provider.CreateGenerator().GenerateCodeFromNamespace(code, sw, new CodeGeneratorOptions());
            }

            var result = new CodeGenerationResult() {Code = sb.ToString()};

            TypeGenerator.BuildGeneratedCode(result, schema.Files[0].FileName /* TODO */, new MetadataReference[0]);

            return result;
        }

        // mostly from here: https://msdn.microsoft.com/en-us/library/aa302301.aspx

        public static CodeNamespace CreateCodeNamespace(PhysicalSchema schema, string targetNamespace)
        {
            XmlSchema xsd;

            // TODO - iterate over all files, and use SchemaSet instead
            var sr = new StringReader(schema.Files.First().Content);
            xsd = XmlSchema.Read(sr, null);
            xsd.Compile(null); 

            XmlSchemas schemas = new XmlSchemas();
            schemas.Add(xsd);

            XmlSchemaImporter importer = new XmlSchemaImporter(schemas);
            
            var ns = new CodeNamespace(targetNamespace);
            var exporter = new XmlCodeExporter(ns);
            
            foreach (XmlSchemaElement element in xsd.Elements.Values)
            {
                // TODO - can this be used to determine 'root' elements better?
                XmlTypeMapping mapping = importer.ImportTypeMapping(element.QualifiedName);
                exporter.ExportTypeMapping(mapping);
            }

            return ns;
        }
    }
}
