using System.Collections.Generic;
using System.Linq;

namespace Apimap.DotnetGenerator.Core.Model
{
    public class Mapping 
    {
        public LogicalSchema SourceInfo { get; set; }

        public LogicalSchema TargetInfo { get; set; }

        public List<SchemaItemMapping> SchemaItemMappings { get; set; }

        public string Description { get; set; }

        public int? Id { get; set; }

        public string Version { get; set; }

        public List<int> SupercededById { get; set; }

        public int? SupercedesId { get; set; }

        public string Title { get; set; }

        public bool IsRetired { get; set; }

        public void RebuildRelationships()
        {
            SourceInfo.RebuildParentRelationships();
            TargetInfo.RebuildParentRelationships();

            if (SchemaItemMappings != null && SchemaItemMappings.Any())
            {
                var allSchemaItems = new Dictionary<int, SchemaItem>();

                FlattenSchemaItems(this.SourceInfo.Roots, allSchemaItems);
                FlattenSchemaItems(this.TargetInfo.Roots, allSchemaItems);

                foreach (var im in SchemaItemMappings)
                {
                    if (im.SourceSchemaItemId != null)
                    {
                        im.SourceSchemaItem = allSchemaItems[im.SourceSchemaItemId.Value];
                    }
                    im.TargetSchemaItem = allSchemaItems[im.TargetSchemaItemId];
                }
            }

        }

        private void FlattenSchemaItems(IEnumerable<SchemaItem> items, Dictionary<int, SchemaItem> allSchemaItems)
        {
            foreach (var schemaItem in items)
            {
                if (!allSchemaItems.ContainsKey(schemaItem.key))
                {
                    allSchemaItems.Add(schemaItem.key, schemaItem);
                    if (schemaItem.children != null && schemaItem.children.Any())
                    {
                        FlattenSchemaItems(schemaItem.children, allSchemaItems);
                    }
                }
            }
        }
    }
}
