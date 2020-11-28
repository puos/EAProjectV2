using UnityEngine;
using System;
using System.Collections.Generic;

public static class ArrayUtil
{
	public static T[] RemoveAt<T>(this T[] source, int index)
	{
		T[] dest = new T[source.Length - 1];
		if (index > 0)
			Array.Copy(source, 0, dest, 0, index);

		if (index < source.Length - 1)
			Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

		return dest;
	}

	public static T[] InsertAt<T>(this T[] source, int index, T value)
	{
		T[] dest = new T[source.Length + 1];
		if(index > 0)
			Array.Copy(source, 0, dest, 0, index);
		if(index < source.Length)
			Array.Copy(source, index, dest, index + 1, source.Length - index);
		dest[index] = value;
		
		return dest;
	}

	public static T[] Add<T>(this T[] source, T value)
	{
		if (source == null)
		{
			T[] dest = new T[1];
			dest[0] = value;
			return dest;
		}
		return InsertAt(source, source.Length, value);
	}

	public static T[] Add<T>(this T[] source, T[] value)
	{
		if (source == null)
		{
			if (value == null)
				return null;

			T[] dest = new T[value.Length];
			Array.Copy(value, 0, dest, 0, value.Length);
			return dest;
		}
		else
		{
			T[] dest = new T[source.Length + value.Length];
			Array.Copy(source, 0, dest, 0, source.Length);
			Array.Copy(value, 0, dest, source.Length, value.Length);
			return dest;
		}
	}

	public static bool Exists<T>(this T[] source, T value)
	{
		return IndexOf(source, value) != -1;
	}
	public static int IndexOf<T>(this T[] source, T value)
	{
		if (source == null)
			return -1;
		
		return Array.IndexOf(source, value);
	}

	public static int FindIndex<T>(this T[] source, Predicate<T> match)
	{
		if (source == null)
			return -1;
		return Array.FindIndex<T>(source, match);
	}

	public static int FindLastIndex<T>(this T[] source, Predicate<T> match)
	{
		if (source == null)
			return -1;
		return Array.FindLastIndex<T>(source, match);
	}

	public static int[] FindAllIndexOf<T>(this T[] source, Predicate<T> match)
	{
		T[] values = Array.FindAll<T>(source, match);
		//return (from T item in subArray select Array.IndexOf(a, item)).ToArray();
		int[] ret = new int[values.Length];
		for (int i = 0; i < values.Length; i++)
			ret[i] = Array.IndexOf(source, values[i]);
		return ret;
	}

	public static T Find<T>(this T[] source, Predicate<T> match)
	{
		int idx = Array.FindIndex<T>(source, match);
		return idx == -1 ? default(T) : source[idx];
	}

	public static T[] FindAll<T>(this T[] source, Predicate<T> match)
	{
		return Array.FindAll<T>(source, match);
	}
	public static T[] Resize<T>(this T[] source, int size)
	{
		int sourceLenth = (source == null) ? 0 : source.Length;
		if (sourceLenth == size)
			return source;

		T[] dest = new T[size];
		for (int i = 0; i < Mathf.Min(sourceLenth, size); i++)
			dest[i] = source[i];
		
		return dest;
	}

	public static T[] MoveUp<T>(this T[] source, int[] indices)
	{
		if (indices.Length == 0 || indices[0] == 0)
			return source;

		for (int i = 0; i < indices.Length; i++)
		{
			T temp = source[indices[i] - 1];
			source[indices[i]-1] = source[indices[i]];
			source[indices[i]] = temp;
		}
		return source;
	}

	public static T[] MoveDown<T>(this T[] source, int[] indices)
	{
		if (indices.Length == 0 || indices[indices.Length-1] == source.Length - 1)
			return source;

		for (int i = indices.Length-1; i >= 0; i--)
		{
			T temp = source[indices[i]+1];
			source[indices[i]+1] = source[indices[i]];
			source[indices[i]] = temp;
		}
		return source;
	}

	public static T[] MoveUp<T>(this T[] source, Predicate<T> match)
	{
		int[] indices = FindAllIndexOf(source, match);
		return MoveUp(source, indices);
	}
	public static T[] MoveDown<T>(this T[] source, Predicate<T> match)
	{
		int[] indices = FindAllIndexOf(source, match);
		return MoveDown(source, indices);
	}
	public static T[] RemoveAt<T>(this T[] source, Predicate<T> match)
	{
		int[] indices = FindAllIndexOf(source, match);
		for (int i = indices.Length - 1; i >= 0; i--)
			source = RemoveAt(source, indices[i]);
		return source;
	}
	public static int CountIf<T>(this T[] source, Predicate<T> match)
	{
		return Array.FindAll<T>(source, match).Length;
	}

	public static void ForEach<T>(this T[] source, Action<T> action)
	{
		Array.ForEach<T>(source, action);
	}

	public static void Sort<T>(this T[] source)
	{
		Array.Sort<T>(source);
	}
	public static void Sort<T>(this T[] source, Comparison<T> comp)
	{
		Array.Sort<T>(source, comp);
	}

	public static void Fill<T>(this T[] source, T val)
	{
		for (int i = 0; i < source.Length; i++)
		{
			source[i] = val;
		}
	}

	public static void Shuffle<T>(this T[] source, System.Random rand = null)
	{
		if (rand == null)
			rand = new System.Random();
		for (var i = 0; i < source.Length; i++)
			source.Swap(i, rand.Next(i, source.Length));
	}

	public static void Swap<T>(this T[] source, int i, int j)
	{
		var temp = source[i];
		source[i] = source[j];
		source[j] = temp;
	}

	public static T[] Distinct<T>(this T[] source)
	{
		List<T> uniques = new List<T>();
		foreach (T item in source)
		{
			if (!uniques.Contains(item)) uniques.Add(item);
		}
		return uniques.ToArray();
	}

	public static bool Equals<T>(T[] a, T[] b) where T : UnityEngine.Object
	{
		if ((a == null && b != null) || (a != null && b == null))
			return false;
		if (a == null && b == null)
			return true;
		if (a.Length != b.Length)
			return false;
		for (int i = 0; i < a.Length; i++)
			if (a[i] != b[i])
				return false;
		return true;
	}

	public static bool IsNullOrEmpty<T>(this T[] source)
	{
		return source == null || source.Length == 0;
	}

	public static void CopyTo<T>(this T[] source, T[] target)
	{
		for (int i = 0; i < source.Length; i++)
			target[i] = source[i];
	}
}
