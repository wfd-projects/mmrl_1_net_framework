using System;
using System.Threading.Tasks;
using System.Linq;
using MbientLab.MetaWear.Data;
using MbientLab.MetaWear;

namespace mmrl_1_net_framework
{
    class Program
    {
        public static async Task ScenarioStreamAcceleration(MetaWearBoardsManager boardsManager, IMetaWearBoard board)
        {
            Acceleration accData = new Acceleration(0, 0, 0);
            Console.WriteLine("Starting accelerometer data stream... Press x to stop!");
            await boardsManager.StartAccelerometerStream(board, accData);
            Console.WriteLine("Press any key to stop the accelerometer.");
            Console.ReadKey();
            boardsManager.StopAccelerometerStream(board);
            Console.WriteLine("Stopped accelerometer.\n");
        }

        public static async Task ScenarioStreamFusion(MetaWearBoardsManager boardsManager, IMetaWearBoard board)
        {
            FusionData fusionData = new FusionData();
            Console.WriteLine("\nStarting sensor fusion data stream... Press any key to stop!");
            await boardsManager.StartSensorFusionStream(board, fusionData);
            Console.ReadKey();
            boardsManager.StopSensorFusionStream(board);
            Console.WriteLine("Stopped sensor fusion.\n");
        }

        /// <summary>
        /// IMPORTANT: Sensor fusion and accelerometer modules cannot be used simultaneously!
        /// https://mbientlab.com/csdocs/1/sensor_fusion.html
        /// Problem: Tapping on sensor to start calibration of arm posture is not possible.
        /// Ideas how to deal with this in the HoloLens:
        ///  * Speech-command to trigger arm calibration.
        ///  * Button that can be pressed an after a countdown of, e.g., 10 seconds during which there is enough time to get the arm into posture, calibration is started.
        /// 
        /// This is a test if it's possible anyways to stream sensor fusion data and acceleration data simultaneously.
        /// I tried to reduce BLE bandwidth of the accelerometer and it works :)
        /// However, we need to evaluate if the reduced bandwidth has negative effects when controlling objects in Unity.
        /// </summary>
        public static async Task ScenarioStreamFusion_double(MetaWearBoardsManager boardsManager, IMetaWearBoard board)
        {
            Console.WriteLine("\nStarting sensor fusion AND acceleration data stream... Press any key to stop!");
            await boardsManager.StartFusion_double(board);
            Console.ReadKey();
            await boardsManager.StopFusion_double(board);
            Console.WriteLine("Stopped sensor fusion.\n");
        }

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
                else
                {
                    Console.WriteLine($"Could not connect to {board.MacAddress}.");
                    return;
                }

                /***** Calibrate sensor fusion *****/
                // TODO

                /***** Stream sensor fusion data *****/
                //await ScenarioStreamFusion(boardsManager, board);
                await ScenarioStreamFusion_double(boardsManager, board);

                /***** Stream accelerometer *****/
                //await ScenarioStreamAcceleration(boardsManager, board);

                /***** Disconnect board *****/
                Console.WriteLine("\nPress any key to disconnect.");
                Console.ReadKey();
                if (boardsManager.DisconnectBoard(board) == 0)
                {
                    Console.WriteLine("Successfully disconnected.");
                }
                else
                {
                    Console.WriteLine("Error: Could not disconnect.");
                }
            }
        }
    }
}
