using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            
            // test noclip hack
            var on = new byte[] { 0xE9, 0x79, 0x06, 0x00, 0x00 };
            var off = new byte[] { 0x6A, 0x14, 0x8B, 0xCB, 0xFF };
            
            var address = new IntPtr(0x20A23C);
            
            memory.Write(address, on);
            
            var rpc = new DiscordClient();

            rpc.OnReady += () =>
            {
                rpc.ChangeStatus(s => s.Details = "hi");
                rpc.ChangeStatus(s => s.State = "fuck");
            };

            // will wait indefinitely
            Task.Delay(-1);
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