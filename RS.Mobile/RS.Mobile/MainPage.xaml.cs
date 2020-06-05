using RS.Shared;
using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace RS.Mobile
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async private void ContentPage_Appearing(object sender, EventArgs e)
        {
            RsClient.OnDestroy += () => { };
            RsClient.OnLog += log => tb_log.Text += log + Environment.NewLine;
            RsClient.OnClearLog += () => tb_log.Text = string.Empty;

            RsClient.BuildOrReBuldHub();
            await RsClient.StartAsync();
        }
    }
}
