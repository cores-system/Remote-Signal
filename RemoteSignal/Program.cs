using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RemoteSignal
{
    class Program
    {
        static HubConnection hubConnection;

        static void Main(string[] args)
        {
            Console.WriteLine("Connection..");
            BuildOrReBuldHub();

            StartAsync().Wait();

            if (hubConnection.State == HubConnectionState.Connected)
            {
                Console.Clear();
                Console.WriteLine("Connected: true");
            }

            Console.ReadLine();
        }

        #region BuildOrReBuldHub
        static void BuildOrReBuldHub()
        {
            if (hubConnection != null)
                hubConnection.Closed -= HubConnection_Closed;

            hubConnection = new HubConnectionBuilder().WithUrl("http://nserv.host:5300/hubs/rs").Build();
            hubConnection.HandshakeTimeout = TimeSpan.FromSeconds(5);
            hubConnection.Closed += HubConnection_Closed;

            hubConnection.On("OnConnected", async (string supportVerion, string connectionId) =>
            {
                if (supportVerion != "13052020")
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Версия больше не поддерживается, обновите до актуальной\n\thttp://rs.nserv.host/");

                    hubConnection.Closed -= HubConnection_Closed;
                    await hubConnection.StopAsync();
                }
                else
                {
                    _ = HttpClient.Get($"http://nserv.host:5300/forkapi/registryrs?connectionId={connectionId}", null);
                }
            });
        }
        #endregion

        #region StartAsync
        async static Task StartAsync()
        {
            #region OnGet
            hubConnection.On("OnGet", async (string randKey, string url, string addHeaders) =>
            {
                Console.WriteLine($"\nGET | {url}");

                byte[] buffer = await HttpClient.Get(url, JsonConvert.DeserializeObject<List<(string name, string val)>>(addHeaders));
                if (buffer == null)
                {
                    Console.WriteLine("\tbufferLength: 0 / ошибка");
                    return;
                }

                Console.WriteLine($"\tbufferLength: {buffer.Length}");
                await hubConnection.SendAsync("OnData", randKey, buffer);
            });
            #endregion

            #region OnPost
            hubConnection.On("OnPost", async (string randKey, string url, string content, string encodingName, string mediaType, string addHeaders) =>
            {
                Console.WriteLine($"\nPOST | {url}\n\t{content}");

                byte[] buffer = await HttpClient.Post(url, new StringContent(content, Encoding.GetEncoding(encodingName), mediaType), JsonConvert.DeserializeObject<List<(string name, string val)>>(addHeaders));
                if (buffer == null)
                {
                    Console.WriteLine("\tbufferLength: 0 / ошибка");
                    return;
                }

                Console.WriteLine($"\tbufferLength: {buffer.Length}");
                await hubConnection.SendAsync("OnData", randKey, buffer);
            });
            #endregion

            #region OnLogin
            hubConnection.On("OnLogin", async (string randKey, string url, string content, string encodingName, string mediaType, string addHeaders) =>
            {
                Console.WriteLine($"\nTakeLogin | {url}\n\t{content}");

                string cookies = await HttpClient.TakeLogin(url, new StringContent(content, Encoding.GetEncoding(encodingName), mediaType), JsonConvert.DeserializeObject<List<(string name, string val)>>(addHeaders));
                if (cookies == null)
                {
                    Console.WriteLine("\ncookie: null");
                    return;
                }

                Console.WriteLine($"\ncookie: {cookies}");
                await hubConnection.SendAsync("OnData", randKey, Encoding.UTF8.GetBytes(cookies));
            });
            #endregion

            await hubConnection.StartAsync();
        }
        #endregion

        #region HubConnection_Closed
        async static Task HubConnection_Closed(Exception arg)
        {
            DateTime startReConnection = DateTime.Now;
            ReConnection: Console.WriteLine("\nReConnection..");

            if (startReConnection.AddMinutes(2) > DateTime.Now)
            {
                Console.WriteLine("ReConnected: false\nTimeoutReConnected > 2min ;(");
                return;
            }

            try
            {
                BuildOrReBuldHub();
                await StartAsync();
            }
            catch { await Task.Delay(2000); }

            if (hubConnection.State == HubConnectionState.Connected)
            {
                Console.Clear();
                Console.WriteLine("Connected: true");
            }
            else 
            {
                Console.WriteLine("ReConnected: false");
                goto ReConnection; 
            }
        }
        #endregion
    }
}
