using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GDRPC.Net.Memory
{
    // Do not extend interfaces, this implementation is not intended to support generalized functions
    // For once I'm not overengineering pepega
    public class AddressDictionary
    {
        private readonly Dictionary<string, AddressEntry> dictionary = new Dictionary<string, AddressEntry>();
        public AddressEntry this[string key] => dictionary[key];

        public void Add(AddressEntry entry)
        {
            dictionary.Add(entry.Name, entry);
        }

        public static AddressDictionary Parse(string contents)
        {
            // Too lazy to import the Garyon extension for splitting lines
            // And why the fuck are we not using .NET Core?
            var lines = contents.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

            var result = new AddressDictionary();
            AddressEntry currentEntry = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                    continue;

                if (line.Length == 0)
                {
                    result.Add(currentEntry);
                    currentEntry = null;

                    continue;
                }

                if (line.StartsWith("["))
                {
                    // New entry registration
                    currentEntry = new AddressEntry(line.Substring(1, line.Length - 2).Trim());

                    continue;
                }

                var colonIndex = line.IndexOf(':');
                var property = line.Substring(0, colonIndex);
                var value = line.Substring(colonIndex + 2);

                switch (property)
                {
                    case "offsets":
                        currentEntry.Offsets = value.Replace(" ", "").Replace("0x", "").Split('|')
                           .Select(o => int.Parse(o, NumberStyles.HexNumber)).ToArray();

                        break;

                    case "valueType":
                        currentEntry.Type = value;

                        break;
                }
            }

            if (currentEntry != null)
                result.Add(currentEntry);

            return result;
        }
    }
}