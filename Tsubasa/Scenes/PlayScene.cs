using System.Collections.Generic;
using DiscordRPC;
using Tsubasa.Information;
using Tsubasa.Memory;

namespace Tsubasa.Scenes
{
    public class PlayScene : RpcScene
    {
        public override IEnumerable<GameScene> Scenes => new[] { GameScene.Play, GameScene.TheChallenge, GameScene.OfficialLevel };

        public PlayScene()
            : base()
        {
        }

        public PlayScene(GdReader reader, DiscordClient client, GdProcessState state)
            : base(reader, client, state)
        {
        }

        public override void Pulse()
        {
            if (!Client.presence.HasTimestamps())
                Client.ChangeStatus(s => s.WithTimestamps(Timestamps.Now));

            var info = State.LevelInfo;

            Client.ChangeStatus(s => s.Details = info.ToString());

            Client.ChangeStatus(s =>
                s.State =
                    $"{info.CompletionProgress}% | {GetCoinString()} Score: {info.CalculateScore():N0} ({info.CalculatePerformance():N} pp)");
        }

        private string GetCoinString()
        {
            var result = string.Empty;

            for (var i = 0; i < State.LevelInfo.MaxCoins; i++)
                result += State.LevelInfo.CoinsGrabbed[i] ? "C" : "-";

            if (!string.IsNullOrEmpty(result))
                result += " |";

            return result;
        }
    }
}