using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private static readonly GdProcessState state = new();
        private static DiscordClient rpc;
        private static Scheduler rpcScheduler;
        private static TcpManager client;
        private static readonly MemoryStorage blacklist_ids = new();
        private static readonly Dictionary<int, List<float>> last_died = new();
        private static bool successfulUpdate;

        public static void Main(string[] args)
        {
            GetGdProcess(args);

            if (gdProcess == null)
            {
                Write("Failed to hook onto process", ConsoleColor.Red);

                return;
            }

            Hook();
            InitializeRpc();
            new Task(InitializeServer).Start();

            while (true)
            {
                if (rpcScheduler.Stopwatch.ElapsedMilliseconds < rpcScheduler.Delay)
                    continue;

                rpcScheduler.Stopwatch.Restart();
                rpcScheduler.Pulse();
            }
        }

        #region Server
        private static void InitializeServer()
        {
            ConnectToTcpServer();
        }

        private static void ConnectToTcpServer()
        {
            try
            {
                Write("Connecting to Tcp server...");

                client = new TcpManager("207.244.229.86", 6967);
                client.Connect();
            }
            catch (Exception e)
            {
                Write(e.ToString(), ConsoleColor.Red);
                Write("Continuing without server...", ConsoleColor.Red);

                throw;
            }
        }

        private static void SendHeartBeat()
        {
            client.StartPacket(PacketIds.Ping);
            client.EndPacket();
            
            var res = client.ReadNext();

            if (res.Id == (short) PacketIds.Pong)
            {
                Write("Heartbeat successfully heard.");
            }
        }

        private static void ServerCheckLevelProgress()
        {
            if (currentRpcScene.GetType() != typeof(PlayScene))
                return;

            // If the player is in practice, then just ignore the attempts.
            if (state.PlayerState.IsPractice)
                return;

            // We've already completed the level before
            if (blacklist_ids.Any(id => id == state.LevelInfo.Id))
                return;

            // We've already completed the level, so let's add it to our blacklist.
            // TODO: Remove this code for the first few months of release, then later on release a "post score" method on the website.
            if (state.LevelInfo.CompletionProgress == 100 && state.PlayerState.X < state.LevelInfo.Length)
                blacklist_ids.Add(state.LevelInfo.Id);

            // Let's check if the player is dead before continuing on.
            if (!state.PlayerState.IsDead)
                return;

            if (!last_died.ContainsKey(state.LevelInfo.Id))
            {
                var points = new List<float> { 0 };
                last_died.Add(state.LevelInfo.Id, points);
            }

            var percent = Math.Round(state.PlayerState.X / state.LevelInfo.Length * 100, MidpointRounding.ToZero);

            var lastPercent = Math.Round(last_died[state.LevelInfo.Id].Max() / state.LevelInfo.Length * 100,
                MidpointRounding.ToZero);

            // new record
            if (percent > lastPercent)
            {
                last_died[state.LevelInfo.Id].Add(state.PlayerState.X);
                Write($"New record! {lastPercent}% -> {percent}%", ConsoleColor.Green);

                client.StartPacket(PacketIds.SendScore);
                client.WritePacket(state.LevelInfo.Id);
                client.WritePacket(state.LevelInfo.CalculateScore());
                client.WritePacket(state.LevelInfo.CalculatePerformance());
                client.EndPacket();
            }
        }
        #endregion

        private static void Hook()
        {
            reader = new GdReader(gdProcess, state);
            Write($"Hooked onto process: {gdProcess.MainWindowTitle} ({gdProcess.Id})");
        }

        private static void InitializeRpc()
        {
            rpc = new DiscordClient();
            rpc.ChangeStatus(s => s.Assets = new Assets { LargeImageKey = "gd" });

            rpcScheduler = new Scheduler(2000);

            rpc.OnReady += () =>
            {
                rpcScheduler.Add(UpdateCurrentState);
                rpcScheduler.Add(UpdateRpcDisplay);
                rpcScheduler.Add(rpc.Update);

                // Even though this really doesn't belong here, it should stay here as to not overload the server with requests.
                rpcScheduler.Add(ServerCheckLevelProgress);
                rpcScheduler.Add(SendHeartBeat);

                rpcScheduler.Pulse();
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