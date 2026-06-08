using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Account.Models.SalaryDetail
{
    public class MainViewModel
    {
        // DataGrid 最终绑定的数据源
        public ObservableCollection<DisplayItem> GridData { get; set; } = new ObservableCollection<DisplayItem>();
        // 合计金额属性
        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged(); // 通知 UI 刷新
            }
        }
        // 模拟加载和转换数据的方法
        public void LoadData(List<RawDataObject> rawList)
        {
            GridData.Clear();

            foreach (var raw in rawList)
            {
                // 每一个原始对象，都拆分成 7 条展示数据
                GridData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "实发绩效", Amount = raw.dataf_95 });
                GridData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "绩效扣款", Amount = raw.dataf_96 });
                GridData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "税前扣款", Amount = raw.dataf_63 });
                GridData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "社保", Amount = raw.dataf_158 });
                GridData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "公积金", Amount = raw.dataf_159 });
                GridData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "个税", Amount = raw.dataf_5 });
                GridData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "实发工资", Amount = raw.dataf_3 });
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // 你的原始数据结构
    public class RawDataObject
    {
        public string? datacyear
        {
            get; set;
        }
        public decimal? dataf_95
        {
            get; set;
        }
        public decimal? dataf_96
        {
            get; set;
        }
        public decimal? dataf_63
        {
            get; set;
        }
        public decimal? dataf_158
        {
            get; set;
        }
        public decimal? dataf_159
        {
            get; set;
        }
        public decimal? dataf_5
        {
            get; set;
        }
        public decimal? dataf_3
        {
            get; set;
        }
    }
}
