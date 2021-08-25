namespace Tsubasa.Information
{
    public partial class LevelInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int Stars { get; set; }
        public bool Demon { get; set; }
        public bool Auto { get; set; }
        public int Difficulty { get; set; }
        public int DemonDifficulty { get; set; }
        public int TotalAttempts { get; set; }
        public int Jumps { get; set; }
        public int CompletionProgress { get; set; }
        public int PracticeCompletionProgress { get; set; }
        public int MaxCoins { get; set; }
        public float Length { get; set; }
        public bool[] CoinsGrabbed { get; set; } = new bool[3];
        public LevelType Type { get; set; }
        public override string ToString() => $"{Title} - {Author}{GetDifficultyString()}{GetIdString()}";

        private string GetDifficultyString()
        {
            var difficulty = CalculateDifficulty();

            if (difficulty == 0)
                return string.Empty;

            return $" [{difficulty:N1}*]";
        }

        private string GetIdString()
        {
            if (Id == 0)
                return " (Local level)";

            return $" (ID: {Id})";
        }
    }
}