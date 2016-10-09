using System;
using System.Net;
using Apimap.DotnetGenerator.Core;

namespace Apimap.DotnetGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var gateway = new ApiMapGateway();
            var mapping = gateway.GetMapping(196, new NetworkCredential("username", "password")); // why can't we 'await' here
            mapping.Wait();
            Console.WriteLine(mapping.Result);
            Console.ReadLine();
        }
    }
}
