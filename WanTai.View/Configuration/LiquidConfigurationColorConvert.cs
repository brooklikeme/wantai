using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

using WanTai.DataModel.Configuration;

namespace WanTai.View
{
    //定义值转换器
    [ValueConversion(typeof(string), typeof(string))]
    public class LiquidConfigurationColorConvert : IValueConverter
    {
        List<LiquidType> types = WanTai.Common.Configuration.GetLiquidTypes();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           if (value.ToString() == "hasRecord")
            {
                return "LightGray";
            }            
            
            if (types != null)
            {
                foreach (LiquidType type in types)
                {
                    if (type.TypeId.ToString() == value.ToString())
                    {
                        return type.Color;
                    }
                }
            }

            return null;           
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return null;
        }
    }
}
