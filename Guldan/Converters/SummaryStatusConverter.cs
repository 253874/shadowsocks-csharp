using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Guldan.Converters
{
    [ValueConversion(typeof(Status), typeof(string))]
    public class SummaryStatusConverter : MarkupExtension, IValueConverter
    {
        private static SummaryStatusConverter converter;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return converter ?? (converter = new SummaryStatusConverter());
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (Status?)value ?? Status.Busy;
            var resolveType = parameter as string;
            if (resolveType == "TrayIcon")
            {
                switch (status)
                {
                    case Status.Ready:
                        return "/Resources/Icons/TrayIcons/Ready.ico";
                    case Status.Disabled:
                        return "/Resources/Icons/TrayIcons/Disabled.ico";
                    default:
                        return "/Resources/Icons/TrayIcons/Busy.ico";
                }
            }
            if (resolveType == "ToolTipImage")
            {
                switch (status)
                {
                    case Status.Ready:
                        return "/Resources/Images/TrayIcons/Ready.png";
                    case Status.Disabled:
                        return "/Resources/Images/TrayIcons/Disabled.png";
                    default:
                        return "/Resources/Images/TrayIcons/Busy.png";
                }
            }
            if (resolveType == "ToolTipText")
            {
                return $"{I18N.GetSplitString("Current status is :")} {I18N.GetString(status.ToString("G"))}";
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
