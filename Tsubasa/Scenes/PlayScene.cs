using System.Collections.Generic;
using DiscordRPC;
using Tsubasa.Information;
using Tsubasa.Memory;

namespace Tsubasa.Scenes
{
    public class PlayScene : RpcScene
    {
        public override IEnumerable<GameScene> Scenes =>
            new[] { GameScene.Play, GameScene.TheChallenge, GameScene.OfficialLevel };

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
            var playerProgress = State.PlayerState.X / info.Length * 100;

            if (playerProgress > info.CompletionProgress)
                info.CompletionProgress = (int) playerProgress;

            Client.ChangeStatus(s => s.Details = info.ToString());

            Client.ChangeStatus(s =>
                s.State =
                    $"{info.CompletionProgress}% | {GetCoinString()} {(info.Id != 0 ? $"Score: {info.CalculateScore():N0} ({info.CalculatePerformance():N} pp)" : "")}");

            if (info.Id != 0)
                Client.ChangeStatus(s =>
                {
                    s.Buttons = new[]
                    {
                        new Button
                        {
                            Label = "Level page",
                            Url = $"https://gdbrowser.com/{State.LevelInfo.Id}"
                        }
                    };
                });
        }

        public override void Dispose()
        {
            base.Dispose();

            Client.ChangeStatus(s => { s.Buttons = null; });
        }

        private string GetCoinString()
        {
            var result = string.Empty;

            for (var i = 0; i < State.LevelInfo.MaxCoins; i++)
                result += State.LevelInfo.CoinsGrabbed[i] ? "C" : "-";

            if (!string.IsNullOrEmpty(result) && State.LevelInfo.Id != 0)
                result += " |";

            return result;
        }
    }
}