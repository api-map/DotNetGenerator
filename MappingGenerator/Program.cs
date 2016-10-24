using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Apimap.DotnetGenerator.Core;
using Apimap.DotnetGenerator.Core.AzureFunction;
using Apimap.DotnetGenerator.Core.Model;
using Apimap.DotnetGenerator.Core.Model.CodeGeneration;
using Mono.Options;

namespace Apimap.DotnetGenerator
{
    class Program
    {
        static int Main(string[] args)
        {
            var opt = new Options();
            var optionset = new OptionSet()
            {
                { "m|mapping=", "the id of the mapping to generate code for", (int id) => opt.MappingId = id },
                { "u|user=", "your api-map user name", user => opt.UserName = user },
                { "p|password=", "your api-map user's password", pwd => opt.Password = pwd },
                { "o|output=", "output directory for generated code", outdir => opt.OutputDirectory = outdir },
            };

            List<string> extra;
            try
            {
                extra = optionset.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine($"Error: Unable to read options - {e.Message}");
                return -1;
            }

            if (!Directory.Exists(opt.OutputDirectory))
            {
                Console.WriteLine($"Error: the output directory you specified {opt.OutputDirectory} does not exist.");
                return -1;
            }

            Mapping mapping = null;

            var gateway = new ApiMapGateway();
            try
            {
                var mappingTask = gateway.GetMapping(opt.MappingId, new NetworkCredential(opt.UserName, opt.Password)); // why can't we 'await' here
                mappingTask.Wait();
                mapping = mappingTask.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Unable to retrieve mapping - {ex.Message}");
                return -1;
            }

            if (mapping == null)
            {
                Console.WriteLine($"Error: Unable to retrieve mapping {opt.MappingId}.");
                return -1;
            }

            return GenerateMappint(mapping, opt);

        }

        private static int GenerateMappint(Mapping mapping, Options opt)
        {
            var tg = new TypeGenerator();

            var source = tg.Generate(mapping.SourceInfo.PhysicalSchema.Files.First().Content, mapping.SourceInfo.PhysicalSchema.Files.First().FileName, "SourceNs");
            var target = tg.Generate(mapping.TargetInfo.PhysicalSchema.Files.First().Content, mapping.TargetInfo.PhysicalSchema.Files.First().FileName, "TargetNs");
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
            var codeGen = new List<CodeGenerationResult>() { source, target };
            azureWriter.WriteOutput(opt.OutputDirectory, sb.ToString(), entrypointGen.Generate(source.RootType, target.RootType, codeGen), codeGen);

            return 0;
        }
    }
}
