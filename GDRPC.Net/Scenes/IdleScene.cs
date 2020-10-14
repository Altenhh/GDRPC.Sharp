using System.Collections.Generic;
using GDRPC.Net.Information;
using GDRPC.Net.Memory;

namespace GDRPC.Net.Scenes
{
    public class IdleScene : RpcScene
    {
        public override IEnumerable<GameScene> Scenes => new[] { GameScene.MainMenu, GameScene.Unknown };

        public IdleScene()
            : base()
        {
        }

        public IdleScene(GdReader reader, DiscordClient client, GdProcessState state)
            : base(reader, client, state)
        {
        }

        public override void Pulse()
        {
            Client.ChangeStatus(s =>
            {
                s.Details = string.Empty;
                s.State = "In menus";
                s.Timestamps = null;
            });
        }
    }
}