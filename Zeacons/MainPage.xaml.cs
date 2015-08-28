using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Zeacons
{
    public sealed partial class MainPage : Page
    {
        BluetoothLEAdvertisementPublisher publisher;
        BluetoothLEAdvertisementWatcher watcher;

        private ushort COMPANY_ID = 0x0006;

        private bool infected = false;

        private bool Infected
        {
            get { return infected; }
            set
            {
                if (value)
                {
                    watcher.Stop();
                    publisher.Start();

                    // call can come from non UI thread
                    Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        VisualStateManager.GoToState(this, "zombie", false);
                    });
                }
                else
                {
                    publisher.Stop();
                    watcher.Start();

                    // call can come from non UI thread
                    Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        VisualStateManager.GoToState(this, "human", false);
                    });
                }

                infected = value;
            }
        }


        public MainPage()
        {
            this.InitializeComponent();

            // don't lock screen
            DisplayRequest request = new DisplayRequest();
            request.RequestActive();

            initWatcher();
            initPublisher();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.Current.Suspending += App_Suspending;
            App.Current.Resuming += App_Resuming;

            watcher.Received += Watcher_Received;
            watcher.Stopped += Watcher_Stopped;

            publisher.StatusChanged += Publisher_StatusChanged;


            watcher.Start();
        }

        private void App_Resuming(object sender, object e)
        {
            watcher.Received += Watcher_Received;
            watcher.Stopped += Watcher_Stopped;

            publisher.StatusChanged += Publisher_StatusChanged;

            Infected = Infected;
        }

        private void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            watcher.Stop();
            publisher.Stop();

            watcher.Received -= Watcher_Received;
            watcher.Stopped -= Watcher_Stopped;

            publisher.StatusChanged -= Publisher_StatusChanged;
        }

        #region Watcher

        private void initWatcher()
        {

            watcher = new BluetoothLEAdvertisementWatcher();

            // let's set up filters so we only watch for specific beacons
            var manufacturerData = new BluetoothLEManufacturerData();
            manufacturerData.CompanyId = COMPANY_ID;

            watcher.AdvertisementFilter.Advertisement.ManufacturerData.Add(manufacturerData);

            watcher.SignalStrengthFilter.InRangeThresholdInDBm = -50;
            watcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -80;
            watcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromMilliseconds(2000);

        }

        private async void Watcher_Stopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
        {
            if (args.Error != Windows.Devices.Bluetooth.BluetoothError.Success)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    MessageDialog dialog = new MessageDialog("Error: " + args.Error.ToString(), "Can not start Watcher");
                    await dialog.ShowAsync();
                });
                
            }
        }

        private void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            BluetoothLEAdvertisement advert = args.Advertisement;

            if (advert == null)
                return;

            if (advert.ManufacturerData.Count > 0)
            {
                var temp = ParseAdvertisementBuffer(advert.ManufacturerData[0].Data);
                Debug.WriteLine("received - " + "rssi: " + args.RawSignalStrengthInDBm.ToString() + " | data: " + printBytes(temp));
                if (args.RawSignalStrengthInDBm > -50)
                {
                    Infected = true;
                }

            }
        }

        #endregion


        #region Publisher

        private void initPublisher()
        {
            publisher = new BluetoothLEAdvertisementPublisher();

            var manufacturerData = new BluetoothLEManufacturerData();
            manufacturerData.CompanyId = COMPANY_ID;

            var writer = new DataWriter();
            UInt16 uuidData = 0x0001;
            writer.WriteUInt16(uuidData);
            manufacturerData.Data = writer.DetachBuffer();

            publisher.Advertisement.ManufacturerData.Add(manufacturerData);


        }

        private async void Publisher_StatusChanged(BluetoothLEAdvertisementPublisher sender, BluetoothLEAdvertisementPublisherStatusChangedEventArgs args)
        {
            if (args.Error != Windows.Devices.Bluetooth.BluetoothError.Success)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    MessageDialog dialog = new MessageDialog("Error: " + args.Error.ToString(), "Can not start Watcher");
                    await dialog.ShowAsync();
                });

            }
        }

        #endregion

        //private async void getInfected()
        //{
        //    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //    {
        //        VisualStateManager.GoToState(this, "zombie", false);
        //        infected = true;
        //    });

        //    if (watcher != null)
        //    {
        //        watcher.Stop();
        //    }
        //    if (publisher == null)
        //    {
        //        initPublisher();
        //    }



        //    publisher.Start();

        //}

        //private async void becomeHuman()
        //{
        //    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //    {
        //        VisualStateManager.GoToState(this, "human", false);
        //        infected = false;

        //    });

        //    if (watcher == null)
        //    {
        //        initWatcher();
        //    }

        //    if (publisher != null)
        //    {
        //        publisher.Stop();
        //    }

        //    watcher.Start();
        //}





        #region
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
                Infected = !Infected;
            }
        }

        
        #endregion
    }
}
