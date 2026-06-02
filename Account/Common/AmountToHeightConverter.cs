using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Account.Common
{
    public class AmountToHeightConverter : IMultiValueConverter
    {
        /// <summary>
        /// 核心多值转换逻辑
        /// </summary>
        /// <param name="values">values[0] 是当前金额 (decimal), values[1] 是当前所有数据中的最大金额 (decimal)</param>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // 1. 安全检查：确保传进来的参数足够且正确
            if (values == null || values.Length < 2 || values[0] == null || values[1] == null)
                return 0.0;

            // 2. 尝试转换数据类型
            if (!double.TryParse(values[0].ToString(), out double currentAmount) ||
                !double.TryParse(values[1].ToString(), out double maxAmount))
            {
                return 0.0;
            }

            // 3. 定义柱子的最大物理高度（单位：像素，对应 XAML 里设定的高度）
            double maxVisualHeight = 300.0;

            // 4. 如果最大金额是 0（比如全是空账本），或者当前金额是负数，直接给个保底高度
            // （注意：如果要完美支持负数向下延伸，需要更复杂的双向布局，这里先做绝对值防崩溃处理）
            if (maxAmount <= 0)
                return 0.0;

            // 取绝对值计算比例，防止负数返回错误高度
            double absAmount = Math.Abs(currentAmount);

            if (absAmount >= maxAmount)
                return maxVisualHeight;

            // 5. 按比例计算出当前柱子的真实像素高度
            // 替换转换器里原本的第 5 步计算逻辑：
            double minHeight = 25.0; // 🔥 矮柱子的基础保底高度（像素）

            // 按比例计算剩余高度
            double targetHeight = minHeight + ((absAmount / maxAmount) * (maxVisualHeight - minHeight));

            return targetHeight;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
