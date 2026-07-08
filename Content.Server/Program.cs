using System;
using System.Threading;
using Robust.Server;

namespace Content.Server
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            // 🌇Sunset🌇 - the engine's IParallelManager (used for e.g. atmos delta-pressure and NPC
            // pathfinding, see Content.Server/Atmos and Content.Server/NPC/Pathfinding) already
            // fans work out across all cores via the shared .NET ThreadPool, whose default minimum
            // thread count is just Environment.ProcessorCount. That's fine for pure CPU-bound
            // parallel jobs alone, but this codebase also fires off a lot of blocking async work on
            // the SAME pool at the same time - DB queries (bans, playtime, notes), Discord/webhook
            // calls, etc. When both compete for a pool sized to exactly the core count, one side
            // waits on the .NET runtime's slow thread-injection ramp-up (~1 new thread/500ms)
            // instead of actually running in parallel. Doubling the minimum gives CPU-bound and
            // I/O-bound work enough headroom to run concurrently without that stall. This is a
            // runtime ThreadPool setting made from content startup, not an engine change -
            // RobustToolbox itself is untouched.
            var threads = Math.Max(Environment.ProcessorCount * 2, 16);
            ThreadPool.SetMinThreads(threads, threads);

            ContentStart.Start(args);
        }
    }
}
