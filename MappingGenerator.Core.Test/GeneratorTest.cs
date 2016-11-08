using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Apimap.DotnetGenerator.Core.Generation;
using Apimap.DotnetGenerator.Core.Model;
using Apimap.DotnetGenerator.Core.Model.CodeGeneration;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Apimap.DotnetGenerator.Core.Test
{
    public class GeneratorTest : TestBase
    {
        public GeneratorTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CanGenerateCodeForSimpleMapping()
        {
            var tg = new JsonTypeGenerator();
            var source = tg.Generate(File.ReadAllText(TestFiles.Person), TestFiles.Person, "Foo.Bar");
            var target = tg.Generate(File.ReadAllText(TestFiles.PersonBasic), TestFiles.PersonBasic, "Baz");

            Write(source.Code);
            Write(target.Code);

            var mapping = JsonConvert.DeserializeObject<Mapping>(File.ReadAllText(TestFiles.PersonMapping));
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

            var result = sb.ToString();
            Assert.True(result.Contains("target.FirstName = Source?.FirstName;"));
            Write(result);
        }

        [Fact]
        public void CanGenerateCodeForNestedMapping()
        {
            var tg = new TypeGenerator();

            var mapping = JsonConvert.DeserializeObject<Mapping>(File.ReadAllText(TestFiles.AToXMapping));

            var source = tg.Generate(mapping.SourceInfo.PhysicalSchema, "Mapping");
            var target = tg.Generate(mapping.TargetInfo.PhysicalSchema, "Mapping");

            Write(source.Code);
            Write(target.Code);

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

            Write(sb.ToString());
        }

        [Fact]
        public void CanGenerateCodeForVariantMapping()
        {
            var tg = new TypeGenerator();

            var mapping = JsonConvert.DeserializeObject<Mapping>(File.ReadAllText(TestFiles.AToXVariantMapping));

            var source = tg.Generate(mapping.SourceInfo.PhysicalSchema, "Source");
            var target = tg.Generate(mapping.TargetInfo.PhysicalSchema, "Target");

            Write(source.Code);
            Write(target.Code);

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

            var result = sb.ToString();
            Write(result);

            Assert.True(result.Contains("target.Z = MapNoTitleToZ(Source?.B);"), "Array was correctly passed as a parameter");
            Assert.True(result.Contains("public virtual Target.Z MapNoTitleToZ(IEnumerable<Source.Anonymous> B)"), "array was correctly added as method argument");
        }

    }
}
