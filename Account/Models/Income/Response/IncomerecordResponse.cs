using Account.Common;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

namespace Account.Models.Income.Response
{
    public class IncomerecordResponse : INotifyPropertyChanged
    {
        private int _categoryId;
        public int incomeId
        {
            get; set;
        }
        public int categoryId
        {
            get => _categoryId;
            set
            {
                if (_categoryId != value)
                {
                    _categoryId = value;
                    OnPropertyChanged();
                }
            }
        }
        public string? categoryName
        {
            get; set;
        }
        public decimal? incomeAmount
        {
            get; set;
        }
        [JsonConverter(typeof(MyDateTimeConverter))]
        private DateTime _incomeTime;
        [JsonConverter(typeof(MyDateTimeConverter))]
        public DateTime incomeTime
        {
            get => _incomeTime;
            set
            {
                _incomeTime = value;
                // 当时间被赋值时，自动把年份也赋过去
                incomeYear = _incomeTime.Year;
                incomeMonth = _incomeTime.Month;
            }
        }
        public int incomeYear
        {
            get; set;
        }
        public int? incomeMonth
        {
            get; set;
        }
        [JsonConverter(typeof(MyDateTimeConverter))]
        public DateTime createTime
        {
            get; set;
        } = DateTime.UtcNow;
        public string? incomeNote
        {
            get; set;
        } = "";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
