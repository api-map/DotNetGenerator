﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Apimap.DotnetGenerator.Core.Model;
using Apimap.DotnetGenerator.Core.Model.CodeGeneration;

namespace Apimap.DotnetGenerator.Core
{
    public class TypeResolver
    {
        public void Resolve(Dictionary<int, TypeMapping> existingTypeMappings, Type rootSourceType, Type rootTargetType, SchemaItem rootSourceSchemaItem, SchemaItem rootTargetSchemaItem,
            Mapping mapping)
        {
            foreach (var itemMapping in mapping.SchemaItemMappings)
            {
                if (itemMapping.SourceSchemaItemId.HasValue)
                {
                    if (itemMapping.SourceSchemaItem.IsRoot)
                    {
                        // TODO - not sure what to do here. There won't be any property path if the item is the root
                    }
                    else
                    {
                        itemMapping.SourcePath = PathManipulation.GetClrPropertyPathFromPath(rootSourceType, itemMapping.SourceSchemaItem.Path, 1);
                    }
                }

                if (!existingTypeMappings.ContainsKey(itemMapping.TargetSchemaItem.key))
                {
                    List<PropertyInfo> targetPath = null;
                    if (!itemMapping.TargetSchemaItem.IsRoot)
                    {
                        targetPath = PathManipulation.GetClrPropertyPathFromPath(rootTargetType, itemMapping.TargetSchemaItem.Path, 1);
                    }
                    existingTypeMappings.Add(itemMapping.TargetSchemaItem.key, new TypeMapping { TargetPath = targetPath, Mappings = new List<SchemaItemMapping>() });
                }
                existingTypeMappings[itemMapping.TargetSchemaItem.key].Mappings.Add(itemMapping);
            }

            if (!existingTypeMappings.ContainsKey(rootTargetSchemaItem.key))
            {
                var rootTm = new TypeMapping()
                {
                    Mappings = new List<SchemaItemMapping>() {new SchemaItemMapping() {SourceSchemaItem = rootSourceSchemaItem, SourceSchemaItemId = rootSourceSchemaItem.key, TargetSchemaItem = rootTargetSchemaItem, TargetSchemaItemId = rootTargetSchemaItem.key, Action = new SchemaItemMappingAction() {Id = MappingAction.Transform } } }
                };
                existingTypeMappings.Add(rootTargetSchemaItem.key, rootTm);
            }

        }
    }
}