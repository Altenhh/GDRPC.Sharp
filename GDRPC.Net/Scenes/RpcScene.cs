using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GDRPC.Net.Information;
using GDRPC.Net.Memory;
using GDRPC.Net.Utilities;

namespace GDRPC.Net.Scenes
{
    public abstract class RpcScene
    {
        private static FlagsEnumDictionary<GameScenes, Type> sceneTypesDictionary = new FlagsEnumDictionary<GameScenes, Type>();
        private static Dictionary<Type, ConstructorInfo> sceneConstructors;

        static RpcScene()
        {
            var sceneTypes = typeof(RpcScene).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(RpcScene)) && !t.IsAbstract);

            sceneConstructors = sceneTypes.ToDictionary(t => t, t => t.GetConstructor(Type.EmptyTypes));
            foreach (var t in sceneTypes)
            {
                var scene = sceneConstructors[t].Invoke(Array.Empty<object>()) as RpcScene;
                sceneTypesDictionary.Add(scene.SceneFlags, t);
            }
        }

        public GdReader Reader { get; set; }
        public DiscordClient Client { get; set; }
        public GdProcessState State { get; set; }
        public abstract IEnumerable<GameScene> Scenes { get; }

        public GameScenes SceneFlags
        {
            get
            {
                var result = GameScenes.None;
                foreach (var s in Scenes)
                    result |= s.ToFlags();
                return result;
            }
        }

        protected RpcScene()
        {
        }

        protected RpcScene(GdReader reader, DiscordClient client, GdProcessState state)
        {
            Reader = reader;
            Client = client;
            State = state;
        }

        public abstract void Pulse();

        public static RpcScene GetScene(GameScene scene) => GetScene(scene.ToFlags());
        public static RpcScene GetScene(GameScenes scene) => sceneConstructors[sceneTypesDictionary[scene]].Invoke(Array.Empty<object>()) as RpcScene;
    }
}