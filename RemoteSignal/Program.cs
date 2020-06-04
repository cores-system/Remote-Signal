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

        async static Task Main(string[] args)
        {
            Console.WriteLine("Connection..");
            BuildOrReBuldHub();
            await StartAsync();

            do
            {
                Console.ReadLine();
                await Task.Delay(40);
            } 
            while (Environment.OSVersion.Platform == PlatformID.Unix);
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
                if (supportVerion != "04062020")
                {
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                    }

                    Console.WriteLine($"Версия больше не поддерживается, обновите до актуальной (v{supportVerion})\n\thttp://rs.nserv.host/");
                    hubConnection.Closed -= HubConnection_Closed;
                    await hubConnection.StopAsync();
                }
                else
                {
                    if (HttpClient.Get($"http://nserv.host:5300/forkapi/registryrs?connectionId={connectionId}", null) != null)
                    {
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                            Console.Clear();
                        Console.WriteLine($"Connected: true\nId: {connectionId}");
                    }
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
                    Console.WriteLine("\tcookie: null");
                    return;
                }

                Console.WriteLine($"\tcookie: {cookies}");
                await hubConnection.SendAsync("OnData", randKey, Encoding.UTF8.GetBytes(cookies));
            });
            #endregion

            #region OnLocation
            hubConnection.On("OnLocation", async (string randKey, string url, bool encodeArgs, string addHeaders) =>
            {
                Console.WriteLine($"\nGetLocation | {url}");

                string location = await HttpClient.Location(url, encodeArgs, JsonConvert.DeserializeObject<List<(string name, string val)>>(addHeaders));
                if (location == null)
                {
                    Console.WriteLine("\tlocation: null");
                    return;
                }

                Console.WriteLine($"\tlocation: {location}");
                await hubConnection.SendAsync("OnData", randKey, Encoding.UTF8.GetBytes(location));
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

            if (DateTime.Now.AddMinutes(-5) > startReConnection)
            {
                Console.WriteLine("ReConnected: false\nTimeoutReConnected > 5min ;(");
                return;
            }

            try
            {
                BuildOrReBuldHub();
                await StartAsync();
            }
            catch { await Task.Delay(8000); }

            if (hubConnection.State != HubConnectionState.Connected)
            {
                Console.WriteLine("ReConnected: false");
                goto ReConnection; 
            }
        }
        #endregion
    }
}
