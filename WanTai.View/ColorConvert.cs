using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using WanTai.DataModel;
using WanTai.DataModel.Configuration;
namespace WanTai.View
{
    //定义值转换器
    [ValueConversion(typeof(string), typeof(string))]
    public class ColorConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString() == "Tube")
                return null;
            if (value.ToString() == "Complement")
            {
               LiquidType _LiquidType=  SessionInfo.LiquidTypeList.Find(delegate(LiquidType lt) { return lt.TypeId == 1; });
               if (_LiquidType == null) return null;

               return _LiquidType.Color;
            }
            if (value.ToString() == "PositiveControl")
            {
                LiquidType _LiquidType = SessionInfo.LiquidTypeList.Find(delegate(LiquidType lt) { return lt.TypeId == 2; });
                if (_LiquidType == null) return null;

                return _LiquidType.Color;
            }
            if (value.ToString() == "NegativeControl")
            {
                LiquidType _LiquidType = SessionInfo.LiquidTypeList.Find(delegate(LiquidType lt) { return lt.TypeId == 3; });
                if (_LiquidType == null) return null;

                return _LiquidType.Color;
            }

            return null;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return null;
        }
    }
}
