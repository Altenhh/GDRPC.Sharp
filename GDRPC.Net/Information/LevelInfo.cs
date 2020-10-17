using System;

namespace GDRPC.Net.Information
{
    public class LevelInfo
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
            return $" [{difficulty}*]";
        }

        private string GetIdString()
        {
            if (Id == 0)
                return " (Local level)";
            return $" (ID: {Id})";
        }

        // Scorev2 pog
        public int CalculateScore() =>
            (int) (Math.Pow(Math.Pow(CompletionProgress / 100d, 1 + (CalculateDifficulty() / 14d) * 0.5d), 1 - ((Math.Log10(Length) - 5) * 0.1)) * 1_000_000);

        public double CalculatePerformance() =>
            Math.Pow(Math.Pow(CalculateDifficulty() / 14d, 0.4d) * (CalculateScore() / 1_000_000d) * Math.Pow(Length, CalculateDifficulty() / 21d), 1.2d);

        public int CalculateDifficulty()
        {
            if (Auto)
                return 0;

            if (!Demon)
                return Stars;

            switch (DemonDifficulty)
            {
                case 3:
                    return 10;

                case 4:
                    return 11;

                case 5:
                    return 13;

                case 6:
                    return 14;

                default:
                    return 12;
            }
        }
    }
}