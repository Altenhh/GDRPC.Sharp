using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Tsubasa.Information;
using ProcessMemory = Binarysharp.MemoryManagement.MemorySharp;

namespace Tsubasa.Memory
{
    public class GdReader
    {
        public const int GAME_MANAGER_ADDRESS = 0x3222D0;
        public const int ACCOUNT_MANAGER_ADDRESS = 0x3222D8;
        public GdProcessState PreviousState;
        private static readonly AddressDictionary addresses;
        public readonly Process Process;
        private readonly GdProcessState currentState;
        private readonly ProcessMemory memory;
        private readonly IntPtr gameManager;
        private readonly IntPtr accountManager;

        static GdReader()
        {
            addresses = AddressDictionary.Parse(File.ReadAllText("Addresses.txt"));
        }

        public GdReader(Process process, GdProcessState state)
        {
            memory = new ProcessMemory(Process = process);
            currentState = state;
            gameManager = memory.Read<IntPtr>((IntPtr) GAME_MANAGER_ADDRESS, true);
            accountManager = memory.Read<IntPtr>((IntPtr) ACCOUNT_MANAGER_ADDRESS, true);
        }

        public bool IsInEditor => Read<bool>(gameManager, 0x0);

        public void UpdateScene()
        {
            // Cannot directly read enum types
            currentState.Scene = (GameScene) Read<int>("Current Scene");
            
            currentState.PlayerState.UserId = Read<int>("User ID");
            
            // This one is special, we have to read from the account manager instead.
            currentState.PlayerState.AccountId = Read<int>("Account ID", accountManager);
        }

        /// <summary>Updates the current GD process state in the locally stored <seealso cref="GdProcessState" /> object. It also calls the <seealso cref="UpdateScene" /> function.</summary>
        /// <returns><see langword="true" /> if the update did not encounter any errors; otherwise <see langword="false" />.</returns>
        public bool UpdateCurrentState(out Exception exception)
        {
            exception = null;

            PreviousState = currentState;
            UpdateScene();

            if (currentState.Scene != GameScene.Play)
                return true;

            try
            {
                LoadLevelInfo();
            }
            catch (Exception e)
            {
                exception = e;

                return false;
            }

            return true;
        }

        private void LoadLevelInfo()
        {
            var levelTitleLength = Read<int>("Level Title Length");

            if (levelTitleLength > 15)
            {
                var titleAddress = Read<IntPtr>("Level Title");
                currentState.LevelInfo.Title = memory.ReadString(titleAddress, Encoding.Default);
            }
            else
            {
                currentState.LevelInfo.Title = ReadString("Level Title");
            }

            currentState.LevelInfo.Id = Read<int>("Level ID");
            currentState.LevelInfo.Author = ReadString("Level Author");
            currentState.LevelInfo.Stars = Read<int>("Level Stars");
            currentState.LevelInfo.Demon = Read<bool>("Is Demon");
            currentState.LevelInfo.Auto = Read<bool>("Is Auto");
            currentState.LevelInfo.Difficulty = Read<int>("Level Difficulty");
            currentState.LevelInfo.DemonDifficulty = Read<int>("Demon Difficulty");
            currentState.LevelInfo.TotalAttempts = Read<int>("Attempts");
            currentState.LevelInfo.Jumps = Read<int>("Jumps");
            currentState.LevelInfo.CompletionProgress = Read<int>("Completion Progress");
            currentState.LevelInfo.PracticeCompletionProgress = Read<int>("Practice Completion Progress");
            currentState.LevelInfo.MaxCoins = Read<int>("Max Coins");

            for (var i = 0; i < 3; i++)
                currentState.LevelInfo.CoinsGrabbed[i] = Read<bool>($"Coin {i} Grabbed");

            currentState.LevelInfo.Length = Read<float>("Level Length");

            for (var i = 0; i < 3; i++)
                currentState.LevelInfo.CoinsGrabbed[i] = Read<bool>($"Coin {i} Grabbed");

            currentState.LevelInfo.Length = Read<float>("Level Length");

            currentState.LevelInfo.Type = (LevelType) Read<int>("Level Type");

            currentState.PlayerState.X = Read<float>("Player X");
            currentState.PlayerState.IsDead = Read<bool>("Player Dead");
            currentState.PlayerState.IsPractice = Read<bool>("Practice Mode");
        }

        private T Read<T>(string addressEntryName)
            where T : struct
        {
            return Read<T>(addresses[addressEntryName]);
        }
        
        private T Read<T>(string addressEntryName, IntPtr baseAddress)
            where T : struct
        {
            return Read<T>(addresses[addressEntryName], baseAddress);
        }

        private T Read<T>(AddressEntry entry)
            where T : struct
        {
            // TODO: Utilize the type of the entry
            return Read<T>(gameManager, entry.Offsets);
        }
        
        private T Read<T>(AddressEntry entry, IntPtr baseAddress)
            where T : struct
        {
            // TODO: Utilize the type of the entry
            return Read<T>(baseAddress, entry.Offsets);
        }

        private T Read<T>(IntPtr baseAddress, params int[] offsets)
            where T : struct
        {
            var address = ForwardAddress(baseAddress, offsets);

            return memory.Read<T>(address + offsets[^1], false);
        }

        private string ReadString(string addressEntryName) => ReadString(gameManager, addresses[addressEntryName]);

        private string ReadString(IntPtr baseAddress, AddressEntry entry)
        {
            var address = ForwardAddress(baseAddress, entry.Offsets);

            return memory.ReadString(address + entry.Offsets[^1], Encoding.Default, false);
        }

        private IntPtr ForwardAddress(IntPtr address, int[] offsets)
        {
            for (var i = 0; i < offsets.Length - 1; i++)
                address = memory.Read<IntPtr>(address + offsets[i], false);

            return address;
        }
    }
}