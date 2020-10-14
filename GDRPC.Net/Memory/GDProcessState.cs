using GDRPC.Net.Information;

namespace GDRPC.Net.Memory
{
    public class GdProcessState
    {
        public Scene Scene { get; set; }
        public LevelInfo LevelInfo { get; } = new LevelInfo();
    }
}