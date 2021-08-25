using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscordRPC;
using Tsubasa.Memory;
using Tsubasa.Online;
using Tsubasa.Online.Tcp;
using Tsubasa.Scenes;
using static Tsubasa.Helper;

namespace Tsubasa
{
    public static class Program
    {
        public const string CONFIG_VERSION = "1.0";
        private static Process gdProcess;
        private static GdReader reader;
        private static readonly GdProcessState state = new();
        private static DiscordClient rpc;
        private static Scheduler rpcScheduler;
        private static Scheduler serverScheduler;
        private static TcpManager client;
        private static readonly MemoryStorage blacklist_ids = new();
        private static readonly Dictionary<int, List<float>> last_died = new();
        private static bool successfulUpdate;
        private static bool authenticated;

        public static void Main(string[] args)
        {
            GetGdProcess(args);

            if (gdProcess == null)
            {
                Write("[hooker] Failed to hook onto process");

                return;
            }

            // Parallel Threads
            Parallel.Invoke(
                // RPC Thread
                () =>
                {
                    Hook();
                    InitializeRpc();

                    CreateLoop(rpcScheduler);
                },
                // TCP Thread
                () =>
                {
                   InitializeServer();
                   new Thread(() => CreateLoop(serverScheduler)).Start();
                }
            );
        }

        private static void CreateLoop(Scheduler scheduler)
        {
            while (true)
            {
                if (scheduler.Stopwatch.ElapsedMilliseconds < scheduler.Delay)
                    continue;

                scheduler.Stopwatch.Restart();
                scheduler.Pulse();
            }
        }

        #region Server
        private static void InitializeServer()
        {
            InitializeLocalDatabase();
            ConnectToTcpServer();
        }

        private static void InitializeLocalDatabase()
        {
        }

        private static void ConnectToTcpServer()
        {
            try
            {
                Write("[tcp] Connecting to Tcp server...");

                client = new TcpManager("159.203.21.142", 6967);
                client.Connect();

                serverScheduler = new Scheduler(5000, "tcp");
                serverScheduler.Add(SendHeartBeat);

                // kickstart reading
                client.PacketRecieved += (_, e) => Handler.Construct(e.Packet);
                
                LoginToServer();
                serverScheduler.Pulse();

                // subscribe to stream
                new Thread(() => client.Subscribe()).Start();
            }
            catch (Exception e)
            {
                Write("[tcp]" + e);
                Write("[tcp] Continuing without server...");

                throw;
            }
        }

        private static void LoginToServer()
        {
            client.StartPacket(PacketIds.Login);
            //TODO: Add login information here.
            client.EndPacket();
        }

        private static void SendHeartBeat()
        {
            client.StartPacket(PacketIds.Ping);
            client.EndPacket();
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
                Write($"[hooker] New record! {lastPercent}% -> {percent}%");

                //client.StartPacket(PacketIds.SendScore);
                //client.WritePacket(state.LevelInfo.Id);
                //client.WritePacket(state.PlayerState.UserId);
                //client.WritePacket(state.PlayerState.AccountId);
                //client.WritePacket(state.LevelInfo.CalculateScore());
                //client.WritePacket(state.LevelInfo.CalculatePerformance());
                //client.WritePacket((state.PlayerState.X / state.LevelInfo.Length) * 100); // More accurate percentage.
                //client.EndPacket();
            }
        }
        #endregion

        private static void Hook()
        {
            reader = new GdReader(gdProcess, state);
            Write($"[hooker] Hooked onto process: {gdProcess.MainWindowTitle} ({gdProcess.Id})");
        }

        private static void InitializeRpc()
        {
            rpc = new DiscordClient();
            rpc.ChangeStatus(s => s.Assets = new Assets { LargeImageKey = "gd" });

            rpcScheduler = new Scheduler(2000, "rpc");

            rpc.OnReady += () =>
            {
                rpcScheduler.Add(UpdateCurrentState);
                rpcScheduler.Add(UpdateRpcDisplay);
                rpcScheduler.Add(rpc.Update);

                // We want to constantly check the level progress in-line with the RPC, since the RPC has more up-to-date information than the servers.
                rpcScheduler.Add(ServerCheckLevelProgress);

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
            currentRpcScene.Dispose();

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

                Write($"[hooker] Switched scene to: {currentRpcScene.State.Scene}");
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