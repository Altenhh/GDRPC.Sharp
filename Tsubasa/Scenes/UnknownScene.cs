using System.Collections.Generic;
using Tsubasa.Information;
using Tsubasa.Memory;

namespace Tsubasa.Scenes
{
    public class UnknownScene : RpcScene
    {
        public override IEnumerable<GameScene> Scenes => new[] { GameScene.Unknown };

        public UnknownScene()
            : base()
        {
        }

        public UnknownScene(GdReader reader, DiscordClient client, GdProcessState state)
            : base(reader, client, state)
        {
        }

        public override void Pulse()
        {
            Client.ChangeStatus(s =>
            {
                s.Details = string.Empty;
                s.State = "Unknown state";
                s.Timestamps = null;
            });
        }
    }
}