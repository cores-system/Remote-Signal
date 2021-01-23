using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RS.Shared
{
    public class RsClient
    {
        #region RsClient
        public static HubConnection hubConnection;

        public static Action<string> OnLog;

        public static Action OnClearLog, OnDestroy;
        #endregion

        #region BuildOrReBuldHub
        public static void BuildOrReBuldHub()
        {
            if (hubConnection != null)
                hubConnection.Closed -= HubConnection_Closed;

            if (hubConnection != null)
                hubConnection.Reconnecting -= HubConnection_Reconnecting;

            if (hubConnection != null)
                hubConnection.Reconnected -= HubConnection_Reconnected;

            hubConnection = new HubConnectionBuilder().WithUrl("http://nserv.host:5300/hubs/rs").Build();
            hubConnection.HandshakeTimeout = TimeSpan.FromSeconds(8);
            hubConnection.Closed += HubConnection_Closed;
            hubConnection.Reconnecting += HubConnection_Reconnecting;
            hubConnection.Reconnected += HubConnection_Reconnected;   

            hubConnection.On("OnConnected", async (string supportVerion, string connectionId) =>
            {
                if (supportVerion != "23012021")
                {
                    OnDestroy();
                    OnClearLog();
                    OnLog($"Версия больше не поддерживается, обновите до актуальной (v{supportVerion}){Environment.NewLine}\thttp://rs.nserv.host/");
                    hubConnection.Closed -= HubConnection_Closed;
                    hubConnection.Reconnecting -= HubConnection_Reconnecting;
                    hubConnection.Reconnected -= HubConnection_Reconnected;
                    await hubConnection.StopAsync();
                }
                else
                {
                    if ((await HttpClient.Get($"http://nserv.host:5300/forkapi/registryrs?connectionId={connectionId}", null)) != null)
                    {
                        OnClearLog();
                        OnLog($"Connected: true{Environment.NewLine}Id: {connectionId}");
                    }
                }
            });
        }
        #endregion

        #region StartAsync
        async public static Task StartAsync()
        {
            #region OnGet
            hubConnection.On("OnGet", async (string randKey, string url, string addHeaders) =>
            {
                OnLog($"{Environment.NewLine}GET | {url}");

                byte[] buffer = await HttpClient.Get(url, JsonConvert.DeserializeObject<List<(string name, string val)>>(addHeaders));
                if (buffer == null)
                {
                    OnLog("\tbufferLength: 0 / ошибка");
                    return;
                }

                OnLog($"\tbufferLength: {buffer.Length}");
                await hubConnection.SendAsync("OnData", randKey, buffer);
            });
            #endregion

            #region OnPost
            hubConnection.On("OnPost", async (string randKey, string url, string content, string encodingName, string mediaType, string addHeaders) =>
            {
                OnLog($"{Environment.NewLine}POST | {url}\n\t{content}");

                byte[] buffer = await HttpClient.Post(url, new System.Net.Http.StringContent(content, Encoding.GetEncoding(encodingName), mediaType), JsonConvert.DeserializeObject<List<(string name, string val)>>(addHeaders));
                if (buffer == null)
                {
                    OnLog("\tbufferLength: 0 / ошибка");
                    return;
                }

                OnLog($"\tbufferLength: {buffer.Length}");
                await hubConnection.SendAsync("OnData", randKey, buffer);
            });
            #endregion

            #region OnLogin
            hubConnection.On("OnLogin", async (string randKey, string url, string content, string encodingName, string mediaType, string addHeaders) =>
            {
                OnLog($"{Environment.NewLine}TakeLogin | {url}\n\t{content}");

                string cookies = await HttpClient.TakeLogin(url, new System.Net.Http.StringContent(content, Encoding.GetEncoding(encodingName), mediaType), JsonConvert.DeserializeObject<List<(string name, string val)>>(addHeaders));
                if (cookies == null)
                {
                    OnLog("\tcookie: null");
                    return;
                }

                OnLog($"\tcookie: {cookies}");
                await hubConnection.SendAsync("OnData", randKey, Encoding.UTF8.GetBytes(cookies));
            });
            #endregion

            #region OnLocation
            hubConnection.On("OnLocation", async (string randKey, string url, bool encodeArgs, string addHeaders) =>
            {
                OnLog($"{Environment.NewLine}Location | {url}");

                string location = await HttpClient.Location(url, encodeArgs, JsonConvert.DeserializeObject<List<(string name, string val)>>(addHeaders));
                if (location == null)
                {
                    OnLog("\turi: null");
                    return;
                }

                OnLog($"\turi: {location}");
                await hubConnection.SendAsync("OnData", randKey, Encoding.UTF8.GetBytes(location));
            });
            #endregion

            await hubConnection.StartAsync();
        }
        #endregion


        #region HubConnection_Closed
        async static Task HubConnection_Closed(Exception arg)
        {
            OnLog($"{Environment.NewLine}Connection closed..");
            ReConnection: OnLog($"{Environment.NewLine}Restart connection..");

            try
            {
                BuildOrReBuldHub();
                await StartAsync();
            }
            catch { await Task.Delay(15_000); }

            if (hubConnection.State != HubConnectionState.Connected)
            {
                OnLog("ReConnected: false");
                await Task.Delay(2_000);
                goto ReConnection;
            }
        }
        #endregion

        #region HubConnection_Reconnecting
        private static Task HubConnection_Reconnecting(Exception arg)
        {
            OnLog($"{Environment.NewLine}Reconnecting..");
            return Task.CompletedTask;
        }
        #endregion

        #region HubConnection_Reconnected
        async private static Task HubConnection_Reconnected(string connectionId)
        {
            if ((await HttpClient.Get($"http://nserv.host:5300/forkapi/registryrs?connectionId={connectionId}", null)) != null)
            {
                OnClearLog();
                OnLog($"Reconnected: true{Environment.NewLine}Id: {connectionId}");
            }
        }
        #endregion
    }
}
