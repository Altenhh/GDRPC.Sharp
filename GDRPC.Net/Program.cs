using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Binarysharp.MemoryManagement;
using DiscordRPC;
using static GDRPC.Net.Helper;

namespace GDRPC.Net
{
    public static class Program
    {
        private static AddressDictionary addresses;

        private static Process gdProcess;
        private static DiscordClient rpc;
        private static MemorySharp memory;
        private static Scene currentScene;
        private static LevelInfo levelInfo = new LevelInfo();
        private static Scheduler scheduler;

        public static void Main(string[] args)
        {
            GetGdProcess(args);
            
            if (gdProcess == null)
            {
                Write("Failed to hook onto process", ConsoleColor.Red);
                return;
            }

            GetAddresses();
            Hook();
            InitializeRPC();

            while (true)
            {
                if (scheduler.Stopwatch.ElapsedMilliseconds < scheduler.Delay)
                    continue;
                
                scheduler.Stopwatch.Restart();
                scheduler.Pulse();
            }
        }

        private static void GetAddresses()
        {
            addresses = AddressDictionary.Parse(File.ReadAllText("Addresses.txt"));
        }
        private static void Hook()
        {
            memory = new MemorySharp(gdProcess);
            Write($"Hooked onto process: {gdProcess.MainWindowTitle} ({gdProcess.Id})");
        }
        private static void InitializeRPC()
        {
            rpc = new DiscordClient();
            rpc.ChangeStatus(s => s.Assets = new Assets { LargeImageKey = "gd" });

            scheduler = new Scheduler(5000);

            rpc.OnReady += () =>
            {
                scheduler.Add(CheckScene);
                scheduler.Add(GetLevelInformation);
                scheduler.Add(rpc.Update);

                scheduler.Pulse();
            };
        }

        public static void CheckScene()
        {
            var address = new IntPtr(0x3222D0);
            address = memory[address].Read<IntPtr>();

            var sceneInt = memory[address + 0x1DC, false].Read<int>();
            Enum.TryParse(sceneInt.ToString(), out Scene scene);

            currentScene = scene;
        }

        public static void GetLevelInformation()
        {
            if (currentScene != Scene.Play)
            {
                rpc.ChangeStatus(s => s.Timestamps = null);
                return;
            }

            /*if (IsInEditor)
            {
                var address = new IntPtr(0x3222D0);
                address = memory[address].Read<IntPtr>();

                address = memory[address + 0x168, false].Read<IntPtr>();
                var obj = memory[address + 0x3A0, false].Read<int>();

                Console.WriteLine(obj);
                
                return;
            }*/

            try
            {
                var address = new IntPtr(0x3222D0);
                address = memory[address].Read<IntPtr>();

                address = memory[address + 0x164, false].Read<IntPtr>();

                var levelLength = memory[address + 0x3B4, false].Read<float>();
                
                address = memory[address + 0x22C, false].Read<IntPtr>();
                address = memory[address + 0x114, false].Read<IntPtr>();

                var length = memory[address + 0x10C, false].Read<int>();

                if (length > 15)
                {
                    var titleAddress = memory[address + 0xFC, false].Read<IntPtr>();
                    levelInfo.Title = memory[titleAddress, false].ReadString(Encoding.Default);
                }
                else
                    levelInfo.Title = memory[address + 0xFC, false].ReadString(Encoding.Default);
                
                
                levelInfo.Id = memory[address + 0xF8, false].Read<int>();
                levelInfo.Author = memory[address + 0x144, false].ReadString(Encoding.Default);
                levelInfo.Stars = memory[address + 0x2AC, false].Read<int>();
                levelInfo.Demon = memory[address + 0x29C, false].Read<bool>();
                levelInfo.Auto = memory[address + 0x2B0, false].Read<bool>();
                levelInfo.Difficulty = memory[address + 0x1E4, false].Read<int>();
                levelInfo.DemonDifficulty = memory[address + 0x2A0, false].Read<int>();
                levelInfo.TotalAttempts = memory[address + 0x218, false].Read<int>();
                levelInfo.Jumps = memory[address + 0x224, false].Read<int>();
                levelInfo.CompletionProgress = memory[address + 0x248, false].Read<int>();
                levelInfo.PracticeCompletionProgress = memory[address + 0x26C, false].Read<int>();
                levelInfo.MaxCoins = memory[address + 0x2B4, false].Read<int>();

                levelInfo.CoinsGrabbed[0] = memory[address + 0x2E8, false].Read<bool>();
                levelInfo.CoinsGrabbed[1] = memory[address + 0x2F4, false].Read<bool>();
                levelInfo.CoinsGrabbed[2] = memory[address + 0x300, false].Read<bool>();

                levelInfo.Length = levelLength;

                Console.WriteLine(memory[address + 0x2B4, false]);
                
                var typeInt = memory[address + 0x364, false].Read<int>();
                Enum.TryParse(typeInt.ToString(), out LevelType type);

                levelInfo.Type = type;

                string GetCoinString()
                {
                    var @string = string.Empty;

                    for (var i = 0; i < levelInfo.MaxCoins; i++)
                        @string += levelInfo.CoinsGrabbed[i] ? "C" : "-";

                    if (!string.IsNullOrEmpty(@string))
                        @string += " |";

                    return @string;
                }

                if (levelInfo.Id == 62028241)
                    levelInfo.Stars = 12;

                rpc.ChangeStatus(s => s.Details = levelInfo.ToString());
                rpc.ChangeStatus(s => s.State = $"{levelInfo.CompletionProgress}% | {GetCoinString()} Att: {levelInfo.TotalAttempts:n0} | Jumps: {levelInfo.Jumps:n0} | Score: {levelInfo.CalculateScore():n0} ({levelInfo.CalculatePerformance():N} pp)");

                if (!rpc.presence.HasTimestamps())
                    rpc.ChangeStatus(s => s.WithTimestamps(Timestamps.Now));
            }
            catch (Exception e)
            {
                Write(e.Message);
                
                // we're most definitely not in the correct scene, so let's just go back to the main menu presence.
                rpc.ChangeStatus(s =>
                {
                    s.Details = string.Empty;
                    s.State = "In menus";
                    s.Timestamps = null;
                });
            }
        }

        public static bool IsInEditor
        {
            get
            {
                var pointer = new IntPtr(0x3222D0);
                var address = memory[pointer].Read<IntPtr>();
                
                return memory[address, false].Read<bool>();
            }
        }

        private static void GetGdProcess(IReadOnlyCollection<string> args)
        {
            if (args.Count > 0 && args.Any(a => a == "--opengd" || a == "-o"))
            {
                // TODO: Allow custom installation paths
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