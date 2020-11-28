using UnityEngine;
using System.Collections;

public static class IntBitFlags
{
	public static bool HasFlag(int flags, int flag)
	{
		return HasFlag((uint)flags, (uint)flag);
	}

	public static bool HasFlag(uint flags, uint flag)
	{
		uint flagsValue = (uint)flags;
		uint flagValue = (uint)flag;

		return (flagsValue & flagValue) != 0;
	}

	public static bool HasFlagIndex(int flags, int idx)
	{
		return HasFlag((uint)flags, 1U << idx);
	}
	public static bool HasFlagIndex(uint flags, int idx)
	{
		return HasFlag(flags, 1U << idx);
	}

	public static void SetFlag(ref uint flags, uint flag)
	{
		uint flagsValue = flags;
		uint flagValue = flag;

		flags = (flagsValue | flagValue);
	}
	public static void SetFlag(ref uint flags, int flag)
	{
		uint flagsValue = flags;
		uint flagValue = (uint)flag;

		flags = (flagsValue | flagValue);
	}

	public static void SetFlagIndex(ref uint flags, int idx)
	{
		SetFlag(ref flags, 1U << idx);
	}

	public static void ClearFlag(ref uint flags, uint flag)
	{
		uint flagsValue = (uint)flags;
		uint flagValue = (uint)flag;
		flags = (flagsValue & (~flagValue));
	}
	public static void ClearFlag(ref uint flags, int flag)
	{
		uint flagsValue = (uint)flags;
		uint flagValue = (uint)flag;
		flags = (flagsValue & (~flagValue));
	}
	public static void ClearFlagIndex(ref uint flags, int idx)
	{
		ClearFlag(ref flags, 1U << idx);
	}
}


public static class LongBitFlags
{
	public static bool HasFlag(long flags, long flag)
	{
		return HasFlag((ulong)flags, (ulong)flag);
	}

	public static bool HasFlag(ulong flags, ulong flag)
	{
		ulong flagsValue = (ulong)flags;
		ulong flagValue = (ulong)flag;

		return (flagsValue & flagValue) != 0;
	}

	public static bool HasFlagIndex(long flags, int idx)
	{
		return HasFlag((ulong)flags, 1U << idx);
	}
	public static bool HasFlagIndex(ulong flags, int idx)
	{
		return HasFlag(flags, 1U << idx);
	}

	public static void SetFlag(ref ulong flags, ulong flag)
	{
		ulong flagsValue = flags;
		ulong flagValue = flag;

		flags = (flagsValue | flagValue);
	}
	public static void SetFlagIndex(ref ulong flags, int idx)
	{
		SetFlag(ref flags, 1U << idx);
	}

	public static void ClearFlag(ref ulong flags, ulong flag)
	{
		ulong flagsValue = (ulong)flags;
		ulong flagValue = (ulong)flag;

		flags = (flagsValue & (~flagValue));
	}
	public static void ClearFlagIndex(ref ulong flags, int idx)
	{
		ClearFlag(ref flags, 1U << idx);
	}
}
