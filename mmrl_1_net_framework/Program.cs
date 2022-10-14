using System;

namespace mmrl_1_net_framework
{
    class Program
    {
        static void Main(string[] args)
        {
            var mwScanner = new MetawearScanner();
            mwScanner.StartScanning();
            Console.WriteLine("Press any key to stop scanning.");
            mwScanner.StopScanning();

            Console.WriteLine("Scan results:");
            var scanResults = mwScanner.GetScanResults();
            foreach (var address in scanResults)
            {
                Console.WriteLine(address);
            }
        }
    }
}
