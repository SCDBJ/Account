using Account.Models.Income.Request;
using Account.Models.Income.Response;
using Account.ViewModel.Income;
using Account.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq; // 确保引入了 LINQ
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Account.Views
{
    public partial class IncomeCategory : Page
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private string incomerecordItems = "/api/incomerecord-items";

        List<IncomerecordResponse> incomerecordList = new List<IncomerecordResponse>();
        private IncomeStatisticsViewModel _viewModel;

        // 标记位：防止页面还没加载完，ComboBox 默认选中触发异常
        private bool _isDataLoaded = false;

        public IncomeCategory()
        {
            InitializeComponent();
            // 构造函数只创建 ViewModel 并绑定，里面先留空集合
            _viewModel = new IncomeStatisticsViewModel(new ObservableCollection<IncomeStatisticsModel>());
            this.DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. 等待网络数据完全返回
            await HttpRequestAsync();

            // 2. 标记数据已就绪
            _isDataLoaded = true;

            // 3. 刷新全界面（表格 + 柱状图）
            RefreshAllUiData();
        }

        // 统一控制所有 UI 刷新的核心方法
        private void RefreshAllUiData()
        {
            if (cboxIncomeCategory == null || cboxIncomeCategory.SelectedItem is not ComboBoxItem selectedItem)
                return;

            if (selectedItem.Content == null)
                return;

            var incomeCategory = selectedItem.Content.ToString();

            // ==================== 1. 更新两个数据表格 (DataGrid) ====================
            List<IncomerecordResponse> filteredList = incomeCategory == "全部"
                ? incomerecordList
                : incomerecordList.Where(r => r.categoryName == incomeCategory).ToList();

            // 天统计
            var matchList = filteredList.GroupBy(x => new { x.incomeDate, x.categoryName })
                .Select(g => new { g.Key.incomeDate, g.Key.categoryName, incomeAmount = g.Sum(x => x.incomeAmount) })
                .ToList();
            incomeCategoryDataGrid.ItemsSource = matchList.OrderByDescending(t => t.incomeDate);
            tblockSummary.Text = matchList.Sum(t => t.incomeAmount).ToString();

            // 年统计
            var matchYearList = filteredList.GroupBy(x => new { x.incomeYear, x.categoryName })
                .Select(g => new { g.Key.incomeYear, g.Key.categoryName, incomeAmount = g.Sum(x => x.incomeAmount) })
                .ToList();
            incomeYearCategoryDataGrid.ItemsSource = matchYearList.OrderByDescending(t => t.incomeYear);
            tblockYearSummary.Text = matchYearList.Sum(t => t.incomeAmount).ToString();


            // ==================== 2. 🔥 刷新柱状图数据 (ItemsControl) ====================
            // 柱状图按日期分组展示
            var chartData = filteredList.GroupBy(x => new { x.incomeYear })
                .Select(g => new
                {
                    incomeYear = g.Key.incomeYear,
                    incomeAmount = g.Sum(x => x.incomeAmount)
                }).ToList();

            var newModels = new ObservableCollection<IncomeStatisticsModel>();
            foreach (var m in chartData)
            {
                newModels.Add(new IncomeStatisticsModel
                {
                    incomeYear = m.incomeYear,
                    IncomeAmount = m.incomeAmount
                });
            }

            // 重新赋值给 ViewModel 触发属性变更通知
            _viewModel.IncomeList = newModels;
        }

        private async Task HttpRequestAsync()
        {
            try
            {
                var startDateStr = "2014-01-01";
                var endDateStr = DateTime.Today.ToString("yyyy-MM-dd");
                var postJson = new IncomerecordRequest { startTime = startDateStr, endTime = endDateStr };

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(App.host + incomerecordItems, postJson);

                if (!response.IsSuccessStatusCode)
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    HandyControl.Controls.Growl.Error($"数据获取失败！StatusCode：{response.StatusCode}，ErrorDetails：{errorDetails}");
                    return;
                }

                string responseJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseJson))
                {
                    var incomerecordResponse = JsonSerializer.Deserialize<List<IncomerecordResponse>>(responseJson);
                    if (incomerecordResponse != null)
                    {
                        incomerecordList = incomerecordResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error("请求发生异常: " + ex.Message);
            }
        }

        // 下拉框切换事件
        private void cboxIncomeCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 如果页面还没 Loaded 完，网络数据还没回来，不要盲目刷新
            if (!_isDataLoaded)
                return;

            RefreshAllUiData();
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}