using System;
using System.Collections.Generic;
using System.Linq;
using Program.Promotion;

namespace Program
{
    public static class Extensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static HashSet<T> Clone<T>(this HashSet<T> setToClone) where T : ICloneable
        {
            return setToClone.Select(item => (T)item.Clone()).ToHashSet();
        }

        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> dicToClone)
            where TValue : class, ICloneable
        {
            var result = new Dictionary<TKey, TValue>();
            foreach (var item in dicToClone)
            {
                result.Add(item.Key, item.Value.Clone() as TValue);
            }
            return result;
        }
    }
}
