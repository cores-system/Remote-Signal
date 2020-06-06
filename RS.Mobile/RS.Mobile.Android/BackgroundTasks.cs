using Android.App;
using Android.Content;
using Microsoft.AspNetCore.SignalR.Client;
using RS.Shared;

namespace RS.Mobile.Droid
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "com.rs.mobile" })]
    public class BackgroundTasks : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (RsClient.hubConnection == null || RsClient.hubConnection.State != HubConnectionState.Connected)
            {
                RsClient.BuildOrReBuldHub();
                RsClient.StartAsync();
            }
        }
    }
}