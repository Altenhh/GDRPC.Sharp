using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Binarysharp.MemoryManagement;
using static GDRPC.Net.Helper;

namespace GDRPC.Net
{
    public static class Program
    {
        private static Process gdProcess;

        public static void Main(string[] args)
        {
            GetGdProcess(args);
            
            var memory = new MemorySharp(gdProcess);

            if (gdProcess == null)
            {
                Write("Failed to hook onto process", ConsoleColor.Red);
                return;
            }
            
            Write($"Hooked onto process: {gdProcess.MainWindowTitle} ({gdProcess.Id})");
            
            // test level reading
            var address = new IntPtr(0x3222D0);
            int[] offsets = { 0x2A0 };

            address = memory[address].Read<IntPtr>();

            var levelByte = memory[address + offsets[0], false].Read<int>();

            Write(levelByte.ToString());
        }
        
        private static void GetGdProcess(string[] args)
        {
            if (args.Length > 0 && args.Any(a => a == "--opengd" || a == "-o"))
            {
                const string path = @"C:\Program Files (x86)\Steam\steamapps\common\Geometry Dash";

                var processStartInfo = new ProcessStartInfo(path + @"\GeometryDash.exe")
                {
                    WorkingDirectory = path,
                    UseShellExecute = false
                };

                gdProcess = Process.Start(processStartInfo);

                // doesn't open immediately so we will have to wait a bit
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
            else
            {
                try
                {
                    gdProcess = Process.GetProcessesByName("GeometryDash")[0];
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}