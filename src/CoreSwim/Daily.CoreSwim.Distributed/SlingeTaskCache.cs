using System.Collections.Concurrent;

namespace Daily.CoreSwim.Distributed
{
    internal static class SingeTaskCache
    {
        internal static readonly ConcurrentDictionary<string, long> TaskNumbers = new();

        internal static readonly ConcurrentDictionary<string, Lazy<DistributedTaskCoordinate>> TaskManager = new();
    }
}