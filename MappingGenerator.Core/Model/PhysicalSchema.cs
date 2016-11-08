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
            get { return Files != null && Files.Any() ? DefinitionType.Json : DefinitionType.Unknown; }
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
