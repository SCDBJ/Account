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
        public ObservableCollection<DisplayItem> GridBaseData { get; set; } = new ObservableCollection<DisplayItem>();
        public void LoadData(List<RawDataObject> rawList)
        {
            GridData.Clear();

            foreach (var raw in rawList)
            {
                GridData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "绩效扣款", Amount = raw.dataf_96 });
                GridData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "税前扣款", Amount = raw.dataf_63 });
                GridData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "社保", Amount = raw.dataf_158 });
                GridData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "个税", Amount = raw.dataf_5 });
            }
        }
        public void LoadBaseData(List<RawBaseDataObject> rawList)
        {
            GridBaseData.Clear();

            foreach (var raw in rawList)
            {
                GridBaseData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "核定工资", Amount = raw.dataf_32 });
                GridBaseData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "公积金单位", Amount = raw.dataf_162 });
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RawDataObject
    {
        public string? datacyear
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
        public decimal? dataf_5
        {
            get; set;
        }
    }
    public class RawBaseDataObject
    {
        public string? datacyear
        {
            get; set;
        }
        public decimal? dataf_32
        {
            get; set;
        }
        public decimal? dataf_162
        {
            get; set;
        }
    }
}
