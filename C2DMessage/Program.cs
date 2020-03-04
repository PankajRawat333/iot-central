using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace C2DMessage
{
    class Program
    {
        static ServiceClient serviceClient;
        static string connectionString = "HostName=******.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=XBKajMESRw******";
        static string registryConnectionString = "HostName=TSFIoTHub-POC.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=******";
        static async Task Main(string[] args)
        {
            Console.WriteLine("Send Cloud-to-Device message\n");
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            //await SendCloudToDeviceMessageAsync();
            //await ReceiveFeedback(serviceClient);
            //await CallDeviceMethod(serviceClient);
            var registryManager = RegistryManager.CreateFromConnectionString(registryConnectionString);
            await ApplyFirmwareUpdate(registryManager, "SampleDevice");
            Console.WriteLine("Press any key to send a C2D message.");
            Console.Read();
        }

        private static async Task ApplyFirmwareUpdate(RegistryManager registryManager, string devicid)
        {
            var deviceTwin = await registryManager.GetTwinAsync(devicid);
            var twinPatch = new
            {
                properties = new
                {
                    desired = new
                    {
                        firmwareVersion = "2.0"
                    }
                }
            };

            var twinPatchJson = JsonConvert.SerializeObject(twinPatch);
            await registryManager.UpdateTwinAsync(devicid, twinPatchJson, deviceTwin.ETag);
            Console.WriteLine("Firmware update sent to device!");
        }

        private static async Task CallDeviceMethod(ServiceClient serviceClient)
        {
            var method = new CloudToDeviceMethod("ShowMessage");
            method.SetPayloadJson("'Hello from C#'");

            var response = serviceClient.InvokeDeviceMethodAsync("SampleDevice", method);
            Console.WriteLine($"Response status {response.Status}");            
        }

        private static async Task ReceiveFeedback(ServiceClient serviceClient)
        {
            var feedbackReceiver = serviceClient.GetFeedbackReceiver();
            while (true)
            {
                var feedbackBatch = await feedbackReceiver.ReceiveAsync();
                if (feedbackBatch == null)
                {
                    continue;
                }
                foreach (var record in feedbackBatch.Records)
                {
                    var messageId = record.OriginalMessageId;
                    var statusCode = record.StatusCode;
                    Console.WriteLine($"Feedback for message {messageId} and status code {statusCode}");
                }

                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }
        }

        private async static Task SendCloudToDeviceMessageAsync()
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes("Cloud to device message."));
            commandMessage.MessageId = Guid.NewGuid().ToString();
            commandMessage.Ack = DeliveryAcknowledgement.Full;
            commandMessage.ExpiryTimeUtc = DateTime.UtcNow.AddSeconds(25);
            await serviceClient.SendAsync("SampleDevice", commandMessage);
        }
    }
}
