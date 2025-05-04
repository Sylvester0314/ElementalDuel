using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;

namespace Shared.Misc
{
    public static class StaticExtends
    {
        public static void Shuffle<T>(this List<T> list, Xoshiro random = null)
        {
            var count = list.Count;
            random ??= Xoshiro.Create();

            for (var n = count - 1; n > 1; n--)
            {
                var k = random.NextInt(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        
        public static void Subtract<T>(this List<T> origin, List<T> value)
        {
            var list = new List<T>(origin);
            var grouped = value
                .GroupBy(x => x)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );

            origin.Clear();
            foreach (var item in list)
            {
                if (grouped.ContainsKey(item) && grouped[item] > 0)
                    grouped[item] -= 1;
                else
                    origin.Add(item);
            }
        }

        public static List<T> Attach<T>(this IEnumerable<T> origin, T value)
        {
            return new List<T>(origin) { value };
        }

        public static T Pop<T>(this List<T> list)
        {
            var value = list[^1];
            list.RemoveAt(list.Count - 1);
            return value;
        }

        public static TV RemoveAndGet<TK, TV>(this Dictionary<TK, TV> dict, TK key)
        {
            if (!dict.TryGetValue(key, out var value))
                return default;
            
            dict.Remove(key);
            return value;
        }

        public static List<T> SplitRange<T>(this List<T> list, int index, int count)
        {
            var result = list.GetRange(index, count);
            list.RemoveRange(index, count);
            return result;
        }

        public static List<T> ReversedList<T>(this List<T> list)
        {
            var reversed = new List<T>(list);
            reversed.Reverse();
            
            return reversed;
        }
        
        public static float InverseEvaluateCurve(this AnimationCurve curve, float progress)
        {
            var keyCount = curve.length;
            if (keyCount == 0)
                return 0;

            for (var i = 0; i < keyCount - 1; i++)
            {
                var x0 = curve[i].time;
                var y0 = curve[i].value;
                var x1 = curve[i + 1].time;
                var y1 = curve[i + 1].value;

                var c1 = progress >= y0 && progress <= y1;
                var c2 = progress <= y0 && progress >= y1;
                if (!c1 && !c2)
                    continue;

                var t = (progress - y0) / (y1 - y0);
                return Mathf.Lerp(x0, x1, t);
            }

            return -1f;
        }

        public static void SplitBy<T>(this List<T> origin, out List<T> inversed, Func<T, bool> predicate)
        {
            var list = new List<T>(origin);

            origin.Clear();
            inversed = new List<T>();

            foreach (var element in list)
            {
                if (predicate(element))
                    origin.Add(element);
                else
                    inversed.Add(element);
            }
        }

        public static (List<T> conforming, List<T> nonconforming) SplitBy<T>(
            this List<T> origin, Func<T, bool> predicate)
        {
            origin.SplitBy(out var inversed, predicate);
            return (origin, inversed);
        }

        public static string ToJson(this Dictionary<string, string> dict)
        {
            var list = dict
                .Select(pair => $"\"{pair.Key}\":\"{pair.Value}\"")
                .ToList();
            return $"{{{string.Join(',', list)}}}";
        }

        public static string ToLog<T>(this List<T> list)
        {
            var elements = list.Select(x => x.ToString()).ToList();
            return $"[{string.Join(" ,", elements)}]";
        }

        public static int FixedBinarySearch<T>(this List<T> list, T item)
        {
            var index = list.BinarySearch(item);
            if (index < 0)
                index = ~index;
            return index;
        }

        public static void Deconstruct(this Vector3 vector, out float x, out float y, out float z)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public static string ToCamelCase(this string str)
        {
            var first = char.ToLower(str[0]);
            return first + str[1..];
        }

        public static bool Contains<T>(this IEnumerator<T> enumerator, T value)
        {
            while (enumerator.MoveNext())
            {
                if (EqualityComparer<T>.Default.Equals(enumerator.Current, value))
                    return true;
            }

            return false;
        }

        public static List<T> SingleList<T>(this T value)
        {
            return new List<T> { value };
        }
        
        public static T[] SingleArray<T>(this T value)
        {
            return new [] { value };
        }

        public static List<List<T>> Paginate<T>(this List<T> list, int pageSize)
        {
            var pages = new List<List<T>>();
            var totalPages = (int)Math.Ceiling(list.Count / (double)pageSize);

            for (var i = 0; i < totalPages; i++)
            {
                var page = list.Skip(i * pageSize).Take(pageSize).ToList();
                pages.Add(page);
            }

            return pages;
        }

        public static bool TryCast<TResult>(this IEnumerable<object> list, out List<TResult> result)
        {
            result = list.OfType<TResult>().ToList();
            return result.Count > 0;
        }

        [CanBeNull]
        public static T TryGetValue<T>(this List<T> list, int index)
        {
            if (index < 0 || index >= list.Count)
                return default;
            
            return list[index];
        }

        public static T TryGetValue<T>(this T[] array, int index)
            => array.ToList().TryGetValue(index);
        
        public static string ToSnakeCase(this object property)
        {
            return Regex.Replace(
                property.ToString(),
                "(?<!^)([A-Z])", "_$1"
            );
        }

        public static void Print(this StringBuilder builder)
        {
            Debug.Log(builder.ToString());
        }
    }
}