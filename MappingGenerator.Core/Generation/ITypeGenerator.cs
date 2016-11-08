using Apimap.DotnetGenerator.Core.Model;
using Apimap.DotnetGenerator.Core.Model.CodeGeneration;

namespace Apimap.DotnetGenerator.Core.Generation
{
    public interface ITypeGenerator
    {
        CodeGenerationResult Generate(PhysicalSchema schema, string targetNamespace);
    }
}
