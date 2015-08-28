using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.Background;

namespace BackgroundTasks
{
    public sealed class AdvertisementWatcherTask : IBackgroundTask
    {
        private IBackgroundTaskInstance backgroundTaskInstance;


        public void Run(IBackgroundTaskInstance taskInstance)
        {

            backgroundTaskInstance = taskInstance;

            var details = taskInstance.TriggerDetails as BluetoothLEAdvertisementWatcherTriggerDetails;

            if (details != null)
            {
                // If the background watcher stopped unexpectedly, an error will be available here.
                var error = details.Error;

                // The Advertisements property is a list of all advertisement events received
                // since the last task triggered. The list of advertisements here might be valid even if
                // the Error status is not Success since advertisements are stored until this task is triggered
                IReadOnlyList<BluetoothLEAdvertisementReceivedEventArgs> advertisements = details.Advertisements;

                // The signal strength filter configuration of the trigger is returned such that further 
                // processing can be performed here using these values if necessary. They are read-only here.
                var rssiFilter = details.SignalStrengthFilter;

                // In this example, the background task simply constructs a message communicated
                // to the App. For more interesting applications, a notification can be sent from here instead.
                //string eventMessage = "";
                //eventMessage += string.Format("ErrorStatus: {0}, EventCount: {1}, HighDBm: {2}, LowDBm: {3}, Timeout: {4}, Sampling: {5}",
                //    error.ToString(),
                //    advertisements.Count.ToString(),
                //    rssiFilter.InRangeThresholdInDBm.ToString(),
                //    rssiFilter.OutOfRangeThresholdInDBm.ToString(),
                //    rssiFilter.OutOfRangeTimeout.GetValueOrDefault().TotalMilliseconds.ToString(),
                //    rssiFilter.SamplingInterval.GetValueOrDefault().TotalMilliseconds.ToString());

                //// Advertisements can contain multiple events that were aggregated, each represented by 
                //// a BluetoothLEAdvertisementReceivedEventArgs object.
                //foreach (var eventArgs in advertisements)
                //{
                //    // Check if there are any manufacturer-specific sections.
                //    // If there is, print the raw data of the first manufacturer section (if there are multiple).
                //    string manufacturerDataString = "";
                //    var manufacturerSections = eventArgs.Advertisement.ManufacturerData;
                //    if (manufacturerSections.Count > 0)
                //    {
                //        var manufacturerData = manufacturerSections[0];
                //        var data = new byte[manufacturerData.Data.Length];
                //        using (var reader = DataReader.FromBuffer(manufacturerData.Data))
                //        {
                //            reader.ReadBytes(data);
                //        }
                //        // Print the company ID + the raw data in hex format.
                //        manufacturerDataString = string.Format("0x{0}: {1}",
                //            manufacturerData.CompanyId.ToString("X"),
                //            BitConverter.ToString(data));
                //    }
                //    eventMessage += string.Format("\n[{0}] [{1}]: Rssi={2}dBm, localName={3}, manufacturerData=[{4}]",
                //        eventArgs.Timestamp.ToString("hh\\:mm\\:ss\\.fff"),
                //        eventArgs.AdvertisementType.ToString(),
                //        eventArgs.RawSignalStrengthInDBm.ToString(),
                //        eventArgs.Advertisement.LocalName,
                //        manufacturerDataString);
                //}

                //// Store the message in a local settings indexed by this task's name so that the foreground App
                //// can display this message.
                //ApplicationData.Current.LocalSettings.Values[taskInstance.Task.Name] = eventMessage;
            }
        }

    }
}
