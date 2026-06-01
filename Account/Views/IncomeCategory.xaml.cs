using Account.Models.Income.Request;
using Account.Models.Income.Response;

using HandyControl.Controls;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json; // .NET 5+ 提供的便捷扩展包
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Account.Views
{
    /// <summary>
    /// IncomeCategory.xaml 的交互逻辑
    /// </summary>
    public partial class IncomeCategory : Page
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private string incomerecordItems = "/api/incomerecord-items";
        List<IncomerecordResponse>? incomerecordList;
        public IncomeCategory()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            incomerecordList = new List<IncomerecordResponse>();
            HttpRequest();
        }
        private async void HttpRequest()
        {
            var startDateStr = "2014-01-01";
            var endDateStr = DateTime.Today.ToString("yyyy-MM-dd");
            var postJson = new IncomerecordRequest { startTime = startDateStr, endTime = endDateStr };

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(App.host + incomerecordItems, postJson);

            if (!response.IsSuccessStatusCode)
            {
                // 专门读取服务器返回的错误文本
                string errorDetails = await response.Content.ReadAsStringAsync();
                var statusCode = response.StatusCode;
                // 可以在这里根据 errorDetails 进一步调试
                Growl.Error("数据获取失败！StatusCode：" + statusCode + "，ErrorDetails：" + errorDetails);

            }
            if (cboxIncomeCategory.SelectedItem is ComboBoxItem selectedItem)
            {
                var incomeCategory = selectedItem.Content.ToString();
                string responseJson = await response.Content.ReadAsStringAsync();
                if (responseJson != null)
                {
                    List<IncomerecordResponse>? incomerecordResponse = JsonSerializer.Deserialize<List<IncomerecordResponse>>(responseJson);
                    if (incomerecordResponse != null)
                    {
                        incomerecordList = incomerecordResponse;
                        var matchList = incomerecordList?.Where(r => r.categoryName == incomeCategory).GroupBy(x => new { x.incomeDate, x.categoryName })
                   .Select(g => new
                   {
                       g.Key.incomeDate,
                       g.Key.categoryName,
                       incomeAmount = g.Sum(x => x.incomeAmount)
                   })
                   .ToList();

                        incomeCategoryDataGrid.ItemsSource = matchList?.OrderByDescending(t => t.incomeDate);
                        var sumAmount = matchList?.Sum(t => t.incomeAmount);
                        tblockSummary.Text = sumAmount.ToString();

                        var matchYearList = incomerecordList?.Where(r => r.categoryName == incomeCategory).GroupBy(x => new { x.incomeYear, x.categoryName })
                   .Select(g => new
                   {
                       g.Key.incomeYear,
                       g.Key.categoryName,
                       incomeAmount = g.Sum(x => x.incomeAmount)
                   })
                   .ToList();
                        incomeYearCategoryDataGrid.ItemsSource = matchYearList?.OrderByDescending(t => t.incomeYear);
                        var sumYearAmount = matchYearList?.Sum(t => t.incomeAmount);
                        tblockYearSummary.Text = sumYearAmount.ToString();

                    }
                }
            }
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // 获取当前行的索引（从 0 开始，所以加 1 变成序号）
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void cboxIncomeCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboxIncomeCategory.SelectedItem is ComboBoxItem selectedItem)
            {
                if (selectedItem.Content == null)
                    return;
                var incomeCategory = selectedItem.Content.ToString();
                if (incomeCategory == "全部")
                {
                    var matchList = incomerecordList?.GroupBy(x => new { x.incomeDate, x.categoryName })
           .Select(g => new
           {
               g.Key.incomeDate,
               g.Key.categoryName,
               incomeAmount = g.Sum(x => x.incomeAmount)
           })
           .ToList();

                    incomeCategoryDataGrid.ItemsSource = matchList?.OrderByDescending(t => t.incomeDate);
                    var sumAmount = matchList?.Sum(t => t.incomeAmount);
                    tblockSummary.Text = sumAmount.ToString();

                    var matchYearList = incomerecordList?.GroupBy(x => new { x.incomeYear, x.categoryName })
          .Select(g => new
          {
              g.Key.incomeYear,
              g.Key.categoryName,
              incomeAmount = g.Sum(x => x.incomeAmount)
          })
          .ToList();
                    incomeYearCategoryDataGrid.ItemsSource = matchYearList?.OrderByDescending(t => t.incomeYear);
                    var sumYearAmount = matchYearList?.Sum(t => t.incomeAmount);
                    tblockYearSummary.Text = sumYearAmount.ToString();
                }
                else
                {
                    var matchList = incomerecordList?.Where(r => r.categoryName == incomeCategory).GroupBy(x => new { x.incomeDate, x.categoryName })
           .Select(g => new
           {
               g.Key.incomeDate,
               g.Key.categoryName,
               incomeAmount = g.Sum(x => x.incomeAmount)
           })
           .ToList();

                    incomeCategoryDataGrid.ItemsSource = matchList?.OrderByDescending(t => t.incomeDate);
                    var sumAmount = matchList?.Sum(t => t.incomeAmount);
                    tblockSummary.Text = sumAmount.ToString();

                    var matchYearList = incomerecordList?.Where(r => r.categoryName == incomeCategory).GroupBy(x => new { x.incomeYear, x.categoryName })
          .Select(g => new
          {
              g.Key.incomeYear,
              g.Key.categoryName,
              incomeAmount = g.Sum(x => x.incomeAmount)
          })
          .ToList();
                    incomeYearCategoryDataGrid.ItemsSource = matchYearList?.OrderByDescending(t => t.incomeYear);
                    var sumYearAmount = matchYearList?.Sum(t => t.incomeAmount);
                    tblockYearSummary.Text = sumYearAmount.ToString();
                }
            }
        }
    }
}
