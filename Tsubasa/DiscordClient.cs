using System;
using DiscordRPC;
using DiscordRPC.Message;
using static Tsubasa.Helper;

namespace Tsubasa
{
    public class DiscordClient : IDisposable
    {
        private const string client_id = "419568479172034561";
        private readonly DiscordRpcClient client;
        public RichPresence presence = new RichPresence();

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

        public void Dispose()
        {
            client.Dispose();
        }

        public event Action OnReady;

        public void ChangeStatus(Action<RichPresence> newStatus)
        {
            newStatus(presence);
        }

        public void Update()
        {
            client.SetPresence(presence);
        }

        private void onReady(object _, ReadyMessage __)
        {
            Write("Discord RPC Client ready.");

            OnReady?.Invoke();
            client.SetPresence(presence);
        }
    }
}