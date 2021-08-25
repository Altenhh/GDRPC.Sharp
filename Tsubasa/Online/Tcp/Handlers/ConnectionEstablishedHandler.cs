using static Tsubasa.Helper;

namespace Tsubasa.Online.Tcp.Handlers
{
    public class ConnectionEstablishedHandler : Handler
    {
        public ConnectionEstablishedHandler(Packet packet)
            : base(packet)
        {
        }

        public override PacketIds Id => PacketIds.ConnectionEstablished;

        public override void Handle(Packet packet)
        {
            Write("[tcp] Connection successfully established!");
        }
    }
}