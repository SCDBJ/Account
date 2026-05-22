using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Account.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // 1. 创建具体的数据源（使用 ObservableCollection 实现动态更新）
        private readonly ObservableCollection<double> _chartValues = new() { 2, 1, 3, 5, 3, 4, 6 };

        // 2. 暴露给 XAML 绑定的 Series 集合
        [ObservableProperty]
        private ISeries[] _seriesCollection;

        public MainViewModel()
        {
            // 初始化 Series 并将数据源赋给它
            SeriesCollection = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = _chartValues,
                    Fill = null // 如果不需要折线下方阴影，设置为 null
                }
            };
        }

        // 模拟动态添加数据的方法（图表会自动同步刷新）
        public void AddNewData(double val)
        {
            _chartValues.Add(val);
        }
    }
}
