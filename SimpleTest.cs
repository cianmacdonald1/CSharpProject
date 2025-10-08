using System;

namespace GalacticCommander
{
    class SimpleTest
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing .NET Runtime...");
            Console.WriteLine($".NET Version: {Environment.Version}");
            Console.WriteLine($"OS: {Environment.OSVersion}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}