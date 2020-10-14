using System.Linq;

namespace GDRPC.Net
{
    public sealed class AddressEntry
    {
        public AddressEntry()
        {
        }

        public AddressEntry(string name)
        {
            Name = name;
        }

        public AddressEntry(string name, int[] offsets, string type)
        {
            Name = name;
            Offsets = offsets;
            Type = type;
        }

        public string Name { get; set; }
        public int[] Offsets { get; set; }
        public string Type { get; set; }

        public override string ToString() =>
            $"{Name} - {{ {string.Join(", ", Offsets.Select(o => $"0x{o:X}"))} }} - {Type}";
    }
}