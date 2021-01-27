using Tsubasa.Information;

namespace Tsubasa.Memory
{
    public class GdProcessState
    {
        public GameScene Scene { get; set; }
        public LevelInfo LevelInfo { get; } = new LevelInfo();
        public PlayerState PlayerState { get; set; } = new();
    }
}