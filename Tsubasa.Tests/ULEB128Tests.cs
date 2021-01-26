// ReSharper disable InconsistentNaming

using System;
using NUnit.Framework;
using Tsubasa.Online;

namespace Tsubasa.Tests
{
    public class ULEB128Tests
    {
        [Test]
        public void TestWrite()
        {
            var val = new byte[] { 128, 23, 53, 43, 75 };

            foreach (var b in val)
            {
                Console.WriteLine(b);
                var res = ULEB128.WriteLEB128Unsigned(b);
                foreach (var re in res)
                {
                    Console.Write(re + " ");
                }

                Console.WriteLine();
            }
        }
    }
}