using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DiscordRPC;
using GDRPC.Net.Memory;
using GDRPC.Net.Scenes;
using static GDRPC.Net.Helper;

namespace GDRPC.Net
{
    public static class Program
    {
        private static Process gdProcess;
        private static GdReader reader;
        private static readonly GdProcessState state = new GdProcessState();
        private static DiscordClient rpc;
        private static Scheduler scheduler;

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
            InitializeRPC();

            while (true)
            {
                if (scheduler.Stopwatch.ElapsedMilliseconds < scheduler.Delay)
                    continue;

                scheduler.Stopwatch.Restart();
                scheduler.Pulse();
            }
        }

        private static void Hook()
        {
            reader = new GdReader(gdProcess, state);
            Write($"Hooked onto process: {gdProcess.MainWindowTitle} ({gdProcess.Id})");
        }

        private static void InitializeRPC()
        {
            rpc = new DiscordClient();
            rpc.ChangeStatus(s => s.Assets = new Assets {LargeImageKey = "gd"});

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
            else if (reader.IsInEditor)
                currentRpcScene = new EditorScene(reader, rpc, state);
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