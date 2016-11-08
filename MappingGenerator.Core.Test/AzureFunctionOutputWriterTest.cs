using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Apimap.DotnetGenerator.Core.AzureFunction;
using Apimap.DotnetGenerator.Core.Generation;
using Apimap.DotnetGenerator.Core.Model;
using Apimap.DotnetGenerator.Core.Model.CodeGeneration;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Apimap.DotnetGenerator.Core.Test
{
    public class AzureFunctionOutputWriterTest : TestBase
    {
        [Fact]
        public void CanGenerateCodeForNestedMapping()
        {
            var tg = new TypeGenerator();

            var mapping = JsonConvert.DeserializeObject<Mapping>(File.ReadAllText(TestBase.TestFiles.AToXMapping));

            var source = tg.Generate(mapping.SourceInfo.PhysicalSchema, "SourceNs");
            var target = tg.Generate(mapping.TargetInfo.PhysicalSchema, "TargetNs");

            output.WriteLine(source.Code);

            mapping.RebuildRelationships();
            var mappings = new Dictionary<int, TypeMapping>();

            var sourceItem = mapping.SourceInfo.Roots[0];
            var targetItem = mapping.TargetInfo.Roots[0];

            var resolver = new TypeResolver();
            resolver.Resolve(mappings, source.RootType, target.RootType, sourceItem, targetItem, mapping);

            var generator = new Generator();
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            generator.Generate(writer, mappings, source.RootType, target.RootType, mapping.TargetInfo.Roots[0], mapping);

            var entrypointGen = new EntryPointGenerator();

            var azureWriter = new OutputWriter();
            var codeGen = new List<CodeGenerationResult>() {source, target};
            var basePath =
                @"I:\AzureFunctions\src\azure-webjobs-sdk-script\sample\HttpTrigger-MappingDemo"; // @"F:\temp\azure";
            azureWriter.WriteOutput(basePath, sb.ToString(), entrypointGen.Generate(source.RootType, target.RootType, codeGen), codeGen);

            Assert.True(File.Exists(Path.Combine(basePath, "bin\\Source.dll")));
            Assert.True(File.Exists(Path.Combine(basePath, "bin\\Target.dll")));
            Assert.True(File.Exists(Path.Combine(basePath, "function.json")));
            Assert.True(File.Exists(Path.Combine(basePath, "run.csx")));



        }

        public AzureFunctionOutputWriterTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}
