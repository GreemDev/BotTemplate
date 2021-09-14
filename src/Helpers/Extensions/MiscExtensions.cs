using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Gommon
{
    public partial class Extensions
    {
        public static List<T> AsSingletonList<T>(this T @this) => new() { @this };

        public static T[] Concat<T>(this T[] current, T[] toConcat) => Enumerable.Concat(current, toConcat).ToArray();

        public static T ValueLock<T>(this object @lock, Func<T> action)
        {
            lock (@lock)
                return action();
        }

        public static void Lock(this object @lock, Action action)
        {
            lock (@lock)
                action();
        }

        public static void LockedRef<T>(this T obj, Action<T> action)
        {
            lock (obj)
                action(obj);
        }
    }
}