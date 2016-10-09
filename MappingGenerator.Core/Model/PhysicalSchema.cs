using System;
using System.Collections.Generic;

namespace Apimap.DotnetGenerator.Core.Model
{
    public class PhysicalSchema
    {
        public List<SchemaFile> Files { get; set; }
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
}
