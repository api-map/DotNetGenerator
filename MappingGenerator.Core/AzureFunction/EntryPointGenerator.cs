using System;
using System.Collections.Generic;
using System.Text;
using Apimap.DotnetGenerator.Core.Model.CodeGeneration;

namespace Apimap.DotnetGenerator.Core.AzureFunction
{
    public class EntryPointGenerator
    {
        public string Generate(Type rootSourceType, Type rootTargetType, List<CodeGenerationResult> generationResults)
        {
            var sb = new StringBuilder();
            sb.AppendLine("#r \"Newtonsoft.JSON\"");
            foreach (var codeGenerationResult in generationResults)
            {
                sb.AppendLine($"#r \"{codeGenerationResult.AssemblyName}\"");
            }
            sb.AppendLine("#load \"mapper.csx\"");

            WriteUsings(sb, generationResults);
            WriteRunMethod(sb, rootSourceType);

            return sb.ToString();
        }

        private void WriteRunMethod(StringBuilder sb, Type rootSourceType)
        {
            var code =
                $@"
    var source = {rootSourceType.FullName}.FromJson(req.Content.ReadAsStringAsync().Result);
    var mapper = new Mapper();
    var target = mapper.Map(source);
    var res = new HttpResponseMessage(HttpStatusCode.OK);
    res.Content = new StringContent(target.ToJson());

    return Task.FromResult(res);
";

            sb.AppendLine("public static Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)");
            sb.AppendLine("{");
            sb.AppendLine(code);
            sb.AppendLine("}");
        }

        private void WriteUsings(StringBuilder sb, List<CodeGenerationResult> generationResults)
        {
            var usings = @"using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Azure.WebJobs.Host;";

            sb.AppendLine(usings);

            foreach (var code in generationResults)
            {
                sb.AppendLine($"using {code.RootType.Namespace};");
            }
        }
    }
}
