using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Zeacons
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        BluetoothLEAdvertisementPublisher publisher;
        BluetoothLEAdvertisementWatcher watcher;

        bool zombieEh = false;

        public MainPage()
        {
            this.InitializeComponent();

            // don't lock screen
            DisplayRequest request = new DisplayRequest();
            request.RequestActive();
           
            becomeHuman();

        }

        private async void getInfected()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                VisualStateManager.GoToState(this, "zombie", false);
                zombieEh = true;
            });

            if (watcher != null)
            {
                watcher.Stop();
            }
            if (publisher == null)
            {
                initPublisher();
            }

           

            publisher.Start();

        }

        private async void becomeHuman()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                VisualStateManager.GoToState(this, "human", false);
                zombieEh = false;

            });

            if (watcher == null)
            {
                initWatcher();
            }

            if (publisher != null)
            {
                publisher.Stop();
            }

            watcher.Start();
        }

        private void initPublisher()
        {
            publisher = new BluetoothLEAdvertisementPublisher();

            var beaconData = new BluetoothLEManufacturerData();
            byte[] pattern = { 0x00, 0x00, 0x00, 0x01 };
            beaconData.Data = pattern.AsBuffer();
            beaconData.CompanyId = 0x0006;

            publisher.Advertisement.ManufacturerData.Add(beaconData);

            publisher.StatusChanged += Publisher_StatusChanged;

            //publisher.Start();
        }

        private void initWatcher()
        {
            
            watcher = new BluetoothLEAdvertisementWatcher();
            watcher.Received += Watcher_Received;
            //watcher.Start();
        }

        private void Publisher_StatusChanged(BluetoothLEAdvertisementPublisher sender, BluetoothLEAdvertisementPublisherStatusChangedEventArgs args)
        {
            Debug.WriteLine("publisher status changed - TODO handle");
        }

       

        private void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {

            BluetoothLEAdvertisement advert = args.Advertisement;

            foreach(var data in advert.ManufacturerData)
            {

                if (data.CompanyId == 6 )
                {
                    var temp = ParseAdvertisementBuffer(data.Data);
                    Debug.WriteLine("received - " + "rssi: " + args.RawSignalStrengthInDBm.ToString() + " | data: " + printBytes(temp));
                    if (args.RawSignalStrengthInDBm > -50)
                    {
                        getInfected();
                    }
                    
                }


            }
        }

        private byte[] ParseAdvertisementBuffer(IBuffer buffer)
        {
            uint dataLength = buffer.Length;

            byte[] data = new byte[dataLength];
            using (DataReader reader = DataReader.FromBuffer(buffer))
            {
                reader.ReadBytes(data);
            }

            return data;
        }

        private string printBytes(byte[] bytes)
        {
            var temp = "";
            for (int i = 0; i < bytes.Length; i += 2)
            {
                temp += ((short)((bytes[i] << 8) | (bytes[i + 1]))) + ",";
            }

            return temp;
        }

        string tapped = "";
        string match = "ABCD";

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            tapped += (sender as Border).Tag;
            if (tapped.Length > 4)
            {
                tapped = tapped.Substring(tapped.Length - 4);
            }

            if (tapped == match)
            {
                if (zombieEh)
                {
                    becomeHuman();
                }
                else
                {
                    getInfected();
                }
            }
        }
    }
}
