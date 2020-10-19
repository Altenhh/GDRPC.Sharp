using System;
using System.Collections.Generic;
using DiscordRPC;
using GDRPC.Net.Information;
using GDRPC.Net.Memory;

namespace GDRPC.Net.Scenes
{
    public class EditorScene : RpcScene
    {
        // Don't ever fetch this scene, let our RpcDisplay update functions handle if we should be on this scene or not.
        public override IEnumerable<GameScene> Scenes => ArraySegment<GameScene>.Empty;
        
        public EditorScene()
            : base()
        {
        }

        public EditorScene(GdReader reader, DiscordClient client, GdProcessState state)
            : base(reader, client, state)
        {
        }

        public override void Pulse()
        {
            if (!Client.presence.HasTimestamps())
                Client.ChangeStatus(s => s.WithTimestamps(Timestamps.Now));

            var objCount = Reader.Read<int>(0x168, 0x3A0);
            var title = Reader.ReadString(0x168, 0x124, 0xEC, 0x110, 0x114, 0xFC);
            
            Client.ChangeStatus(s =>
            {
                s.Details = $"Editing: {title}";
                s.State = $"{GetCoinString()} Objects: {objCount}";
            });
        }

        // This whole functions gets obsolete whenever the user places a coin.
        private string GetCoinString()
        {
            var coinCount = Reader.Read<int>(0x168, 0x124, 0xEC, 0x110, 0x114, 0x2B4);
            var @string = new string('-', coinCount);

            if (!string.IsNullOrEmpty(@string))
                @string += " |";

            return @string;
        }
    }
}