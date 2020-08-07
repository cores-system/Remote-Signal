using System;
using System.Threading.Tasks;
using RS.Shared;

namespace RemoteSignal
{
    class Program
    {
        async static Task Main(string[] args)
        {
            RsClient.OnDestroy += () => Console.ForegroundColor = ConsoleColor.Red;
            RsClient.OnLog += Console.WriteLine;

            RsClient.OnClearLog += () =>
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    Console.Clear();
            };

            Console.WriteLine("Connection..");
            RsClient.BuildOrReBuildHub();
            await RsClient.StartAsync();

            do
            {
                Console.ReadLine();
                await Task.Delay(40);
            } 
            while (Environment.OSVersion.Platform == PlatformID.Unix);
        }
    }
}
