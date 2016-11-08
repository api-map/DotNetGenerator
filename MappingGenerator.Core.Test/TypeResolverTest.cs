using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apimap.DotnetGenerator.Core.Generation;
using Apimap.DotnetGenerator.Core.Model;
using Apimap.DotnetGenerator.Core.Model.CodeGeneration;
using Newtonsoft.Json;
using NJsonSchema.CodeGeneration;
using Xunit;
using Xunit.Abstractions;

namespace Apimap.DotnetGenerator.Core.Test
{
    public class TypeResolverTest : TestBase
    {
        public TypeResolverTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CanResolveTypesForSourceAndTarget()
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
            var sourcetype = source.Assembly.DefinedTypes.First(a => a.Name == ConversionUtilities.ConvertToUpperCamelCase(sourceItem.title, true));
            var targettype = target.Assembly.DefinedTypes.First(a => a.Name == ConversionUtilities.ConvertToUpperCamelCase(targetItem.title, true));

            var resolver = new TypeResolver();
            resolver.Resolve(mappings, sourcetype, targettype, sourceItem, targetItem , mapping);
            Assert.Equal(4, mappings.Keys.Count);

            foreach (var tm in mappings.Values)
            {
                foreach (var schemaItemMapping in tm.Mappings)
                {
                    Write(string.Format("Item {0} of type {1} will be mapped from {2}", schemaItemMapping.TargetSchemaItem.title, tm.TargetProperty, schemaItemMapping.SourceProperty));
                }
            }

            var tm1 = mappings[targetItem.children.First(a => a.title == "firstName").key];
            Assert.Equal(typeof(System.String), tm1.Mappings[0].SourceProperty.PropertyType);
            Assert.NotNull(tm1.Mappings[0].SourcePath);

            var tm2 = mappings[targetItem.children.First(a => a.title == "lastName").key];
            Assert.Equal(typeof(System.String), tm2.Mappings[0].SourceProperty.PropertyType);
            Assert.NotNull(tm2.Mappings[0].SourcePath);
        }

        [Fact]
        public void CanResolveTypesForSourceAndTargetAToX()
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

            Assert.Equal(7, mappings.Keys.Count);

            var wTargetItemId = 33309;
            var wTypeMapping = mappings[wTargetItemId];

            Assert.Equal(1, wTypeMapping.Mappings.Count);
            Assert.NotNull(wTypeMapping.Mappings[0].SourcePath);

            foreach (var tm in mappings.Values)
            {
                foreach (var schemaItemMapping in tm.Mappings)
                {
                    Write(string.Format("Item {0} of type {1} will be mapped from {2}", schemaItemMapping.TargetSchemaItem.title, tm.TargetProperty, schemaItemMapping.SourceProperty));
                    Assert.NotNull(schemaItemMapping.SourcePath);
                }
            }
        }
    }
}
