using System.Threading.Tasks;
using MudBlazor.Services;

namespace Banking.Web.WebApp.Extensions;

public static class ResizeServiceExtensions
{
    public static async Task<bool> IsDesktop(this IResizeService resizeService)
    {
        var windowSize = await resizeService.GetBrowserWindowSize();

        return windowSize.Width > 600;
    }
}