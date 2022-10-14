using MbientLab.MetaWear;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using System.Linq;
using MbientLab.MetaWear.Data;
using MbientLab.MetaWear.Sensor;
using MbientLab.MetaWear.Core;
using MbientLab.MetaWear.Sensor.AccelerometerBmi160;
using MbientLab.MetaWear.Sensor.AccelerometerBosch;

namespace mmrl_1_net_framework
{
    /// <summary>
    /// Handles communication between user and MetaWear board.
    /// </summary>
    class MetaWearBoardsManager
    {
        /// <summary>
        /// MAC addresses of the boards to which a connection already exists.
        /// </summary>
        public HashSet<ulong> ConnectedBoardsAddresses { get; private set; }

        public enum AccelerometerSpeed
        {
            normal = 0,
            fast = 1
        }

        // Minimum battery level in percent.
        private const byte BATTERY_LEVEL_MIN = 20;

        public MetaWearBoardsManager()
        {
            ConnectedBoardsAddresses = new HashSet<ulong>();
        }

        /// <summary>
        /// Connects to and initialises a new MetaWear board.
        /// </summary>
        /// <param name="macAddress"></param>
        /// <returns>Returns a reference to a MetaWear board, null otherwise.</returns>
        public async Task<IMetaWearBoard> ConnectToBoard(ulong macAddress)
        {
            if (ConnectedBoardsAddresses.Contains(macAddress))
            {
                Console.WriteLine($"INFO: Already connected to a board with MAC address {MetaWearScanner.MacUlongToString(macAddress)}.");
            }
            else
            {
                var bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(macAddress);
                var metaWearBoard = MbientLab.MetaWear.Win10.Application.GetMetaWearBoard(bleDevice);
                try
                {
                    await metaWearBoard.InitializeAsync();

                    // TODO: Assign method which tries to re-connect to the board x-times and only then aborts the process with an error message.
                    metaWearBoard.OnUnexpectedDisconnect = () => Console.WriteLine($"Unexpectedly lost connection to board with MAC address {MetaWearScanner.MacUlongToString(macAddress)}!");

                    var batteryLevel = await metaWearBoard.ReadBatteryLevelAsync();
                    if (batteryLevel < BATTERY_LEVEL_MIN)
                    {
                        Console.WriteLine($"INFO: Battery level low! (MAC={MetaWearScanner.MacUlongToString(macAddress)}, Charge={batteryLevel}%)");
                    }

                    ConnectedBoardsAddresses.Add(macAddress);
                    return metaWearBoard;
                }
                catch (Exception e)
                {
                    metaWearBoard.TearDown();
                    Console.WriteLine($"ERROR: Could not connect to or initialise MetaWear board with MAC address {MetaWearScanner.MacUlongToString(macAddress)}!");
                    Console.WriteLine($"       Reason: {e}");
                }
            }
            return null;
        }

        /// <summary>
        /// Disconnects a MetaWear board.
        /// </summary>
        /// <param name="macAddress"></param>
        /// <returns>0 if the board could successfully be disconnected, -1 otherwise.</returns>
        public int DisconnectBoard(IMetaWearBoard board)
        {
            ulong macAddress = MetaWearScanner.MacUlongFromString(board.MacAddress);
            if (ConnectedBoardsAddresses.Contains(macAddress) || board.IsConnected)
            {
                board.TearDown();
                ConnectedBoardsAddresses.Remove(macAddress);
                return 0;
            }
            else
            {
                Console.WriteLine($"ERROR: Could not disconnect MetaWear board with MAC address {board.MacAddress}!");
                return -1;
            }
        }

        public async Task StartAccelerometerStream(IMetaWearBoard board, Acceleration accData, AccelerometerSpeed speed = AccelerometerSpeed.normal)
        {
            if (ConnectedBoardsAddresses.Contains(MetaWearScanner.MacUlongFromString(board.MacAddress)))
            {
                // Reduce the max BLE connection interval to 7.5ms so the BLE connection can handle the acceleroemter's sampling frequency.
                board.GetModule<ISettings>()?.EditBleConnParams(maxConnInterval: 7.5f);
                await Task.Delay(1500);

                IAccelerometerBmi160 accelerometer = board.GetModule<IAccelerometerBmi160>();

                // Set output data rate to 25Hz, set range to +/-4g.
                accelerometer.Configure(odr: OutputDataRate._25Hz, range: DataRange._4g);

                // Accelerometer has a fast mode that combines 3 data samples into 1 BLE package increasing the data throughput by 3x.
                if (speed == AccelerometerSpeed.fast)
                {
                    await accelerometer.PackedAcceleration.AddRouteAsync(source => source.Stream(data => {
                            accData = data.Value<Acceleration>();
                            Console.WriteLine("Acceleration = " + accData);
                        }
                    ));

                    // Start the acceleration data.
                    accelerometer.PackedAcceleration.Start();
                }
                else
                {
                    await accelerometer.Acceleration.AddRouteAsync(source => source.Stream(data => {
                            accData = data.Value<Acceleration>();
                            Console.WriteLine("Acceleration = " + accData);
                        }
                    ));

                    // Start the acceleration data.
                    accelerometer.Acceleration.Start();
                }

                // Put accelerometer in active mode.
                accelerometer.Start();
            }
            else
            {
                Console.WriteLine($"ERROR: Could not stream acceleration data from {board.MacAddress}!");
            }
        }

        public void StopAccelerometerStream(IMetaWearBoard board)
        {
            if (ConnectedBoardsAddresses.Contains(MetaWearScanner.MacUlongFromString(board.MacAddress)))
            {
                IAccelerometerBmi160 accelerometer = board.GetModule<IAccelerometerBmi160>();

                // Put accelerometer back into standby mode.
                accelerometer.Stop();

                // Stop accelerometer data collection.
                accelerometer.Acceleration.Stop();
            }
            else
            {
                Console.WriteLine($"ERROR: StopAccelerometerStream() could not find {board.MacAddress}!");
            }
        }
    }
}
