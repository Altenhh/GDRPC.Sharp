using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Binarysharp.MemoryManagement;
using GDRPC.Net.Information;

namespace GDRPC.Net.Memory
{
    public class GdReader
    {
        public const int BaseAddress = 0x3222D0;
        private static readonly AddressDictionary addresses;
        public readonly Process Process;
        private readonly GdProcessState currentState;
        private readonly MemorySharp memory;
        private readonly IntPtr processBaseAddress;

        static GdReader()
        {
            addresses = AddressDictionary.Parse(File.ReadAllText("Addresses.txt"));
        }

        public GdReader(Process process, GdProcessState state)
        {
            memory = new MemorySharp(Process = process);
            currentState = state;
            processBaseAddress = memory[(IntPtr) BaseAddress].Read<IntPtr>();
        }

        public bool IsInEditor => Read<bool>(0x0);

        public void UpdateScene()
        {
            // It would be great if you could directly read the value and convert it to an enum through the function; pending test
            var sceneInt = Read<int>("Current Scene");
            currentState.Scene = (Scene) sceneInt;
        }

        /// <summary>Updates the current GD process state in the locally stored <seealso cref="GdProcessState" /> object. It also calls the <seealso cref="UpdateScene" /> function.</summary>
        /// <returns><see langword="true" /> if the update did not encounter any errors; otherwise <see langword="false" />.</returns>
        public bool UpdateCurrentState(out Exception exception)
        {
            exception = null;

            UpdateScene();

            if (currentState.Scene != Scene.Play)
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
                currentState.LevelInfo.Title = memory[titleAddress, false].ReadString(Encoding.Default);
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
        }

        private T Read<T>(string addressEntryName) => Read<T>(addresses[addressEntryName]);

        private T Read<T>(AddressEntry entry)
        {
            // TODO: Utilize the type of the entry
            return Read<T>(entry.Offsets);
        }

        private T Read<T>(params int[] offsets)
        {
            var address = ForwardAddress(processBaseAddress, offsets);

            return memory[address + offsets[offsets.Length - 1], false].Read<T>();
        }

        private string ReadString(string addressEntryName) => ReadString(addresses[addressEntryName]);

        private string ReadString(AddressEntry entry)
        {
            var address = ForwardAddress(processBaseAddress, entry.Offsets);

            return memory[address + entry.Offsets[entry.Offsets.Length - 1], false].ReadString(Encoding.Default);
        }

        private IntPtr ForwardAddress(IntPtr address, int[] offsets)
        {
            for (var i = 0; i < offsets.Length - 1; i++)
                address = memory[address + offsets[i], false].Read<IntPtr>();

            return address;
        }
    }
}