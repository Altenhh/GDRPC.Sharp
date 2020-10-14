using GDRPC.Net.Information;

namespace GDRPC.Net.Memory
{
    public class GdProcessState
    {
        public GameScene Scene { get; set; }
        public LevelInfo LevelInfo { get; } = new LevelInfo();
    }
}