using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Data;
using System.Reflection;
using System.IO;
namespace WanTai.View
{
    public class HeaderConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,System.Globalization.CultureInfo culture)
        {
            var text = value.ToString();
            if (string.IsNullOrEmpty(text)) return null;
          
            var panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            panel.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            StringBuilder strinBuilder = new StringBuilder();
            foreach (string str in text.Split(','))
            {
                if (string.IsNullOrEmpty(str)) continue;
                panel.Children.Add(new TextBlock() { Text = str, VerticalAlignment= System.Windows.VerticalAlignment.Top, HorizontalAlignment = System.Windows.HorizontalAlignment.Left });
                //strinBuilder.Append("<TextBlock  HorizontalAlignment=\"Left\"  Text=\"" + str + "\" VerticalAlignment=\"Top\" />");
                //Stream stream = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(SBuilder.ToString()));
                //DataGridTemplateColumn colIcon = XamlReader.Load(stream) as DataGridTemplateColumn;
            }
           // Array.ForEach(text.Split(','), s => panel.Children.Add(new TextBlock() { Text = s }));
            //Array.ForEach(text.Split(','), s => strinBuilder.Append("<TextBlock  HorizontalAlignment=\"Left\"  Text=\""+s+"\" VerticalAlignment=\"Top\" />"));
            return panel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
