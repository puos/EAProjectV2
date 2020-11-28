using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public static class ListUtil
{
	public static int CountIf<T>(this List<T> source, Predicate<T> match)
	{
		return source.FindAll(match).Count;
	}

	public static void Shuffle<T>(this IList<T> source, System.Random rand = null)
	{
		if (rand == null)
			rand = new System.Random();
		for (var i = 0; i < source.Count; i++)
			source.Swap(i, rand.Next(i, source.Count));
	}

	public static void Swap<T>(this IList<T> source, int i, int j)
	{
		var temp = source[i];
		source[i] = source[j];
		source[j] = temp;
	}

	public static void Resize<T>(this IList<T> source, int newSize)
	{
		// 넘어가는 삭제
		for (int i = source.Count - 1; i >= source.Count; i--)
			source.RemoveAt(i);
		// 부족하면 추가
		for (int i = source.Count; i < newSize; i++)
			source.Add(default(T));
	}

	public static List<T> Distinct<T>(this IList<T> source)
	{
		if (source == null)
			return null;
		List<T> uniques = new List<T>();
		foreach (T item in source)
		{
			if (!uniques.Contains(item))
				uniques.Add(item);
		}
		return uniques;
	}

	public static bool IsNullOrEmpty<T>(this IList<T> source)
	{
		return source == null || source.Count == 0;
	}

}