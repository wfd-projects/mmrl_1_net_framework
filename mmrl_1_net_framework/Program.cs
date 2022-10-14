using System;
using System.Threading.Tasks;
using System.Linq;
using MbientLab.MetaWear.Data;

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

            if(mwScanner.ScanResults.Count > 0)
            {
                /***** Just connect to the first board found. *****/
                Console.WriteLine("Trying to connect to a board...");
                var firstBoardMac = mwScanner.ScanResults.First<ulong>();

                MetaWearBoardsManager boardsManager = new MetaWearBoardsManager();
                var connectingTask = boardsManager.ConnectToBoard(firstBoardMac);
                await connectingTask;
                var board = connectingTask.Result;
                if (board != null)
                {
                    Console.WriteLine("Successfully conected.");
                }
                Console.WriteLine("Wait some moments and then press any key to disconnect again...");
                Console.ReadKey();
                if(boardsManager.DisconnectBoard(board) == 0)
                {
                    Console.WriteLine("Successfully disconnected.");
                }

                /***** Stream acceleration data *****/
                Acceleration accData = new Acceleration(0, 0, 0);
                Console.WriteLine("Starting accelerometer data stream...");
                await boardsManager.StartAccelerometerStream(board, accData);
                Console.WriteLine("Press any key to stop the accelerometer.");
                Console.ReadKey();
                boardsManager.StopAccelerometerStream(board);
                Console.WriteLine("Stopped accelerometer.");
            }
        }
    }
}
