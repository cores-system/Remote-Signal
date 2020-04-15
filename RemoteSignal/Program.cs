using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace RemoteSignal
{
    class Program
    {
        static HubConnection hubConnection;

        static void Main(string[] args)
        {
            Console.WriteLine("Connection..");
            hubConnection = new HubConnectionBuilder().WithUrl("http://nserv.host:5300/hubs/rs").Build();
            hubConnection.Closed += HubConnection_Closed;

            hubConnection.On("OnConnected", (string supportVerion, string connectionId) =>
            {
                if (supportVerion != "14042020")
                {
                    Console.WriteLine("Версия больше не поддерживается, обновите до актуальной");
                    hubConnection.StopAsync();
                }
                else
                {
                    _ = HttpClient.Get($"http://nserv.host:5300/forkapi/registryrs?connectionId={connectionId}", null);
                }
            });

            RunAsync().Wait();

            Console.Clear();
            Console.WriteLine("Connected: " + (hubConnection.State == HubConnectionState.Connected));
            Console.ReadLine();
        }

        #region RunAsync
        async static Task RunAsync()
        {
            #region OnGet
            hubConnection.On("OnGet", async (string randKey, string url, string addHeaders) =>
            {
                Console.WriteLine($"\nGET | {url}");

                byte[] buffer = await HttpClient.Get(url, JsonConvert.DeserializeObject<List<(string name, string val)>>(addHeaders));
                if (buffer == null)
                {
                    Console.WriteLine("\tbufferLength: 0");
                    return;
                }

                Console.WriteLine($"\tbufferLength: {buffer.Length}");
                await hubConnection.SendAsync("OnData", randKey, buffer);
            });
            #endregion

            #region OnPost
            hubConnection.On("OnPost", async (string randKey, string url, string data, string addHeaders) =>
            {
                Console.WriteLine($"\nPOST | {url}");

                byte[] buffer = await HttpClient.Post(url, JsonConvert.DeserializeObject<HttpContent>(data), JsonConvert.DeserializeObject<List<(string name, string val)>>(addHeaders));
                if (buffer == null)
                {
                    Console.WriteLine("\tbufferLength: 0");
                    return;
                }

                Console.WriteLine($"\tbufferLength: {buffer.Length}");
                await hubConnection.SendAsync("OnData", randKey, buffer);
            });
            #endregion

            await hubConnection.StartAsync();
        }
        #endregion

        #region HubConnection_Closed
        static Task HubConnection_Closed(Exception arg)
        {
            Console.WriteLine("Connection Closed: " + arg.Message);
            hubConnection.Closed -= HubConnection_Closed;
            return Task.CompletedTask;
        }
        #endregion
    }
}
