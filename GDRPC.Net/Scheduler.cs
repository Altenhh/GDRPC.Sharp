using System;
using System.Collections.Generic;
using System.Diagnostics;
using static GDRPC.Net.Helper;

namespace GDRPC.Net
{
    public class Scheduler
    {
        private readonly Queue<Action> runQueue = new Queue<Action>();

        public int Delay;
        public Stopwatch Stopwatch = new Stopwatch();

        public Scheduler(int delay)
        {
            Delay = delay;
        }

        public void Pulse()
        {
            if (!Stopwatch.IsRunning)
                Stopwatch.Start();

            Stopwatch.Restart();
            Write($"Executing {runQueue.Count} tasks.", ConsoleColor.Yellow);

            foreach (var action in runQueue)
                action();
        }

        public void Add(Action task)
        {
            runQueue.Enqueue(task);
        }
    }
}