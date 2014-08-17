using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace MediaBrowserPlayer.Classes
{
    public class SecondsToTimeSpan : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                return TimeSpan.FromSeconds((double)value);
            }
            return TimeSpan.Zero;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan)
            {
                return ((TimeSpan)value).TotalSeconds;
            }
            return 0d;
        }
    }
}
