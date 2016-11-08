using System.IO;
using System.Linq;
using Apimap.DotnetGenerator.Core.Generation;
using Xunit;
using Xunit.Abstractions;

namespace Apimap.DotnetGenerator.Core.Test
{
    public class TypeGeneratorTest:TestBase
    {
        public TypeGeneratorTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void TypesCanBeGeneratedFromPersonSchema()
        {
            var tg = new JsonTypeGenerator();
            var result = tg.Generate(File.ReadAllText(TestFiles.Person), TestFiles.Person, "Foo.Bar");
            Assert.Null(result.Errors);
            Assert.NotNull(result.Assembly);
            Assert.NotNull(result.AssemblyBytes);
            Assert.False(string.IsNullOrEmpty(result.Code));
            Assert.Equal("person.dll", result.AssemblyName);

            Assert.Equal("Person", result.RootTypeName);

            Assert.NotNull(result.Assembly.DefinedTypes.First(a => a.Name == "Person"));
            Assert.NotNull(result.Assembly.DefinedTypes.First(a => a.Name == "Company"));
            Assert.NotNull(result.Assembly.DefinedTypes.First(a => a.Name == "Car"));

            Write(result.Code);
        }
    }
}
