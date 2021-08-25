using System;
using System.Linq;
using System.Reflection;

namespace Tsubasa.Online.Tcp
{
    public abstract class Handler
    {
        private readonly Packet packet;
        public abstract PacketIds Id { get; }

        protected Handler(Packet packet)
        {
            this.packet = packet;
        }

        public abstract void Handle(Packet packet);

        public static void Construct(Packet packet)
        {
            var handler = Assembly
               .GetExecutingAssembly()
               .GetTypes()
               .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Handler)))
               .Select(t => (Handler) Activator.CreateInstance(t, packet))
               .FirstOrDefault(h => h != null && h.Id == (PacketIds) packet.Id);

            handler?.Handle(packet);
        }
    }
}