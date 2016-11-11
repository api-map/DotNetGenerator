using Xunit.Abstractions;

namespace Apimap.DotnetGenerator.Core.Test
{
    public class TestBase
    {
        protected ITestOutputHelper output;

        public TestBase(ITestOutputHelper output)
        {
            this.output = output;
        }

        protected void Write(string info)
        {
            output.WriteLine(info);
        }

        internal class TestFiles
        {
            internal const string Person = "Schemas\\person.json";
            internal const string PersonBasic = "Schemas\\person-basic.json";
            internal const string PersonMapping = "Schemas\\person_to_person-basic.json";
            internal const string AToXMapping = "Schemas\\a-to-x.json";
            internal const string AToXVariantMapping = "Schemas\\a-to-x-variant.json";
            internal const string JsonToXsd = "Schemas\\json-to-xsd.json";

        }
    }
}
