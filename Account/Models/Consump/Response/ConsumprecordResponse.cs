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
        public DateTime consumpTime
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
