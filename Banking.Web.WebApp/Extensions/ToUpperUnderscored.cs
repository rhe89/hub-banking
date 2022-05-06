namespace Banking.Web.WebApp.Extensions;

public static class StringExtensions
{
    public static string ToUpperUnderscored(this string value)
    {
        return value.ToUpper().Replace(" ", "_");
    }
}