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
            if (RsClient.HubConnection == null || RsClient.HubConnection.State != HubConnectionState.Connected)
            {
                RsClient.BuildOrReBuildHub();
                RsClient.StartAsync();
            }
        }
    }
}