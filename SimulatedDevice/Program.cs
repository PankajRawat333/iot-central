// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

//This is the code that sends messages to the IoT Hub for testing the routing as defined
//  in this article: https://docs.microsoft.com/en-us/azure/iot-hub/tutorial-routing
//The scripts for creating the resources are included in the resources folder in this
//  Visual Studio solution. 

using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SimulatedDevice
{
    class Program
    {
        private static DeviceClient s_deviceClient;
        private readonly static string s_myDeviceId = "esp";
        private readonly static string s_iotHubUri = "******.azure-devices.net";
        // This is the primary key for the device. This is in the portal. 
        // Find your IoT hub in the portal > IoT devices > select your device > copy the key. 
        private readonly static string s_deviceKey = "******kpd2Reg=";

        private static void Main(string[] args)
        {
            Console.WriteLine("Routing Tutorial: Simulated device\n");
            //s_deviceClient = DeviceClient.Create(s_iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(s_myDeviceId, s_deviceKey), TransportType.Mqtt);
            s_deviceClient = DeviceClient.CreateFromConnectionString("HostName=iotc-******.azure-devices.net;DeviceId=2nfroa5bjxo;SharedAccessKey=CksOulnG******");

            //ReceiveEvent(s_deviceClient);

            //s_deviceClient.SetMethodHandlerAsync("ShowMessage", ShowMessage, null);
            SendDeviceToCloudMessagesAsync();
            //SendTwinData();
            s_deviceClient.SetDesiredPropertyUpdateCallbackAsync(UpdateDesiredProperty, null);
            Console.WriteLine("Press the Enter key to stop.");
            Console.ReadLine();
        }

        private static async Task UpdateDesiredProperty(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine("Desired property change:");
            if (desiredProperties["TelemetryInterval"] != null)
            {
                var twinProperties = new Microsoft.Azure.Devices.Shared.TwinCollection();
                twinProperties["TelemetryInterval"] = desiredProperties["TelemetryInterval"];
                await s_deviceClient.UpdateReportedPropertiesAsync(twinProperties);
            }
        }

        private static Task<MethodResponse> ShowMessage(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("****Message Received*****");
            Console.WriteLine(methodRequest.DataAsJson);

            var responsePayload = Encoding.ASCII.GetBytes("{\"response\":\"Message shown\"}");
            return Task.FromResult(new MethodResponse(responsePayload, 200));
        }

        private static async void ReceiveEvent(DeviceClient deviceClient)
        {
            while (true)
            {
                var message = await deviceClient.ReceiveAsync();
                if (message == null)
                {
                    continue;
                }

                var messageBody = message.GetBytes();
                var payload = Encoding.ASCII.GetString(messageBody);
                await deviceClient.CompleteAsync(message);
                //await deviceClient.ReceiveAsync();
                //await deviceClient.AbandonAsync(message);
                Console.WriteLine($"Message Received from cloud {payload}");
            }
        }
        private static async void SendTwinData()
        {
            var twinProperties = new Microsoft.Azure.Devices.Shared.TwinCollection();
            twinProperties["TelemetryInterval"] = "12";
            await s_deviceClient.UpdateReportedPropertiesAsync(twinProperties);
        }
        private static async void SendDeviceToCloudMessagesAsync()
        {
            Random rand = new Random();

            while (true)
            {
                int randMoisture = new Random().Next(1000, 9999);
                DateTime messageTime = DateTime.UtcNow;

                string infoString;
                string levelValue;

                

                var telemetryDataPoint = new
                {
                    MessageTime = messageTime,
                    Moisture = randMoisture
                };
                var telemetryDataString = JsonConvert.SerializeObject(telemetryDataPoint);

                //set the body of the message to the serialized value of the telemetry data
                var message = new Message(Encoding.ASCII.GetBytes(telemetryDataString));
                
                message.Properties.Add("TelemetryInterval", "10");
                message.ContentEncoding = "utf-8";
                message.ContentType = "application/json";
                message.MessageId = Guid.NewGuid().ToString();
                await s_deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sent message: {1}", DateTime.Now, telemetryDataString);

                await Task.Delay(10000);
            }
        }
    }
}
