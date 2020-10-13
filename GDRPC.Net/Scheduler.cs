using System;
using System.Collections.Generic;
using System.Diagnostics;
using static GDRPC.Net.Helper;

namespace GDRPC.Net
{
    public class Scheduler
    {
        private readonly Queue<Action> runQueue = new Queue<Action>();
        public int delay;
        public Stopwatch stopwatch = new Stopwatch();

        public Scheduler(int delay)
        {
            this.delay = delay;
        }

        public void Pulse()
        {
            if (!stopwatch.IsRunning)
                stopwatch.Start();

            stopwatch.Restart();
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