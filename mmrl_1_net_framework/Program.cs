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
            if(mwScanner.ScanResults.Count > 0)
            {
                var firstBoardMac = mwScanner.ScanResults.First<ulong>();

                MetaWearBoardsManager boardsManager = new MetaWearBoardsManager();
                var connectingTask = boardsManager.ConnectToBoard(firstBoardMac);
                await connectingTask;
                if (connectingTask.Result == 0)
                {
                    Console.WriteLine("Successfully conected.");
                }
                Console.WriteLine("Wait some moments and then press any key to disconnect again...");
                Console.ReadKey();
                if(boardsManager.DisconnectBoard(firstBoardMac) == 0)
                {
                    Console.WriteLine("Successfully disconnected.");
                }
            }
        }
    }
}
