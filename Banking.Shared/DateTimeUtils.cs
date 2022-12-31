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
    
    public static DateTime FirstDayOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    public static DateTime FirstDayOfMonth()
    {
        return FirstDayOfMonth(DateTime.Now);
    }
    
    public static DateTime LastDayOfMonth(int? year, int? month)
    {
        year ??= DateTime.Now.Year;
        month ??= DateTime.Now.Month;
        
        return new DateTime(year.Value, month.Value, DateTime.DaysInMonth(year.Value, month.Value));
    }
    
    public static DateTime LastDayOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    }
    
    public static DateTime LastDayOfMonth()
    {
        return LastDayOfMonth(DateTime.Now);
    }
}