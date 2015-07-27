using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
namespace WanTai.View
{
    //定义值转换器
    [ValueConversion(typeof(string), typeof(string))]
    public class TubeShowID : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value.ToString()))
                return "";
            return "ID:" + value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return null;
        }
    }
}
