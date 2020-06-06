using Microsoft.AspNetCore.SignalR.Client;
using RS.Shared;
using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace RS.Mobile
{
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            RsClient.OnDestroy += () => { };
            RsClient.OnLog += log => tb_log.Text += log + Environment.NewLine;
            RsClient.OnClearLog += () => tb_log.Text = string.Empty;

            if (RsClient.hubConnection != null && RsClient.hubConnection.State == HubConnectionState.Connected)
                tb_log.Text = "Connected: true";
        }
    }
}
