using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RS.Shared;

namespace RS.Shared
{
    public class RsClient
    {
        #region RsClient
        public static HubConnection HubConnection;

        public static Action<string> OnLog;

        public static Action OnClearLog, OnDestroy;
        #endregion

        #region BuildOrReBuldHub
        public static void BuildOrReBuildHub()
        {
            if (HubConnection != null)
                HubConnection.Closed -= HubConnection_Closed;

            HubConnection = new HubConnectionBuilder().WithUrl("http://nserv.host:5300/hubs/rs").Build();
            HubConnection.HandshakeTimeout = TimeSpan.FromSeconds(5);
            HubConnection.Closed += HubConnection_Closed;

            HubConnection.On("OnConnected", async (string supportVersion, string connectionId) =>
            {
                if (supportVersion != "06062020")
                {
                    OnDestroy();
                    OnClearLog();
                    OnLog($"Версия больше не поддерживается, обновите до актуальной (v{supportVersion}){Environment.NewLine}\thttp://rs.nserv.host/");
                    HubConnection.Closed -= HubConnection_Closed;
                    await HubConnection.StopAsync();
                }
                else
                {
                    if (HttpClient.Get($"http://nserv.host:5300/forkapi/registryrs?connectionId={connectionId}", null).Result != null)
                    {
                        OnClearLog();
                        OnLog($"Connected: true{Environment.NewLine}Id: {connectionId}");
                    }
                }
            });
        }
        #endregion

        #region StartAsync
        public static async Task StartAsync()
        {
            #region OnGet
            HubConnection.On("OnGet", async (string randKey, string url, string addHeaders) =>
            {
                OnLog($"{Environment.NewLine}GET | {url}");

                byte[] buffer = await HttpClient.Get(url, JsonConvert.DeserializeObject<List<(string name, string val)>>(addHeaders));
                if (buffer == null)
                {
                    OnLog("\tbufferLength: 0 / ошибка");
                    return;
                }

                OnLog($"\tbufferLength: {buffer.Length}");
                await HubConnection.SendAsync("OnData", randKey, buffer);
            });
            #endregion

            #region OnPost
            HubConnection.On("OnPost", async (string randKey, string url, string content, string encodingName, string mediaType, string addHeaders) =>
            {
                OnLog($"{Environment.NewLine}POST | {url}\n\t{content}");

                byte[] buffer = await HttpClient.Post(url, new System.Net.Http.StringContent(content, Encoding.GetEncoding(encodingName), mediaType), JsonConvert.DeserializeObject<List<(string name, string val)>>(addHeaders));
                if (buffer == null)
                {
                    OnLog("\tbufferLength: 0 / ошибка");
                    return;
                }

                OnLog($"\tbufferLength: {buffer.Length}");
                await HubConnection.SendAsync("OnData", randKey, buffer);
            });
            #endregion

            #region OnLogin
            HubConnection.On("OnLogin", async (string randKey, string url, string content, string encodingName, string mediaType, string addHeaders) =>
            {
                OnLog($"{Environment.NewLine}TakeLogin | {url}\n\t{content}");

                string cookies = await HttpClient.TakeLogin(url, new System.Net.Http.StringContent(content, Encoding.GetEncoding(encodingName), mediaType), JsonConvert.DeserializeObject<List<(string name, string val)>>(addHeaders));
                if (cookies == null)
                {
                    OnLog("\tcookie: null");
                    return;
                }

                OnLog($"\tcookie: {cookies}");
                await HubConnection.SendAsync("OnData", randKey, Encoding.UTF8.GetBytes(cookies));
            });
            #endregion

            #region OnLocation
            HubConnection.On("OnLocation", async (string randKey, string url, bool encodeArgs, string addHeaders) =>
            {
                OnLog($"{Environment.NewLine}Location | {url}");

                string location = await HttpClient.Location(url, encodeArgs, JsonConvert.DeserializeObject<List<(string name, string val)>>(addHeaders));
                if (location == null)
                {
                    OnLog("\turi: null");
                    return;
                }

                OnLog($"\turi: {location}");
                await HubConnection.SendAsync("OnData", randKey, Encoding.UTF8.GetBytes(location));
            });
            #endregion

            await HubConnection.StartAsync();
        }
        #endregion

        #region HubConnection_Closed
        static async Task HubConnection_Closed(Exception arg)
        {
            DateTime startReConnection = DateTime.Now;
            ReConnection: OnLog($"{Environment.NewLine}ReConnection..");

            if (DateTime.Now.AddMinutes(-5) > startReConnection)
            {
                OnDestroy();
                OnLog($"ReConnected: false{Environment.NewLine}TimeoutReConnected > 5min ;(");
                return;
            }

            try
            {
                BuildOrReBuildHub();
                await StartAsync();
            }
            catch { await Task.Delay(8000); }

            if (HubConnection.State != HubConnectionState.Connected)
            {
                OnLog("ReConnected: false");
                goto ReConnection;
            }
        }
        #endregion
    }
}
