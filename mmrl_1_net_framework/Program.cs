using System;

namespace mmrl_1_net_framework
{
    class Program
    {
        static void Main(string[] args)
        {
            var mwScanner = new MetawearScanner();
            mwScanner.StartScanning();
            Console.WriteLine("Press key x to stop scanning. Current scan results:");

            char key;
            do
            {
                key = Console.ReadKey().KeyChar;
            } while (key != 'x');

            Console.WriteLine("");
            mwScanner.StopScanning();

            Console.WriteLine("Results:");
            var scanResults = mwScanner.GetScanResults();
            foreach (var address in scanResults)
            {
                Console.WriteLine(address);
            }
        }
    }
}
