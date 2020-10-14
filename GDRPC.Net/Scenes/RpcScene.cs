using System.Collections.Generic;
using GDRPC.Net.Information;
using GDRPC.Net.Memory;

namespace GDRPC.Net.Scenes
{
    public abstract class RpcScene
    {
        public GdReader Reader { get; set; }
        public DiscordClient Client { get; set; }
        public GdProcessState State { get; set; }
        // TODO: This should utilize the Flag system.
        public abstract IEnumerable<GameScene> Scenes { get; }

        public abstract void Pulse();
    }
}