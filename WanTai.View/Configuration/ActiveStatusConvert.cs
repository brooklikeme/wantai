using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.Windows;

namespace WanTai.View
{
    //定义值转换器
    [ValueConversion(typeof(string), typeof(string))]
    public class ActiveStatusConvert : IValueConverter
    {        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString() == parameter.ToString())
            {
                return Visibility.Visible;
            }           
            else
                return Visibility.Hidden;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
