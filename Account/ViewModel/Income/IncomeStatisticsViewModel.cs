using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Account.ViewModel.Income
{
    public class IncomeStatisticsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<IncomeStatisticsModel> _incomeList;

        public IncomeStatisticsViewModel(ObservableCollection<IncomeStatisticsModel> _IncomeList)
        {
            IncomeList = _IncomeList;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ======= 必须这样写才能通知到前端 =======
        public ObservableCollection<IncomeStatisticsModel> IncomeList
        {
            get => _incomeList;
            set
            {
                _incomeList = value;
                OnPropertyChanged(); // 🔥 关键：当引用改变时通知前端
            }
        }
        private decimal? _maxAmount = 100; // 默认给个100防止除以0
        public decimal? MaxAmount
        {
            get => _maxAmount;
            set
            {
                _maxAmount = value;
                OnPropertyChanged();
            }
        }
    }
}
