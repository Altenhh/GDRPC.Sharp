namespace Tsubasa.Information
{
    public enum GameScene
    {
        MainMenu = 0,
        Select = 1,
        Play = 3,
        Search = 4,
        Leaderboard = 6,
        Online = 7,
        OfficialLevelListing = 8,
        OfficialLevel = 9,
        TheChallenge = 12,

        // FlagsEnumDictionary does not like negative values
        // so we use the highest possible value that wouldn't overflow GameScenes when converted
        Unknown = 63
    }
}