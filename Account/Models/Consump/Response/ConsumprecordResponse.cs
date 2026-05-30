using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Account.Models.Consump.Response
{
    public class ConsumprecordResponse : INotifyPropertyChanged
    {
        private int _categoryId;
        public int consumpId
        {
            get; set;
        }
        public int categoryId
        {
            get=> _categoryId;
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
        public decimal? consumpAmount
        {
            get; set;
        }
        private DateTime _consumpTime;
        public DateTime consumpTime
        {
            get => _consumpTime;
            set
            {
                _consumpTime = value;
                // 当时间被赋值时，自动把年份也赋过去
                consumpYear = _consumpTime.Year;
                consumpMonth = _consumpTime.Month;
            }
        }
        public int consumpYear
        {
            get; set;
        }
        public int? consumpMonth
        {
            get; set;
        }
        public DateTime createTime
        {
            get; set;
        }= DateTime.UtcNow;
        public string? consumpNote
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
