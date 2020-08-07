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
            RsClient.OnLog += log => TbLog.Text += log + Environment.NewLine;
            RsClient.OnClearLog += () => TbLog.Text = string.Empty;

            if (RsClient.HubConnection != null && RsClient.HubConnection.State == HubConnectionState.Connected)
                TbLog.Text = "Connected: true";
        }
    }
}
