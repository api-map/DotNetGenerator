using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Apimap.DotnetGenerator.Core.Extension;

namespace Apimap.DotnetGenerator.Core.Model.CodeGeneration
{
    public class TypeMapping
    {
        public PropertyInfo TargetProperty { get { return TargetPath != null && TargetPath.Path != null && TargetPath.Path.Any() ? TargetPath.Path.Last().Property : null; } } // TODO - assume is always property?

        public PropertyTraversalPath TargetPath { get; set; }

        public List<SchemaItemMapping> Mappings { get; set; } 
        public GeneratedMethod MappingMethod { get; set; }

        public SchemaItem TargetItem
        {
            get { return Mappings != null && Mappings.Any() ? Mappings.First().TargetSchemaItem : null; } // all of the target items for a particular TypeMapping should be the same
        }
    }

    public class GeneratedMethod
    {
        private int indentation = 1;
        private string indentationString = "\t";

        public string Name { get; set; }

        public List<PropertyTraversalPath> Parameters { get; set; }
        public Type ReturnType { get; set; }

        private List<string> codeLines = new List<string>();

        public void AppendMethodBodyCode(string content, IndentationChange indent)
        {
            UpdateIndentation(indent);
            codeLines.Add(indentationString + content) ;
        }

        public void AppendMethodBodyCode(string content)
        {
            this.AppendMethodBodyCode(content, IndentationChange.None);
        }

        private void UpdateIndentation(IndentationChange indent)
        {
            switch (indent)
            {
                case IndentationChange.None:
                    break;

                case IndentationChange.Increase:
                    indentation ++;
                    break;

                case IndentationChange.Decrease:
                    indentation --;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(indent), indent, null);
            }
            indentationString = "";
            indentation.Times(() => indentationString += "\t");
        }

        public void Render(TextWriter output, string indentation)
        {
            string parameters = null;
            if (Parameters != null && Parameters.Any())
            {
                parameters = string.Join(",", Parameters.Select(a => a.TypeName + " " + a.Name));
            }
            output.WriteLine(indentation + $"public virtual {ReturnType.FullName} {Name}({parameters})");
            output.WriteLine(indentation + "{");
            if (!codeLines.Any())
            {
                output.WriteLine(indentation + "\tthrow new NotImplementedException();");
            }
            else
            {
                foreach (var codeLine in codeLines)
                {
                    output.WriteLine(indentation + codeLine);
                }
                output.WriteLine(indentation + "\treturn target;");
            }
            output.WriteLine(indentation + "}");
            output.WriteLine("");
        }
    }

    public enum IndentationChange
    {
        None,
        Increase,
        Decrease,
    }
}
