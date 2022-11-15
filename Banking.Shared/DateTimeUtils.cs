using System;

namespace Banking.Shared;

public static class DateTimeUtils
{
    public static DateTime FirstDayOfMonth(int? year, int? month)
    {
        year ??= DateTime.Now.Year;
        month ??= DateTime.Now.Month;
        
        return new DateTime(year.Value, month.Value, 1);
    }

    public static DateTime FirstDayOfMonth()
    {
        return FirstDayOfMonth(null, null);
    }
    
    public static DateTime LastDayOfMonth(int? year, int? month)
    {
        year ??= DateTime.Now.Year;
        month ??= DateTime.Now.Month;
        
        return new DateTime(year.Value, month.Value, DateTime.DaysInMonth(year.Value, month.Value));
    }
    
    public static DateTime LastDayOfMonth()
    {
        return LastDayOfMonth(null, null);
    }
}