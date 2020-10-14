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
        public override string ToString() => $"{Title} - {Author} [{CalculateDifficulty()}*]";

        public int CalculateScore() =>
            (int) (Stars * CompletionProgress * Math.Pow(Length, 0.75f));

        public double CalculatePerformance() =>
            Stars * (CompletionProgress * 0.01) * Math.Pow(Length, Stars / 40d);

        public int CalculateDifficulty()
        {
            if (Auto)
                return 0;

            if (!Demon) return Stars;

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