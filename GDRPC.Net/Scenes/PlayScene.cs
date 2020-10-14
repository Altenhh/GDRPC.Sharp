using System.Collections.Generic;
using DiscordRPC;
using GDRPC.Net.Information;

namespace GDRPC.Net.Scenes
{
    public class PlayScene : RpcScene
    {
        public override IEnumerable<Scene> Scene => new[] {Information.Scene.Play, Information.Scene.TheChallenge, Information.Scene.OfficialLevel};

        public override void Pulse()
        {
            if (!Client.presence.HasTimestamps())
                Client.ChangeStatus(s => s.WithTimestamps(Timestamps.Now));

            Client.ChangeStatus(s => s.Details = State.LevelInfo.ToString());

            Client.ChangeStatus(s =>
                s.State =
                    $"{State.LevelInfo.CompletionProgress}% | {GetCoinString()} Att: {State.LevelInfo.TotalAttempts:N0} | Jumps: {State.LevelInfo.Jumps:N0} | Score: {State.LevelInfo.CalculateScore():N0} ({State.LevelInfo.CalculatePerformance():N} pp)");
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