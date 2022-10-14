using MbientLab.MetaWear;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;

namespace mmrl_1_net_framework
{
    /// <summary>
    /// Handles communication between user and MetaWear board.
    /// </summary>
    class MetaWearBoardsManager
    {
        private Dictionary<ulong, IMetaWearBoard> _metaWearBoards;

        // Minimum battery level in percent.
        private const byte BATTERY_LEVEL_MIN = 20;

        public MetaWearBoardsManager()
        {
            _metaWearBoards = new Dictionary<ulong, IMetaWearBoard>();
        }

        /// <summary>
        /// Connects to and initialises a new MetaWear board.
        /// </summary>
        /// <param name="macAddress"></param>
        public async Task ConnectToBoard(ulong macAddress)
        {
            if (_metaWearBoards.ContainsKey(macAddress))
            {
                Console.WriteLine($"INFO: There already exists a board with MAC address {macAddress}.");
            }
            else
            {
                var bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(macAddress);
                var metaWearBoard = MbientLab.MetaWear.Win10.Application.GetMetaWearBoard(bleDevice);
                try
                {
                    await metaWearBoard.InitializeAsync();

                    // TODO: Assign method which tries to re-connect to the board x-times and only then aborts the process with an error message.
                    metaWearBoard.OnUnexpectedDisconnect = () => Console.WriteLine($"Unexpectedly lost connection to board with MAC address {macAddress}!");

                    var batteryLevel = await metaWearBoard.ReadBatteryLevelAsync();
                    if (batteryLevel < BATTERY_LEVEL_MIN)
                    {
                        Console.WriteLine($"INFO: Battery level low! (MAC={macAddress}, Charge={batteryLevel}%)");
                    }

                    _metaWearBoards.Add(macAddress, metaWearBoard);
                }
                catch (Exception e)
                {
                    metaWearBoard.TearDown();
                    Console.WriteLine($"ERROR: Could not connect to or initialise MetaWear board with MAC address {macAddress}!");
                    Console.WriteLine($"       Reason: {e}");
                }
            }
        }

        /// <summary>
        /// Disconnects from a MetaWear board and removes it from this manager.
        /// </summary>
        /// <param name="macAddress"></param>
        /// <returns>0 if the board could successfully be removed, -1 otherwise.</returns>
        public int DisconnectBoard(ulong macAddress)
        {
            IMetaWearBoard board = null;
            if (_metaWearBoards.TryGetValue(macAddress, out board))
            {
                board.TearDown();
                _metaWearBoards.Remove(macAddress);
                return 0;
            }
            else
            {
                Console.WriteLine($"ERROR: Could not disconnect MetaWear board with MAC address {macAddress}!");
                return -1;
            }
        }
    }
}
