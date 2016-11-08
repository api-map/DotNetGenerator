using System;
using Apimap.DotnetGenerator.Core.Model;
using Apimap.DotnetGenerator.Core.Model.CodeGeneration;

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
    }
}
