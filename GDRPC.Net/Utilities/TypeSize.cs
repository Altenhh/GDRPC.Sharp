using System;
using System.Collections.Generic;

namespace GDRPC.Net.Utilities
{
    public static class TypeSize
    {
        private static readonly Dictionary<Type, int> sizes = new Dictionary<Type, int>();

        static TypeSize()
        {
            RegisterSize<byte>();
            RegisterSize<sbyte>();
            RegisterSize<short>();
            RegisterSize<ushort>();
            RegisterSize<int>();
            RegisterSize<uint>();
            RegisterSize<long>();
            RegisterSize<ulong>();
            RegisterSize<bool>();
            RegisterSize<char>();
            RegisterSize<float>();
            RegisterSize<double>();

            static unsafe void RegisterSize<T>()
                where T : unmanaged
            {
                sizes.Add(typeof(T), sizeof(T));
            }
        }

        public static int GetSize<T>()
            where T : unmanaged =>
            GetSize(typeof(T));

        public static int GetSize(Type type) => sizes[type];
    }
}
