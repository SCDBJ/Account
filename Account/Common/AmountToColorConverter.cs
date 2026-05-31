using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Account.Common
{
    public class AmountToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && decimal.TryParse(value.ToString(), out decimal amount))
            {
                if (amount > 0)
                {
                    return Brushes.Red;   // 正数显示红色
                }
                else if (amount < 0)
                {
                    return Brushes.Green; // 负数显示绿色
                }
            }
            return Brushes.Black; // 0 或解析失败时显示默认黑色
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
