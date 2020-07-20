using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Com.O2Bionics.PageTracker.Tests.Utilities
{
    public static class ConcurrentBagExtensions
    {
        public static void Clear<T>([NotNull] this ConcurrentBag<T> bag)
        {
            while (!bag.IsEmpty)
                bag.TryTake(out _);
        }
    }
}