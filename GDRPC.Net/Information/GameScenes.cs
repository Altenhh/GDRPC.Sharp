using System;

namespace GDRPC.Net.Information
{
    [Flags]
    public enum GameScenes
    {
        Unknown = 0,
        MainMenu = 1 << GameScene.MainMenu,
        Select = 1 << GameScene.Select,
        Play = 1 << GameScene.Play,
        Search = 1 << GameScene.Search,
        Leaderboard = 1 << GameScene.Leaderboard,
        Online = 1 << GameScene.Online,
        OfficialLevelListing = 1 << GameScene.OfficialLevelListing,
        OfficialLevel = 1 << GameScene.OfficialLevel,
        TheChallenge = 1 << GameScene.TheChallenge,
    }
}