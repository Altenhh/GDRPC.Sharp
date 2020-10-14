using System.Collections.Generic;
using GDRPC.Net.Information;

namespace GDRPC.Net.Scenes
{
    public class IdleScene : RpcScene
    {
        public override IEnumerable<GameScene> Scenes => new[] { GameScene.MainMenu, GameScene.Unknown };

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