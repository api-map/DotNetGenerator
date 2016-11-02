using System.Collections.Generic;
using System.IO;

namespace Apimap.DotnetGenerator
{
    public class Options 
    {
        public int? MappingId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string OutputDirectory { get; set; }

        public List<string> Validate()
        {
            var errors = new List<string>();

            if (!MappingId.HasValue)
            {
                errors.Add("You must specify the Id for the mapping you want to generate code for.");
            }

            if (string.IsNullOrEmpty(UserName))
            {
                errors.Add("You must specify a user-name to connect to api-map with.");
            }

            if (string.IsNullOrEmpty(Password))
            {
                errors.Add("You must specify a password to connect to api-map with");
            }

            if (string.IsNullOrEmpty(OutputDirectory))
            {
                errors.Add($"You must specify an output directory for generated code to be written to.");
            }
            else if (!Directory.Exists(OutputDirectory))
            {
                errors.Add($"Error: the output directory you specified '{OutputDirectory}' does not exist.");
            }

            return errors;
        }
    }
}
