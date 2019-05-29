using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bitbird.Core
{
    public static class EnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
                action(item);
        }
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
        {
            int i = 0;
            foreach (var item in enumerable)
            {
                action(item, i);
                i++;
            }
        }
        public static async Task ForEachAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> action)
        {
            foreach (var item in enumerable)
                await action(item);
        }
        public static async Task ForEachAsync<T>(this IEnumerable<T> enumerable, Func<T, int, Task> action)
        {
            int i = 0;
            foreach (var item in enumerable)
            {
                await action(item, i);
                i++;
            }
        }
    }
}
