namespace GDRPC.Net
{
    public enum Scene
    {
        Unknown = -1,
        MainMenu = 0,
        Select = 1,
        Play = 3,
        Search = 4,
        UnknownProperty1 = 5,
        Leaderboard = 6,
        Online = 7,
        OfficialLevelListing = 8,
        OfficialLevel = 9,
        TheChallenge = 12
    }

    public enum LevelType
    {
        Null = 0,
        Official = 1,
        Editor = 2,
        Saved = 3,
        Online = 4
    }
}