using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Account.Models.Consump.Response
{
    public class CategoryResponse : INotifyPropertyChanged
    {
        private int _categoryId;
        public int categoryId
        {
            get;
            set
            {
                _categoryId = value;
                OnPropertyChanged();
            }
        }
        public string? categoryName
        {
            get; set;
        }
        public string? categoryType
        {
            get; set;
        }
        public int priority
        {
            get; set;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
