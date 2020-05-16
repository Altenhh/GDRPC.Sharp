using System;
using DiscordRPC;
using DiscordRPC.Message;
using static GDRPC.Net.Helper;

namespace GDRPC.Net
{
    public class DiscordClient : IDisposable
    {
        private const string client_id = "419568479172034561";

        private readonly DiscordRpcClient client;

        private RichPresence presence = new RichPresence();

        public event Action OnReady;

        public DiscordClient()
        {
            client = new DiscordRpcClient(client_id)
            {
                SkipIdenticalPresence = false
            };

            client.OnReady += onReady;

            client.OnConnectionFailed += (_, __) => client.Deinitialize();

            client.OnError += (_, e) => Write($"An error occurred with Discord RPC Client: {e.Code} {e.Message}");

            client.Initialize();
        }

        public void ChangeStatus(Action<RichPresence> newStatus)
        {
            newStatus(presence);
            client.SetPresence(presence);
        }

        private void onReady(object _, ReadyMessage __)
        {
            Write("Discord RPC Client ready.");

            OnReady?.Invoke();
            client.SetPresence(presence);
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}