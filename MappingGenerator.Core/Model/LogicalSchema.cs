using System.Collections.Generic;

namespace Apimap.DotnetGenerator.Core.Model
{
    public class LogicalSchema : SchemaInfo
    {
        public List<SchemaItem> Roots { get; set; }

        public PhysicalSchema PhysicalSchema { get; set; }

        public string Errors { get; set; }

        public void RebuildParentRelationships()
        {
            foreach (var hierarchyItem in Roots)
            {
                RebuildRelationship(hierarchyItem, null);
            }
        }

        public List<SchemaInfo> SupercededBy { get; set; }

        public int? SupercedesId { get; set; }

        public int? ServiceDefinitionId { get; set; }

        private void RebuildRelationship(SchemaItem schemaItem, SchemaItem parent)
        {
            schemaItem.Parent = parent;
            if (schemaItem.children == null)
            {
                schemaItem.children = new List<SchemaItem>();
            }

            foreach (var child in schemaItem.children)
            {
                RebuildRelationship(child, schemaItem);
            }
        }

    }
}
