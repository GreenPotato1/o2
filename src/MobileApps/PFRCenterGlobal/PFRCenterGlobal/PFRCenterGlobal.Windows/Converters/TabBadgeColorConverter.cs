using PFRCenterGlobal.Windows.Helpers;
using UI = Windows.UI;

namespace PFRCenterGlobal.Windows.Converters
{
    public class TabBadgeColorConverter : UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Page)
            {
                var badgeColor = CustomTabbedPage.GetBadgeColor((Page)value);

                var color = ColorHelper.XamarinFormColorToWindowsColor(badgeColor);

                return new SolidColorBrush(color);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
