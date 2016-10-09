using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Apimap.DotnetGenerator.Core.Model.CodeGeneration
{
    public class CodeGenerationResult
    {
        public string Code { get; set; }
        public Assembly Assembly { get; set; }
        public List<string> Errors { get; set; }
        public MemoryStream AssemblyBytes { get; set; }
        public string AssemblyName { get; set; }
        public string RootTypeName { get; set; }

        public Type RootType
        {
            get
            {
                if (Assembly == null || RootTypeName == null)
                {
                    return null;
                }

                return Assembly.DefinedTypes.FirstOrDefault(a => a.Name == RootTypeName);
            }
        }
    }
}
