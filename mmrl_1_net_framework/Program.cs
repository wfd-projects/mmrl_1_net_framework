using System;
using System.Threading.Tasks;
using System.Linq;

namespace mmrl_1_net_framework
{
    class Program
    {
        static async Task Main(string[] args)
        {
            /***** Find all MetaWear boards. *****/
            var mwScanner = new MetaWearScanner();
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
            foreach (var address in mwScanner.ScanResults)
            {
                Console.WriteLine(MetaWearScanner.MacUlongToString(address));
            }

            /***** Just connect to the first board found. *****/
            var firstBoardMac = mwScanner.ScanResults.First<ulong>();

            MetaWearBoardsManager boardsManager = new MetaWearBoardsManager();
            await boardsManager.ConnectToBoard(firstBoardMac);
            Console.WriteLine("Wait some moments and then press any key to disconnect again...");
            Console.ReadKey();
            boardsManager.DisconnectBoard(firstBoardMac);
        }
    }
}
