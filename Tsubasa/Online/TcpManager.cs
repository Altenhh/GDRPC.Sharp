using System;
using System.Net.Sockets;
using System.Text;
using Tsubasa.Online.Tcp;

namespace Tsubasa.Online
{
    public class TcpManager : IDisposable
    {
        public const int MAX_PACKET_SIZE = 1024;
        private readonly TcpClient _client = new TcpClient();
        private NetworkStream _stream;
        public string Hostname { get; set; }
        public int Port { get; set; }
        public bool Connected => _client.Connected;
        public bool Closed { get; set; }

        private Packet buildPacket;

        public TcpManager(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;
        }

        public TcpManager()
        {
        }

        public void Connect(string hostname, int port)
        {
            _client.Connect(hostname, port);
            _stream = _client.GetStream();

            OnOpen();
        }

        public void Connect()
        {
            Connect(Hostname, Port);
        }

        public TcpCloseReason Close()
        {
            Closed = true;
            _client.Close();

            OnDisconnect(TcpCloseReason.User);

            return TcpCloseReason.User;
        }

        public void Read()
        {
            if (!Connected)
                Connect();

            while (true)
            {
                byte[] rawData = new byte[MAX_PACKET_SIZE];
                int bytesRead = _stream.Read(rawData, 0, rawData.Length);

                if (bytesRead > 0)
                {
                    var response = Handle(rawData);

                    OnPacketRecieved(new PacketRecievedEventArgs()
                    {
                        Packet = response
                    });

                    rawData = new byte[MAX_PACKET_SIZE]; // reset raw data
                }

                if (!Connected)
                {
                    if (!Closed)
                    {
                        OnDisconnect(TcpCloseReason.Server);
                    }

                    break;
                }
            }
        }

        private Packet Handle(byte[] rawData)
        {
            return null;
        }

        public Packet ReadNext()
        {
            byte[] rawData = new byte[MAX_PACKET_SIZE];
            int bytesRead = _stream.Read(rawData, 0, rawData.Length);

            if (bytesRead > 0)
            {
                return new Packet(rawData);
            }

            return null;
        }

        // insert packet shit here
        public void Write(byte[] data)
        {
            if (!Connected)
                return;

            _stream.Write(data, 0, data.Length);
        }

        public void Write(string data)
        {
            Write(Encoding.ASCII.GetBytes(data));
        }

        public void Write(Packet packet)
        {
            if (!Connected)
                return;

            var packed = packet.Pack();
            _stream.Write(packed, 0, packed.Length);
        }

        public void StartPacket(RequestId id)
        {
            buildPacket = new Packet { Id = (int) id };
        }

        public void WritePacket<T>(T data)
        {
            buildPacket.Write(data);
        }

        public void WritePacket(params byte[] data)
        {
            buildPacket.Write(data);
        }

        public void EndPacket()
        {
            Write(buildPacket);
        }

        #region IDisposable Implementation
        public void Dispose()
        {
            _client.Dispose();
            OnDisconnect(TcpCloseReason.Disposed);
        }
        #endregion

        #region Events
        protected virtual void OnPacketRecieved(PacketRecievedEventArgs e)
        {
            PacketRecieved?.Invoke(this, e);
        }

        protected virtual void OnOpen()
        {
            Open?.Invoke(this, null);
        }

        protected virtual void OnDisconnect(TcpCloseReason reason)
        {
            Disconnect?.Invoke(this, reason);
        }

        // Events
        public event EventHandler<PacketRecievedEventArgs> PacketRecieved;
        public event EventHandler Open;
        public event EventHandler<TcpCloseReason> Disconnect;
        #endregion

        private string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        private string ToReadableByteArray(byte[] bytes)
        {
            return string.Join(", ", bytes);
        }
    }

    public enum TcpCloseReason
    {
        Server,
        User,
        Disposed
    }
}