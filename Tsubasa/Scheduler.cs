using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Tsubasa.Helper;

namespace Tsubasa
{
    public class Scheduler
    {
        private readonly Queue<Action> runQueue = new Queue<Action>();
        public int Delay;
        public string name;
        public Stopwatch Stopwatch = new Stopwatch();

        public Scheduler(int delay, string name)
        {
            Delay = delay;
            this.name = name;
        }

        public void Pulse()
        {
            if (!Stopwatch.IsRunning)
                Stopwatch.Start();

            Stopwatch.Restart();
            Write($"[{name}] Executing {runQueue.Count} tasks.", ConsoleColor.Yellow);

            foreach (var action in runQueue)
                action();
        }

        public void Add(Action task)
        {
            runQueue.Enqueue(task);
        }
    }
}