using System;

public class TimeUtil
{
	public static readonly DateTime _unixTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	public const long DayInMillisec = 1000 * 60 * 60 * 24;
	public const long HourInMillisec = 1000 * 60 * 60;
	public const long MinuteInMillisec = 1000 * 60;

    static long _utcoffsetTime = 0;

    public static long utcOffset
    {
        get
        {
            if (_utcoffsetTime == 0)
            {
                TimeZoneInfo localZone = TimeZoneInfo.Local;
                TimeSpan offset = localZone.GetUtcOffset(System.DateTime.Now);
                _utcoffsetTime = TimeStampConst.Hour * offset.Hours + TimeStampConst.Minute * offset.Minutes;
            }

            return _utcoffsetTime;
        }
    }

    public static int GetDaySequenceForRenewTime(long timestamp, int renewHour, int renewMin , int serverTimeZoneSeconds)
	{
		DateTime localDateTime = TimeUtil.TimestampToServerDateTime(timestamp - renewHour * TimeStampConst.Hour - renewMin * TimeStampConst.Minute , serverTimeZoneSeconds);
		return localDateTime.Year * 1000 + localDateTime.DayOfYear;
	}

	
	public static long GetDeviceTimeForOffline()
	{
		TimeSpan epochTicks = new TimeSpan(_unixTimeUtc.Ticks);
		TimeSpan span = new TimeSpan(DateTime.UtcNow.Ticks) - epochTicks;
		return (long)span.TotalMilliseconds;
	}

	public static DateTime TimestampToLocalDateTimeForOffline(long timestamp)
	{
		DateTime utc = _unixTimeUtc.AddMilliseconds(timestamp);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Local);
    }

	public static long LocalDateTimeToTimestampForOffline(DateTime localDateTime)
	{
		DateTime utc = TimeZoneInfo.ConvertTimeToUtc(localDateTime);
        TimeSpan span = utc.Subtract(_unixTimeUtc);
		return (long)span.TotalMilliseconds;
	}

    public static long LocalDateTimeToTimestamp(DateTime localDateTime)
    {
        TimeSpan utcOffsetSpan = new TimeSpan(TimeSpan.TicksPerMillisecond * utcOffset);
        DateTime utc = localDateTime - utcOffsetSpan;
        TimeSpan span = utc.Subtract(_unixTimeUtc);
        return (long)span.TotalMilliseconds;
    }

    public static DateTime TimestampToServerDateTime(long timestamp ,int serverTimeZoneSeconds)
    {
        DateTime utc = TimeUtil._unixTimeUtc.AddMilliseconds(timestamp);
        TimeSpan utcOffsetSpan = new TimeSpan(TimeSpan.TicksPerMillisecond * serverTimeZoneSeconds * 1000);
        return utc + utcOffsetSpan;
    }

    public static long ServerDataTimeToTimestamp(DateTime serverDateTime, int serverTimeZoneSeconds)
    {
        TimeSpan utcOffsetSpan = new TimeSpan(TimeSpan.TicksPerMillisecond * serverTimeZoneSeconds * 1000);
        DateTime utc = serverDateTime - utcOffsetSpan;
        TimeSpan span = utc.Subtract(TimeUtil._unixTimeUtc);
        return (long)span.TotalMilliseconds;
    }
    
    public static DateTime ParseDateTime(string text)
	{
		return DateTime.Parse(text);
	}

	public static TimeSpan DiffFromServerTime(long curUtc)
	{
        long timeSeconds = utcOffset - curUtc * 1000;

		long hour   =  timeSeconds / TimeStampConst.Hour;
		long minute = (timeSeconds % TimeStampConst.Hour) / TimeStampConst.Minute;

		TimeSpan offset = new TimeSpan((int)hour, (int)minute, 0);

		return offset;
	}

    public static DateTime TimestampToLocalDateTime(long timestamp)
    {
        DateTime utc = TimeUtil._unixTimeUtc.AddMilliseconds(timestamp);
        TimeSpan utcOffsetSpan = new TimeSpan(TimeSpan.TicksPerMillisecond * utcOffset);
        return utc + utcOffsetSpan;
    }

    public static string FormatTimeLeft_HMMSS(long millisecLeft)
	{
		millisecLeft += 999; 
		if (millisecLeft >= 3600000)
			return string.Format("{0}:{1:00}:{2:00}", millisecLeft / 3600000, (millisecLeft % 3600000) / 60000, (millisecLeft % 60000) / 1000);
		else
			return string.Format("{0}:{1:00}", millisecLeft / 60000, (millisecLeft % 60000) / 1000);
	}

	public static string FormatTime_HMMSS(long timestamp)
	{
		return FormatTime_HMMSS(TimestampToLocalDateTime(timestamp));
	}
	public static string FormatTime_HMMSS(DateTime localTime)
	{
		return string.Format("{0}:{1:00}:{2:00}", localTime.Hour, localTime.Minute, localTime.Second);
	}
	public static string FormatTime_YYYYMMDD_HMMSS(DateTime localTime)
	{
		return string.Format("{0}/{1:00}/{2:00} {3}:{4:00}:{5:00}", localTime.Year, localTime.Month, localTime.Day, localTime.Hour, localTime.Minute, localTime.Second);
	}

	public static string FormatTime_HMM(long timestamp)
	{
		return FormatTime_HMM(TimestampToLocalDateTime(timestamp));
	}
	public static string FormatTime_HMM(DateTime localTime)
	{
		return string.Format("{0}:{1:00}", localTime.Hour, localTime.Minute);
	}

	public static string TimeAgoToString(System.TimeSpan ts)
	{
		if (ts.Days > 0)
		{
			return string.Format("{0} 일전", ts.Days);
		}
		else if (ts.Hours > 0)
		{
			return string.Format("{0} 시간전", ts.Hours);
		}
		else if (ts.Minutes > 0)
		{
			return string.Format("{0} 분전", ts.Minutes);
		}
		return string.Format("{0} 초전", ts.Seconds);
	}
}

public class TimeStampConst
{
	public const long Day = 1000 * 60 * 60 * 24;
	public const long Hour = 1000 * 60 * 60;
	public const long Minute = 1000 * 60;
	public const long Second = 1000;
}