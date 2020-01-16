using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public static class Utils {
    public static bool StartsWith(this string str, KeyWords value)
    {
        return str.StartsWith(value.ToString());
    }
    public static bool StartsWithAny(this string str, params KeyWords[] values)
    {
        if (!values.Any())
        {
            values = Enum.GetValues(typeof(KeyWords)).Cast<KeyWords>().ToArray();
        }
        foreach (var val in values)
        {
            if (str.Equals(val.ToString(), StringComparison.CurrentCultureIgnoreCase) || str.StartsWith(val + " ") || str.StartsWith(val + "("))
            {
                return true;
            }
        }
        return false;
    }
    public static T AppendNew<T>(this Transform parent, T pref, Vector3 position) where  T : MonoBehaviour
    {
        var o = Object.Instantiate(pref, position, Quaternion.identity);
        o.transform.parent = parent;
        return o;
    }

    public static List<T> SplitList<T>(this List<T> list, T item)
    {
        for (int i = list.Count - 1; i > 0; i -= 1)
        {
            list.Insert(i, item);
        }
        return list;
    }

    public static string Cut(this string str, int from, int to)
    {
        return str.Substring(from, 1 +  to - from);
    }
    public static string ReverseCut(this string str, int from, int to)
    {
        return str.Remove(from, to - from + 1);
    }
    public static string TrimString(this string str, string stringToTrim)
    {
        int i = str.IndexOf(stringToTrim,StringComparison.OrdinalIgnoreCase);
        if (i != 0)
        {
            return str;
        }

        return str.Remove(0, stringToTrim.Length);
    }

    public static string ContainsAny(this string str, IEnumerable<string> values)
    {
        foreach (var v in values)
        {
            if (str.Contains(v))
            {
                return v;
            }
        }
        return null;
    }
    public static string ContainsAny(this string str, params string[] values)
    {
        return str.ContainsAny(values as IEnumerable<string>);
    }

    public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> self, IEnumerable<KeyValuePair<TKey, TValue>> values)
    {
        foreach (var pair in values)
        {
            self.Add(pair.Key,pair.Value);
        }
    }

    public static void SafeDestroyObject<T>(this T obj) where T : MonoBehaviour
    {
        if (obj != null)
        {
            GameObject.Destroy(obj.gameObject);
        }
    }

    public static Color EditColor(this Color baseColor, float? r = null, float? g = null, float? b = null, float? a = null)
    {
        return new Color(r ?? baseColor.r, g ?? baseColor.g, b ?? baseColor.b, a ?? baseColor.a);
    }

    public static NameColor[] ToWithNewColor(this NameColor[] names, Color color)
    {
        return names.Select(x => new NameColor(x.Name, color)).ToArray();
    }

    public static T GetRandomItem<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new Exception("Can't get random from nothing");
        }
        return list[Random.Range(0,list.Count - 1)];
    }

    public static string ToHex(this Color color)
    {
        return "#" + ColorUtility.ToHtmlStringRGB(color).ToLower();
    }
    public static Vector2 NanVector2 { get { return new Vector2(float.NaN, float.NaN);} }

    public static RaycastHit2D RaycastTo(this Vector2 from, Vector2 to, int layermask = 0)
    {
        if (layermask == 0)
        {
            return Physics2D.Raycast(from, to - from, Vector2.Distance(from, to));
        }
        else
        {
            return Physics2D.Raycast(from, to - from, Vector2.Distance(from, to), layermask);
        }
    }
    public static RaycastHit2D RaycastTo(this Vector3 from, Vector2 to, int layermask = 0)
    {
        if (layermask == 0)
        {
            return Physics2D.Raycast(from, to - (Vector2) from, Vector2.Distance(from, to));
        }
        else
        {
            return Physics2D.Raycast(from, to - (Vector2)from, Vector2.Distance(from, to), layermask);
        }
    }
    public static Vector2 Expand(this Vector2 vector, float ratio)
    {
        return vector + vector.normalized * ratio;
    }

    public static void InsertRangeFixed(this List<Vector3> list, int index, IEnumerable<Vector3> toInsert)
    {
        foreach (var item in toInsert.Reverse())
        {
            list.Insert(index, item);
        }
    }
    public static float GetPathLength(this Vector2[] list)
    {
        float length = 0;
        for (int i = 0; i < list.Length - 1; i++)
        {
            length += Vector2.Distance(list[i], list[i+1]);
        }
        return length;
    }
    public static float GetPathLength(this Vector3[] list)
    {
        float length = 0;
        for (int i = 0; i < list.Length - 1; i++)
        {
            length += Vector3.Distance(list[i], list[i + 1]);
        }
        return length;
    }
    public static T MinElement<T>(this IEnumerable<T> source, Func<T, float> selector)
    {
        float minValue = 0;
        T minElement = default(T);
        bool hasValue = false;

        foreach (T s in source)
        {
            float x = selector(s);
            if (hasValue)
            {
                if (x < minValue)
                {
                    minValue = x;
                    minElement = s;
                }
            }
            else
            {
                minValue = x;
                minElement = s;
                hasValue = true;
            }
        }

        if (hasValue)
        {
            return minElement;
        }

        throw new InvalidOperationException("MinElement: No elements in sequence.");
    }
}

public enum Compare
{
    Equal,
    Better,
    Worse
}