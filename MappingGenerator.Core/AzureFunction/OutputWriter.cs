using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apimap.DotnetGenerator.Core.Model.CodeGeneration;

namespace Apimap.DotnetGenerator.Core.AzureFunction
{
    public class OutputWriter
    {
        public void WriteOutput(string path, string mappingClass, string entryPoint, List<CodeGenerationResult> generationResults)
        {
            if (!Directory.Exists(path))
            {
                throw new ArgumentException($"Path {path} does not exist");
            }

            if (generationResults == null || !generationResults.Any())
            {
                throw new ArgumentException("No code generation results where provided to be written out.");
            }

            WriteOutReferencedAssemblies(path, generationResults);
            WriteFunctionJson(path);

            File.WriteAllText(Path.Combine(path, "run.csx"), entryPoint);
            File.WriteAllText(Path.Combine(path, "mapper.csx"), mappingClass);
        }

        private void WriteFunctionJson(string path)
        {
            #region RawJson
            var functionJson = @"{
    ""bindings"": [
        {
            ""type"": ""httpTrigger"",
            ""name"": ""req"",
            ""direction"": ""in"",
            ""methods"": [ ""post"" ],
            ""authLevel"" :  ""anonymous"" 
        },
        {
            ""type"": ""http"",
            ""name"": ""res"",
            ""direction"": ""out""
        }
    ]
}";
            #endregion

            File.WriteAllText(Path.Combine(path, "function.json"), functionJson);
        }

        private static void WriteOutReferencedAssemblies(string path, List<CodeGenerationResult> generationResults)
        {
            var binPath = Path.Combine(path, "bin");
            if (!Directory.Exists(binPath))
            {
                Directory.CreateDirectory(binPath);
            }

            foreach (var code in generationResults)
            {
                var fileName = Path.Combine(binPath, code.AssemblyName);
                File.WriteAllBytes(fileName, code.AssemblyBytes.ToArray());
            }
        }
    }
}
