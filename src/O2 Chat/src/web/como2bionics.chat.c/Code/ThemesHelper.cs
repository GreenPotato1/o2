using System.IO;
using System.Web;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;

namespace Com.O2Bionics.ChatService.Web.Chat
{
    public class ThemesHelper
    {
        public static string GetCssUrl(ChatWidgetAppearance widgetAppearance)
        {
            if (!string.IsNullOrWhiteSpace(widgetAppearance.CustomCssUrl))
                return widgetAppearance.CustomCssUrl;

            const string cssPathFormat = "/themes/maximized";

            return $"{cssPathFormat}/{widgetAppearance.ThemeId}/styles.css";
        }
    }

    public class ThemesMinHelper
    {
        private const string CssPathFormat = "/themes/minimized";

        public static string GetCssUrl(ChatWidgetAppearance widgetAppearance)
        {
            return $"{CssPathFormat}/{widgetAppearance.ThemeMinId}/styles.css";
        }

        public static string GetHtmlLayout(ChatWidgetAppearance widgetAppearance)
        {
            var layoutFilePath = HttpContext.Current.Server.MapPath($"{CssPathFormat}/{widgetAppearance.ThemeMinId}/min.html");

            if (File.Exists(layoutFilePath))
                return File.ReadAllText(layoutFilePath);

            return null;
        }
    }
}