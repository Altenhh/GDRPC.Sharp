using System;

namespace Tsubasa.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PacketAttribute : Attribute
    {
        public short Id;

        public PacketAttribute(short id)
        {
            Id = id;
        }
    }
}