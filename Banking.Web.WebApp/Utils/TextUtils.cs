using System;

namespace Banking.Web.WebApp.Utils;

public static class TextUtils
{
    public static string GetMonthText(int month) => month switch
    {
        1 => "January",
        2 => "February",
        3 => "March",
        4 => "April",
        5 => "May",
        6 => "June",
        7 => "July",
        8 => "August",
        9 => "September",
        10 => "October",
        11 => "November",
        12 => "December",
        _ => throw new ArgumentException("Invalid month", nameof(month))
    };
}