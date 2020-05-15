using System;
using System.Diagnostics;
using System.Threading;
using Binarysharp.MemoryManagement;
using static GDRPC.Sharp.Helper;

namespace GDRPC.Sharp
{
    public static class Program
    {
        private static Process gdProcess;

        public static void Main(string[] args)
        {
            GetGdProcess(args);

            var memory = new MemorySharp(gdProcess);
        }

        private static void GetGdProcess(string[] args)
        {
            if (args.Length > 0 && args[0] == "--opengd")
            {
                var processStartInfo = new ProcessStartInfo(@"C:\Program Files (x86)\Steam\steamapps\common\Geometry Dash\GeometryDash.exe")
                {
                    WorkingDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Geometry Dash",
                    UseShellExecute = false
                };

                gdProcess = Process.Start(processStartInfo);

                // doesn't open immediately so we will have to wait a bit
                Thread.Sleep(TimeSpan.FromSeconds(10));

                Write($"Started process: {gdProcess?.Id} (Geometry Dash)");
            }
            else
            {
                try
                {
                    gdProcess = Process.GetProcessesByName("GeometryDash")[0];
                    Write($"Hooked onto process: {gdProcess.Id} ({gdProcess.MainWindowTitle})");
                }
                catch
                {
                    Write("Failed to hook onto process", ConsoleColor.Red);
                }
            }
        }
    }
}