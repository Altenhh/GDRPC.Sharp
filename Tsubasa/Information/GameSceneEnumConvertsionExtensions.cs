namespace Tsubasa.Information
{
    public static class GameSceneEnumConvertsionExtensions
    {
        public static GameScenes ToFlags(this GameScene scene) => (GameScenes)(1ul << (int)scene);
    }
}
