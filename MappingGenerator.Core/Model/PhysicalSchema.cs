using System;
using System.Collections.Generic;
using System.Linq;

namespace Apimap.DotnetGenerator.Core.Model
{
    public class PhysicalSchema
    {
        public List<SchemaFile> Files { get; set; }

        public DefinitionType DefinitionType
        {
            get
            {
                if (Files == null || !Files.Any())
                {
                    return DefinitionType.Unknown;
                }

                if (Files.First().FileName.EndsWith(".xsd") || Files.First().FileName.EndsWith(".xml"))
                {
                    return DefinitionType.Xsd;
                }

                return DefinitionType.Json;
            }
        }
    }

    public class SchemaFile
    {
        public string FileName { get; set; }
        public int Size { get; set; }
        public string Content { get; set; }
        public DateTime Modified { get; set; }
        public int? Id { get; set; }
        public int SchemaId { get; set; }
    }

    public enum DefinitionType
    {
        Unknown,
        Json,
        Xsd
    }
}
