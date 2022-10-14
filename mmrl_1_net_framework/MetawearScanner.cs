using MbientLab.MetaWear;
using System;
using System.Collections.Generic;
using Windows.Devices.Bluetooth.Advertisement;
using System.Linq;

namespace mmrl_1_net_framework
{
    class MetawearScanner
    {
        /// <summary>
        /// Devices which have already been encountered will be remembered and only listed once.
        /// </summary>
        private HashSet<ulong> _seenDeviceMacAddresses;
        private BluetoothLEAdvertisementWatcher _watcher;

        public MetawearScanner()
        {
            _seenDeviceMacAddresses = new HashSet<ulong>();

            BluetoothLEManufacturerData manufacturerFilter = new BluetoothLEManufacturerData();

            _watcher = new BluetoothLEAdvertisementWatcher();
            _watcher.Received += OnAdvertisementReceived;
            // Actively scanning consumes more power but makes sure we receive scan response advertisements as well.
            _watcher.ScanningMode = BluetoothLEScanningMode.Active;
        }

        public void StartScanning()
        {
            Console.WriteLine("Scanning for MetaWear boards...");
            _watcher.Start();
        }

        public void StopScanning()
        {
            Console.WriteLine("Stopped scanning.");
            _watcher.Stop();
        }

        public List<string> GetScanResults()
        {
            List<string> macAddresses = new List<string>();
            foreach (var addressUl in _seenDeviceMacAddresses)
            {
                macAddresses.Add(MacUlongToString(addressUl));
            }
            return macAddresses;
        }

        /// <summary>
        /// Converts a MAC address from ulong into a more readable colon-separated string.
        /// Example: (ulong)253022581560120 becomes (string)E6:1F:69:18:13:38
        /// Code taken from: https://stackoverflow.com/questions/50519301/how-to-convert-a-mac-address-from-string-to-unsigned-int
        /// </summary>
        /// <param name="macAddress"></param>
        /// <returns></returns>
        public static string MacUlongToString(ulong macAddress)
        {
            return string.Join(":", BitConverter.GetBytes(macAddress).Reverse().Select(b => b.ToString("X2"))).Substring(6);
        }

        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            // Remember new BLE device only when it is a MetaWear board.
            if (!_seenDeviceMacAddresses.Contains(eventArgs.BluetoothAddress) && eventArgs.Advertisement.ServiceUuids.Contains(Constants.METAWEAR_GATT_SERVICE))
            {
                _seenDeviceMacAddresses.Add(eventArgs.BluetoothAddress);
                Console.WriteLine($"Found new device with MAC addres {MacUlongToString(eventArgs.BluetoothAddress)}.");
            }
        }
    }
}
