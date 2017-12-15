using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Tweet_Bunny_Service
{
    class Program
    {
        ILogger logger;

        static void Main(string[] args)
        {
            var _t = RunApplication(args);
            _t.Wait();
        }

        static async public Task<bool> RunApplication(string[] args)
        {
            var apiKey = Environment.GetEnvironmentVariable("API_KEY");

            Console.WriteLine("Hello World!");
            return true;
        }
    }
}
