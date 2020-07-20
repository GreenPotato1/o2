using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils
{
    public static class ListExtensions
    {
        /// <summary>
        /// Swap the elements at <paramref name="position"/> and the last item,
        /// then remove the last item.
        /// The <paramref name="position"/> must not exceed the <paramref name="list"/> size.
        /// </summary>
        public static void SwapLastAndRemove<T>([NotNull] this IList<T> list, int position)
        {
            Debug.Assert(0 < list.Count);
            var lastIndex = list.Count - 1;
            list[position] = list[lastIndex];
            list.RemoveAt(lastIndex);
        }
    }
}