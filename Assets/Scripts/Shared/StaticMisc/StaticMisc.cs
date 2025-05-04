using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;
using Shared.Misc;
using UnityEngine;
using Unity.Mathematics;

namespace Shared.Misc
{
    public class StaticMisc : MonoBehaviour
    {
        public static List<string> EmptyStringList = new ();
        
        public static void DestroyAllChildren(Transform parent)
        {
            foreach (Transform child in parent)
                Destroy(child.gameObject);
        }
        
        public static void InvokeThenClear(ref Action action)
        {
            action?.Invoke();
            action = null;
        }

        public static float RandomGaussian(float mean, float stdDev)
        {
            var rand = Xoshiro.Create();
            var u1 = 1.0f - rand.NextFloat();
            var u2 = 1.0f - rand.NextFloat();

            var normal = math.sqrt(-2.0f * math.log(u1)) * math.sin(2.0f * math.PI * u2);
            return mean + stdDev * normal;
        }

        public static void LoadJson<T>(string fileName, out T obj)
        {
            obj = default;
            var path = Path.Combine(Application.streamingAssetsPath, $"{fileName}.json");

            if (!File.Exists(path))
                return;

            var json = File.ReadAllText(path);
            obj = JsonUtility.FromJson<T>(json);
        }

        public static Guid GenerateGuid(string input)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return new Guid(hash);
        }
        
        public static IEnumerator AwaitAsync(Task task)
        {
            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted)
                Debug.LogError("Task failed: " + task.Exception);
        }
        
        public static List<T> GetSelectedFlags<T>(Enum type)
        {
            return Enum.GetValues(type.GetType())
                .Cast<Enum>()
                .Where(flag => type.HasFlag(flag) && !flag.Equals(Enum.ToObject(type.GetType(), 0)))
                .Cast<T>()
                .ToList();
        }
        
        public static List<string> EnumsToStrings(Enum value)
        {
            return GetSelectedFlags<Enum>(value).Select(x => x.ToString()).ToList();
        }
        
        public static T StringsToEnums<T>(List<string> labels) where T : Enum
        {
            var result = labels
                .Aggregate(0, (prev, cur) =>
                {
                    if (!Enum.TryParse(typeof(T), cur, out var parsed))
                        return prev;
                    
                    return prev | Convert.ToInt32(parsed);
                });

            return (T)Enum.ToObject(typeof(T), result);
        }

        public static bool Compare(int value1, int value2, CompareOperator @operator)
            => @operator switch
            {
                CompareOperator.Equality           => value1 == value2,
                CompareOperator.Inequality         => value1 != value2,
                CompareOperator.LessThanOrEqual    => value1 <= value2,
                CompareOperator.GreaterThanOrEqual => value1 >= value2,
                CompareOperator.LessThan           => value1 < value2,
                CompareOperator.GreaterThan        => value1 > value2,
                _                                  => false
            };
    }
}