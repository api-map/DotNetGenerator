using Mono.Options;

namespace Apimap.DotnetGenerator
{
    public class Options 
    {
        public int MappingId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string OutputDirectory { get; set; }
    }
}
