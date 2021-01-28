using System;
using static Tsubasa.Helper;

namespace Tsubasa.Online.Tcp.Handlers
{
    public class PingHandler : Handler
    {
        public PingHandler(Packet packet)
            : base(packet)
        {
        }

        public override PacketIds Id => PacketIds.Pong;

        public override void Handle()
        {
            Write("Heartbeat successfully heard.", ConsoleColor.Green);
        }
    }
}