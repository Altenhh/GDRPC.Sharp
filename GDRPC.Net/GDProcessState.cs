namespace GDRPC.Net
{
    public class GdProcessState
    {
        public Scene Scene { get; set; }
        public LevelInfo LevelInfo { get; } = new LevelInfo();
    }
}