using System;
using NUnit.Framework;
using Tsubasa.Online;

namespace Tsubasa.Tests
{
    public class PacketTests
    {
        [Test]
        public void TestWritePacket()
        {
            var packet = new Packet();
        
            packet.Write(1);
        
            Console.WriteLine("Write");
            Console.WriteLine(packet.RawData[0]);
            Console.WriteLine();
        }

        [Test]
        public void TestReadPacket()
        {
            var packet = new byte[] { 2, 0, 0, 0, 0, 1, 1, 1 };
            Packet testPacket = new Packet(packet);

            byte data = testPacket.Read<byte>();
            Assert.AreEqual(packet[5], data);
            
            data = testPacket.Read<byte>();
            Assert.AreEqual(packet[6], data);
        }

        [Test]
        public void TestWritePacketString()
        {
            var packet = new Packet();
            
            packet.Write("Help");
            var packed = packet.Pack();
            
            foreach (var b in packed)
            {
                Console.Write($"{b} ");
            }
        }

        [Test]
        public void TestReadPacketString()
        {
            // id, len, data
            var packet = new byte[] { 2, 0, 11, 4, 0x48, 0x65, 0x6C, 0x70  };
            var testPacket = new Packet(packet);

            var data = testPacket.Read<string>();
            Assert.AreEqual("Help", data);
        }
    }
}
