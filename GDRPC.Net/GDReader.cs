using Binarysharp.MemoryManagement;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GDRPC.Net
{
    public class GdReader
    {
        private static AddressDictionary addresses;
        
        static GdReader()
        {
            addresses = AddressDictionary.Parse(File.ReadAllText("Addresses.txt"));
        }

        public const int BaseAddress = 0x3222D0;

        private MemorySharp memory;
        private GdProcessState currentState;
        private IntPtr processBaseAddress;

        public readonly Process Process;

        public GdReader(Process process, GdProcessState state)
        {
            memory = new MemorySharp(Process = process);
            currentState = state;
            processBaseAddress = memory[(IntPtr)BaseAddress].Read<IntPtr>();
        }

        // .. Reading

        public void UpdateScene()
        {
            // It would be great if you could directly read the value and convert it to an enum through the function; pending test
            var sceneInt = Read<int>("Current Scene");
            currentState.Scene = (Scene)sceneInt;
        }
        /// <summary>Updates the current GD process state in the locally stored <seealso cref="GdProcessState"/> object. It also calls the <seealso cref="UpdateScene"/> function.</summary>
        /// <returns><see langword="true"/> if the update did not encounter any errors; otherwise <see langword="false"/>.</returns>
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
            /*if (IsInEditor)
            {
                var address = new IntPtr(0x3222D0);
                address = memory[address].Read<IntPtr>();

                address = memory[address + 0x168, false].Read<IntPtr>();
                var obj = memory[address + 0x3A0, false].Read<int>();

                Console.WriteLine(obj);
                
                return;
            }*/

            var levelTitleLength = Read<int>("Level Title Length");

            if (levelTitleLength > 15)
            {
                var titleAddress = Read<IntPtr>("Level Title");
                currentState.LevelInfo.Title = memory[titleAddress, false].ReadString(Encoding.Default);
            }
            else
                currentState.LevelInfo.Title = ReadString("Level Title");

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

            for (int i = 0; i < 3; i++)
                currentState.LevelInfo.CoinsGrabbed[i] = Read<bool>($"Coin {i} Grabbed");

            currentState.LevelInfo.Length = Read<float>("Level Length");
            for (int i = 0; i < 3; i++)
                currentState.LevelInfo.CoinsGrabbed[i] = Read<bool>($"Coin {i} Grabbed");

            currentState.LevelInfo.Length = Read<float>("Level Length");
            
            // Debugging again?
            Console.WriteLine(Read<int>("Max Coins"));

            currentState.LevelInfo.Type = (LevelType)Read<int>("Level Type");
        }

        private T Read<T>(string addressEntryName)
        {
            return Read<T>(addresses[addressEntryName]);
        }
        private T Read<T>(AddressEntry entry)
        {
            // TODO: Utilize the type of the entry
            var address = ForwardAddress(processBaseAddress, entry.Offsets);
            return memory[address + entry.Offsets[entry.Offsets.Length - 1], false].Read<T>();
        }
        private string ReadString(string addressEntryName)
        {
            return ReadString(addresses[addressEntryName]);
        }
        private string ReadString(AddressEntry entry)
        {
            var address = ForwardAddress(processBaseAddress, entry.Offsets);
            return memory[address + entry.Offsets[entry.Offsets.Length - 1], false].ReadString(Encoding.Default);
        }

        private IntPtr ForwardAddress(IntPtr address, int[] offsets)
        {
            for (int i = 0; i < offsets.Length - 1; i++)
                address = memory[address + offsets[i], false].Read<IntPtr>();

            return address;
        }
    }
}
