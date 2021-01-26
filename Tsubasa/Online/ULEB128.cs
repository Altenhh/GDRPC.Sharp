// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.IO;

namespace Tsubasa.Online
{
    public static class ULEB128
    {
        public static byte[] WriteLEB128Unsigned (int value)
        {
            var array = new List<byte>();
            var length = 0;

            if (value == 0)
                return new byte[] { 0x0 };

            while (value > 0)
            {
                array.Add((byte)(value & 0x7F));

                if (value >> 7 != 0)
                {
                    value >>= 7; // bitshift
                    array[length] |= 0x80;
                } else
                {
                    break;
                }

                length++;
            }

            return array.ToArray();
        }
        
        public static (int, int) ReadLEB128Unsigned (this BinaryReader reader) => ReadLEB128Unsigned(reader, out _);

        public static (int, int) ReadLEB128Unsigned (this BinaryReader reader, out int bytes) {
            bytes = 0;

            int value = 0;
            int shift = 0;
            int len = 0;

            while (true)
            {
                var next = reader.ReadByte();
                if (next < 0) { throw new InvalidOperationException("Unexpected end of reader"); }

                len++;
                value |= ((next & 0x7F) << shift);
                if ((reader.ReadByte() & 0x80) == 0) break;
                shift += 7;
            }
            
            return (value, len);
        }
    }
}