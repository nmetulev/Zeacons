using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Background;

namespace BackgroundTasks
{
    class AdvertisementPublisherTask : IBackgroundTask
    {
        private IBackgroundTaskInstance backgroundTaskInstance;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            backgroundTaskInstance = taskInstance;

            var details = taskInstance.TriggerDetails as BluetoothLEAdvertisementPublisherTriggerDetails;

            if (details != null)
            {
                // If the background publisher stopped unexpectedly, an error will be available here.
                var error = details.Error;

                // The status of the publisher is useful to determine whether the advertisement payload is being serviced
                // It is possible for a publisher to stay in a Waiting state while radio resources are in use.
                var status = details.Status;

                // In this example, the background task simply constructs a message communicated
                // to the App. For more interesting applications, a notification can be sent from here instead.
                string eventMessage = "";
                eventMessage += string.Format("Publisher Status: {0}, Error: {1}",
                    status.ToString(),
                    error.ToString());

                // Store the message in a local settings indexed by this task's name so that the foreground App
                // can display this message.
                //ApplicationData.Current.LocalSettings.Values[taskInstance.Task.Name] = eventMessage;
            }
        }
    }
}
