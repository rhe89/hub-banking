using System;

namespace Banking.Web.WebApp.Extensions;

public static class DateTimeExtensions
{
    public static string ToNorwegianDateString(this DateTime dateTime)
    {
        return dateTime.ToString("dd.MM.yyyy");
    }
}