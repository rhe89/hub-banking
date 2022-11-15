using System.Globalization;
using MudBlazor;

namespace Banking.Web.WebApp.Shared;

public static class Constants
{
    public static CultureInfo CultureInfoNorway = CultureInfo.GetCultureInfo("nb-NO");
    
    public static Variant InputVariant = Variant.Outlined;
    public static Variant FormButtonVariant = Variant.Filled;

    public static string FormInputCol = "col-lg-5 col-sm-12";
}