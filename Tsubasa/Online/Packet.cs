using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Tsubasa.Online
{
    public class Packet
    {
        private byte[] _raw;
        private List<byte> _write = new List<byte>();

        // reader
        private BinaryReader _reader;

        // writer
        private MemoryStream _stream;
        private BinaryWriter _writer;

        // some data we would use
        public short Id;
        public int Length => _raw.Length;

        public int Offset = 0;

        public byte[] RawData => _raw;

        public Packet()
        {
            Id = 0;
            _raw = new byte[0];

            _stream = new MemoryStream(_raw);
            _writer = new BinaryWriter(_stream);
        }

        public Packet(short _id)
        {
            Id = _id;
            _raw = new byte[0];
        }

        public Packet(byte[] rawData)
        {
            _raw = rawData;
            _reader = new BinaryReader(new MemoryStream(_raw));

            // first we should get data
            Id = Read<short>();

            // turn the raw data into data since the first few bytes are essentially useless 
            _raw = new byte[rawData.Length - 2]; // negate first 2 bytes now
            Array.Copy(rawData, 2, _raw, 0, rawData.Length - 2);

            // reset the reader
            _reader = new BinaryReader(new MemoryStream(_raw));
        }

        public void Write<T>(T data)
        {
            byte[] setter = new byte[4];

            if (data == null)
                return;

            // data
            if (typeof(T) == typeof(string))
            {
                var dataString = data as string;
                var length = ULEB128.WriteLEB128Unsigned(dataString.Length);
                var bytes = Encoding.UTF8.GetBytes(dataString);

                // create our buffer
                if (string.IsNullOrEmpty(dataString))
                {
                    setter = new byte[] { 0 };
                } else
                {
                    setter = new byte[dataString.Length + (1 + length.Length)];
                    setter[0] = 0x0B; // set string indicator

                    // copy length to buffer
                    Array.Copy(length, 0, setter, 1, length.Length);

                    // copy data tobuffer
                    Array.Copy(bytes, 0, setter, length.Length + 1, bytes.Length);
                }
            }
            else
            {
                // data
                int size = SizeOf<T>();

                // create two buffers
                setter = new byte[size]; // managed buffer
                var ptr = Marshal.AllocHGlobal(size); // unmanaged buffer

                // copy bytes
                Marshal.StructureToPtr(data, ptr, false);

                // Copy data from unmanaged to managed
                Marshal.Copy(ptr, setter, 0, size);

                // Release unmanaged memory.
                Marshal.FreeHGlobal(ptr);
            }

            byte[] res = new byte[_raw.Length + setter.Length];

            // copy main to result
            Array.Copy(_raw, 0, res, 0, _raw.Length);
            // copy data to res
            Array.Copy(setter, 0, res, Offset, setter.Length);
            
            Offset += setter.Length;

            // reset the reader
            _raw = res;
            _reader = new BinaryReader(new MemoryStream(_raw));
        }

        public byte[] Pack()
        {
            byte[] res = new byte[_raw.Length + 2]; // ID + raw length

            // copy data to response
            Array.Copy(BitConverter.GetBytes(Id), 0, res, 0, 2); // copy id
            Array.Copy(_raw, 0, res, 2, _raw.Length); // copy raw data

            return res;
        }

        public T Read<T>()
        {
            Type type = typeof(T);
            BinaryFormatter bf = new BinaryFormatter();
            byte[] bytes = new byte[0];

            if (type == typeof(string))
            {
                if (_reader.ReadByte() == 0x0B) // there is a string
                {
                    int i = 0;
                    var len = _reader.ReadLEB128Unsigned();
                    bytes = new byte[len.Item1];
                    _reader.BaseStream.Seek(-1, SeekOrigin.Current);

                    while (i < len.Item1)
                    {
                        bytes[i] = _reader.ReadByte();
                        i++;
                    }
                }
            }
            else
            {
                int size = SizeOf<T>();
                bytes = _reader.ReadBytes(size);

                // cringe
                Array.Reverse(bytes);
            }

            // add to offset
            return Transfer<T>(bytes);
        }

        public static int SizeOf<T>()
        {
            Type type = typeof(T);
            
            return Marshal.SizeOf(type);
        }

        public static T Transfer<T>(byte[] data)
        {
            Type type = typeof(T);
            object d;

            if (type == typeof(byte))
                d = data[0];
            else if (type == typeof(ushort))
                d = BitConverter.ToUInt16(data, 0);
            else if (type == typeof(short))
                d = BitConverter.ToInt16(data, 0);
            else if (type == typeof(uint))
                d = BitConverter.ToUInt32(data, 0);
            else if (type == typeof(int))
                d = BitConverter.ToInt32(data, 0);
            else if (type == typeof(ulong))
                d = BitConverter.ToUInt64(data, 0);
            else if (type == typeof(long))
                d = BitConverter.ToInt64(data, 0);
            else if (type == typeof(float))
                d = BitConverter.ToSingle(data, 0);
            else if (type == typeof(double))
                d = BitConverter.ToDouble(data, 0); // oh no!!!!! double floating point values suck
            else if (type == typeof(string))
                d = Encoding.UTF8.GetString(data);
            else
                d = default(T);

            return (T) d;
        }
    }
}