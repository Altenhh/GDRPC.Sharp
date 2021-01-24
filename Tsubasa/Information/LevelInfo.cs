using System;

namespace Tsubasa.Information
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
        public int CalculateScore()
        {
            var res = (Math.Pow(Math.Pow(CompletionProgress / 100d, 1 + (CalculateDifficulty() / 14d) * 0.5d),
                                 1 - ((Math.Log10(Length) - 5) * 0.1)) * 1_000_000);
            
            // jump bonus
            res += Math.Pow(Jumps, 1.1f);

            return (int)res;
        }

        public double CalculatePerformance()
        {
            Console.WriteLine("Difficulty: " + CalculateDifficulty());
            Console.WriteLine("Score: " + CalculateScore());
            Console.WriteLine("Length: " + Length);

            var res = Math.Pow(
                Math.Pow(CalculateDifficulty() / 14d, 0.4d) * (CalculateScore() / 1_000_000d) *
                Math.Pow(Length, CalculateDifficulty() / 25d), 1.2d);

            if (Demon)
                res += Math.Pow(DemonDifficulty * (CompletionProgress / 100), 1.15f);

            double attemptPenalty;

            if (Demon && TotalAttempts < (500 * DemonDifficulty))
                attemptPenalty = Math.Pow(TotalAttempts, -((TotalAttempts * 0.75) / 1e7)); // demon, low attempts
            else
                attemptPenalty = Math.Pow(TotalAttempts, -((TotalAttempts * 0.85) / 1e7)) * 0.7; // everything else

            Console.WriteLine("res: " + res);
            Console.WriteLine("low: " + Math.Pow(TotalAttempts, -((TotalAttempts * 0.75) / 1e7)));
            Console.WriteLine("high: " + Math.Pow(TotalAttempts, -((TotalAttempts * 0.85) / 1e7)) * 0.7);

            res *= attemptPenalty;

            Console.WriteLine("total: " + res);

            if (CompletionProgress >= 100)
                res *= 1.05f;

            foreach (var coin in CoinsGrabbed)
                if (coin)
                    res *= (.1 / MaxCoins) + 1; // should at least have 1.1 bonus max.

            return res;
        }

        public int CalculateDifficulty()
        {
            if (Auto)
                return 0;

            if (!Demon)
                return Stars;

            return DemonDifficulty switch
            {
                3 => 10,
                4 => 11,
                5 => 13,
                6 => 14,
                _ => 12,
            };
        }
    }
}