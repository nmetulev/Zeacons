using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Advertiser
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            startAdvert();
        }

        private void startAdvert()
        {
            var publisher = new BluetoothLEAdvertisementPublisher();

            var beaconData = new BluetoothLEManufacturerData();
            byte[] pattern = {0x00, 0x00, 0x00, 0x0A };
            beaconData.Data = pattern.AsBuffer();
            beaconData.CompanyId = 0x0006;

            publisher.Advertisement.ManufacturerData.Add(beaconData);

            publisher.StatusChanged += Publisher_StatusChanged;

            publisher.Start();
        }

        private void Publisher_StatusChanged(BluetoothLEAdvertisementPublisher sender, BluetoothLEAdvertisementPublisherStatusChangedEventArgs args)
        {
            Debug.WriteLine("publisher status changed");
        }
    }
}
