using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DiscordRPC;
using Tsubasa.Memory;
using Tsubasa.Online;
using Tsubasa.Scenes;
using static Tsubasa.Helper;

namespace Tsubasa
{
    public static class Program
    {
        private static Process gdProcess;
        private static GdReader reader;
        private static readonly GdProcessState state = new GdProcessState();
        private static DiscordClient rpc;
        private static Scheduler scheduler;
        private static TcpClient client;
        private static bool successfulUpdate;

        public static async Task Main(string[] args)
        {
            GetGdProcess(args);

            if (gdProcess == null)
            {
                Write("Failed to hook onto process", ConsoleColor.Red);

                return;
            }

            Hook();
            InitializeRpc();
            await InitializeServer();

            while (true)
            {
                if (scheduler.Stopwatch.ElapsedMilliseconds < scheduler.Delay)
                    continue;

                scheduler.Stopwatch.Restart();
                scheduler.Pulse();
            }
        }

        private static async Task InitializeServer()
        {
            await ConnectToTcpServer();
            SendTestPacket();
        }

        private static async Task ConnectToTcpServer()
        {
            try
            {
                client = new TcpClient();
                Write("Connecting to Tcp server...");

                await client.ConnectAsync("207.244.229.86", 6967);

                Write("Connected!", ConsoleColor.Green);
            }
            catch (Exception e)
            {
                Write(e.Message, ConsoleColor.Red);
                Write("Continuing without server...", ConsoleColor.Red);
            }
        }

        private static void SendTestPacket()
        {
            if (client.Connected)
            {
                var packet = new Packet();
                var stream = client.GetStream();
                var rng = new Random();

                // set id
                packet.Id = 10;

                for (int i = 0; i < 5; i++)
                {
                    packet.Write<int>(rng.Next(int.MaxValue));
                }

                // pack thing
                var packedpacketedpacket = packet.Pack();

                stream.Write(packedpacketedpacket, 0, packedpacketedpacket.Length);
            }
        }

        private static void Hook()
        {
            reader = new GdReader(gdProcess, state);
            Write($"Hooked onto process: {gdProcess.MainWindowTitle} ({gdProcess.Id})");
        }

        private static void InitializeRpc()
        {
            rpc = new DiscordClient();
            rpc.ChangeStatus(s => s.Assets = new Assets { LargeImageKey = "gd" });

            scheduler = new Scheduler(2000);

            rpc.OnReady += () =>
            {
                scheduler.Add(UpdateCurrentState);
                scheduler.Add(UpdateRpcDisplay);
                scheduler.Add(rpc.Update);

                scheduler.Pulse();
            };
        }

        private static void UpdateCurrentState()
        {
            var currentState = reader.UpdateCurrentState(out var e);
            successfulUpdate = currentState;

            if (!successfulUpdate)
                Write(e.Message);
        }

        private static RpcScene currentRpcScene = new IdleScene();

        private static void UpdateRpcDisplay()
        {
            if (successfulUpdate)
                GetNewRpcScene();
            else
                // We're most definitely not in the correct scene, so let's just go back to the main menu presence.
                currentRpcScene = new IdleScene(reader, rpc, state);
            
            currentRpcScene.Pulse();

            static void GetNewRpcScene()
            {
                currentRpcScene = RpcScene.GetScene(state.Scene);
                currentRpcScene.State = state;
                currentRpcScene.Client = rpc;
                currentRpcScene.Reader = reader;
                
                Write($"Switched scene to: {currentRpcScene.State.Scene}", ConsoleColor.Blue);
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