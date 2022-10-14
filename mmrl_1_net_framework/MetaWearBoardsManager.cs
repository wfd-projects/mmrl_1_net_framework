using MbientLab.MetaWear;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using System.Linq;

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
    }
}
