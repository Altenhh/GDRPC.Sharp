using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GDRPC.Net.Utilities
{
    public class FlagsEnumDictionary<TKey, TValue>
        where TKey : unmanaged, Enum
        // Sadly you cannot constrain the enum types that have the FlagsAttribute
    {
        private readonly TValue[] values;

        public FlagsEnumDictionary()
        {
            var flags = typeof(TKey).GetCustomAttribute<FlagsAttribute>();
            if (flags == null)
                throw new ArgumentException("The enum must be marked with the FlagsAttribute.");

            var underlyingType = Enum.GetUnderlyingType(typeof(TKey));
            int size = TypeSize.GetSize(underlyingType);

            values = new TValue[size * 8];
        }

        public void Add(TKey key, TValue value)
        {
            var indices = GetIndices(key);
            foreach (var i in indices)
                values[i] = value; 
        }

        public TValue this[TKey key]
        {
            get
            {
                var indices = GetIndices(key);

                if (!indices.Any())
                    return default;

                var common = values[indices[0]];

                for (int i = 1; i < indices.Count; i++)
                    if (!values[indices[i]].Equals(common))
                        return default;

                return common;
            }
        }

        private unsafe List<int> GetIndices(TKey key)
        {
            var indices = new List<int>();

            ulong keyBits = *(ulong*)&key;
            ulong mask = 1;
            for (int i = 0; i < values.Length; i++, mask <<= 1)
            {
                if ((keyBits & mask) == 0)
                    continue;

                indices.Add(i);
            }

            return indices;
        }
    }
}
