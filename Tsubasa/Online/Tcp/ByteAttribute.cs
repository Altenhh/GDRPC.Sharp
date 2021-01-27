using System;

namespace Tsubasa.Online.Tcp
{
    public class ByteAttribute : Attribute
    {
        public ByteLength Length = ByteLength.Short;
        public int Offset = 0;
    }

    public enum ByteLength
    {
        Variable = 0,
        Byte = 1,
        Short = 2,
        Integer = 4,
        Long = 8
    }
}