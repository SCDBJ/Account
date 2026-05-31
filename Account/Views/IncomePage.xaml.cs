using Account.Models.Income.Request;
using Account.Models.Income.Response;

using HandyControl.Controls;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json; // .NET 5+ 提供的便捷扩展包
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

using hc = HandyControl.Controls;
namespace Account.Views
{
    /// <summary>
    /// IncomePage.xaml 的交互逻辑
    /// </summary>
    public partial class IncomePage : Page
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private string incomerecordItems = "/api/incomerecord-items";
        private string incomerecordDelete = "/api/incomerecord-delete";
        private string incomerecordAdd = "/api/incomerecord-add";
        List<IncomerecordResponse>? incomerecordList;
        public IncomePage()
        {
            InitializeComponent();
            // 计算本月第一天并赋值
            DateTime now = DateTime.Now;
            startDatePicker.SelectedDate = new DateTime(now.Year, now.Month-1, 1);
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            incomerecordList = new List<IncomerecordResponse>();
            HttpRequest();
            BindingYearMonth();
        }
        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDate = startDatePicker.SelectedDate ?? DateTime.Today;
            DateTime endDate = endDatePicker.SelectedDate ?? DateTime.Today;
            var matchList = incomerecordList?.Where(x => x.incomeTime >= startDate && x.incomeTime <= endDate);
            incomeDataGrid.ItemsSource = matchList;
            var sumAmount = matchList?.Sum(t => t.incomeAmount);
            tblockSummary.Text = sumAmount.ToString();
        }
        private void BindingYearMonth()
        {
            cboxStatisticsYear.Items.Clear();
            for (int i = DateTime.Now.Year; i >= 2014; i--)
            {
                cboxStatisticsYear.Items.Add(i.ToString());
            }
            cboxStatisticsYear.SelectedIndex = 0;
            cboxStatisticsMonth.Items.Clear();
            for (int j = 12; j >= 1; j--)
            {
                cboxStatisticsMonth.Items.Add(j.ToString());
            }
            string currentMonth = DateTime.Now.AddMonths(-1).Month.ToString();
            cboxStatisticsMonth.SelectedValue = currentMonth;
        }
        private async void HttpRequest()
        {
            DateTime startDate = startDatePicker.SelectedDate ?? DateTime.Today;
            //var startDateStr = startDate.ToString("yyyy-MM-dd");
            var startDateStr = "2014-01-01";
            DateTime endDate = endDatePicker.SelectedDate ?? DateTime.Today;
            var endDateStr = endDate.ToString("yyyy-MM-dd");
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

            string responseJson = await response.Content.ReadAsStringAsync();
            if (responseJson != null)
            {
                List<IncomerecordResponse>? incomerecordResponse = JsonSerializer.Deserialize<List<IncomerecordResponse>>(responseJson);
                if (incomerecordResponse != null)
                {
                    incomerecordList = incomerecordResponse;
                    incomeDataGrid.ItemsSource = incomerecordResponse.Where(t => t.incomeTime >= startDate).OrderByDescending(t => t.incomeTime);
                    var sumAmount = incomerecordResponse.Where(t => t.incomeTime >= startDate)?.Sum(t => t.incomeAmount);
                    tblockSummary.Text = sumAmount.ToString();

                    int statisticsYear = Convert.ToInt16(cboxStatisticsYear.SelectedValue);
                    int statisticsMonth = Convert.ToInt16(cboxStatisticsMonth.SelectedValue);
                    var matchList = incomerecordList?.Where(r => r.incomeYear == statisticsYear && r.incomeMonth == statisticsMonth).GroupBy(x => new { x.incomeYear, x.incomeMonth, x.categoryName })
                .Select(g => new
                {
                    g.Key.incomeYear,
                    g.Key.incomeMonth,
                    g.Key.categoryName,
                    incomeAmount = g.Sum(x => x.incomeAmount)
                })
                .ToList();
                    incomeStatisticsDataGrid.ItemsSource = matchList;
                    
                    statisticsSummary.Text = matchList?.Sum(x => x.incomeAmount).ToString();
                }
            }
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // 获取当前行的索引（从 0 开始，所以加 1 变成序号）
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;
            if (button != null)
            {
                // 2. Button 的 DataContext 就是当前行绑定的实体对象
                // 假设你给 DataGrid 绑定的数据源是一个 UserModel 列表
                IncomerecordResponse? currentItem = button.DataContext as IncomerecordResponse;

                if (hc.MessageBox.Show("确定要删除吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    HttpResponseMessage response = await _httpClient.DeleteAsync(App.host + incomerecordDelete + $"/{currentItem?.incomeId}");
                    if (response.IsSuccessStatusCode)
                    {
                        hc.MessageBox.Show("删除成功！");
                        HttpRequest();
                    }
                    else
                    {
                        hc.MessageBox.Show($"删除失败，状态码: {response.StatusCode}");
                    }
                }
            }
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            IncomeAdd incomeAdd = new IncomeAdd();
            if (incomeAdd.ShowDialog() == true)
            {
                IncomerecordResponse newIncomerecord = incomeAdd.IncomerecordData;
                IncomerecordAddRequest incomerecordAddRequest = new IncomerecordAddRequest() { categoryId = newIncomerecord.categoryId, incomeAmount = newIncomerecord.incomeAmount, incomeNote = newIncomerecord.incomeNote, incomeTime = newIncomerecord.incomeTime };
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(App.host + incomerecordAdd, incomerecordAddRequest);
                if (!response.IsSuccessStatusCode)
                {
                    // 专门读取服务器返回的错误文本
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    var statusCode = response.StatusCode;
                    // 可以在这里根据 errorDetails 进一步调试
                    Growl.Error("数据获取失败！StatusCode：" + statusCode + "，ErrorDetails：" + errorDetails);

                    return;
                }

                string responseJson = await response.Content.ReadAsStringAsync();
                if (responseJson != null && responseJson.Contains("保存成功"))
                {
                    HttpRequest();
                    hc.MessageBox.Show($"添加成功！");
                }
            }
        }

        private void cboxStatistics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 确保有选中项
            if (e.AddedItems.Count > 0)
            {
                // 转换为 ComboBoxItem
                if (e.AddedItems[0] is ComboBoxItem selectedItem)
                {
                    var tag = selectedItem.Tag;
                    switch (tag)
                    {
                        case "1"://年份+月份+类别
                            dgtc_incomeYear.Visibility = Visibility.Visible;
                            dgtc_incomeMonth.Visibility = Visibility.Visible;
                            dgtc_incomeType.Visibility = Visibility.Visible;

                            if (cboxStatisticsYear != null)
                                cboxStatisticsYear.Visibility = Visibility.Visible;
                            if (cboxStatisticsMonth != null)
                                cboxStatisticsMonth.Visibility = Visibility.Visible;

                            if (cboxStatisticsYear != null && cboxStatisticsMonth != null)
                            {
                                int statisticsYear1 = Convert.ToInt16(cboxStatisticsYear.SelectedValue);
                                int statisticsMonth1 = Convert.ToInt16(cboxStatisticsMonth.SelectedValue);
                                var matchList = incomerecordList?.Where(r => r.incomeYear == statisticsYear1 && r.incomeMonth == statisticsMonth1).GroupBy(x => new { x.incomeYear, x.incomeMonth, x.categoryName })
           .Select(g => new
           {
               g.Key.incomeYear,
               g.Key.incomeMonth,
               g.Key.categoryName,
               incomeAmount = g.Sum(x => x.incomeAmount)
           })
           .ToList();
                                incomeStatisticsDataGrid.ItemsSource = matchList;
                                statisticsSummary.Text = matchList?.Sum(x => x.incomeAmount).ToString();
                            }

                            break;
                        case "2"://年份+月份
                            dgtc_incomeYear.Visibility = Visibility.Visible;
                            dgtc_incomeMonth.Visibility = Visibility.Visible;
                            dgtc_incomeType.Visibility = Visibility.Collapsed;
                            cboxStatisticsMonth.Visibility = Visibility.Collapsed;

                            cboxStatisticsYear.Visibility = Visibility.Visible;

                            int statisticsYear2 = Convert.ToInt16(cboxStatisticsYear.SelectedValue);
                            var matchList1 = incomerecordList?.Where(r => r.incomeYear == statisticsYear2).GroupBy(x => new { x.incomeYear, x.incomeMonth })
        .Select(g => new
        {
            g.Key.incomeYear,
            g.Key.incomeMonth,
            incomeAmount = g.Sum(x => x.incomeAmount)
        })
        .ToList();
                            incomeStatisticsDataGrid.ItemsSource = matchList1;
                            statisticsSummary.Text = matchList1?.Sum(x => x.incomeAmount).ToString();
                            break;
                        case "3"://年份+类别
                            dgtc_incomeYear.Visibility = Visibility.Visible;
                            dgtc_incomeType.Visibility = Visibility.Visible;
                            dgtc_incomeMonth.Visibility = Visibility.Collapsed;

                            cboxStatisticsYear.Visibility = Visibility.Visible;
                            cboxStatisticsMonth.Visibility = Visibility.Collapsed;

                            int statisticsYear3 = Convert.ToInt16(cboxStatisticsYear.SelectedValue);
                            var matchList2 = incomerecordList?.Where(r => r.incomeYear == statisticsYear3).GroupBy(x => new { x.incomeYear, x.categoryName })
        .Select(g => new
        {
            g.Key.incomeYear,
            g.Key.categoryName,
            incomeAmount = g.Sum(x => x.incomeAmount)
        })
        .ToList().OrderByDescending(t => t.incomeAmount);
                            incomeStatisticsDataGrid.ItemsSource = matchList2;
                            statisticsSummary.Text = matchList2?.Sum(x => x.incomeAmount).ToString();
                            break;
                        case "4"://年份
                            dgtc_incomeYear.Visibility = Visibility.Visible;
                            dgtc_incomeType.Visibility = Visibility.Collapsed;
                            dgtc_incomeMonth.Visibility = Visibility.Collapsed;

                            cboxStatisticsYear.Visibility = Visibility.Collapsed;
                            cboxStatisticsMonth.Visibility = Visibility.Collapsed;

                            var matchList3 = incomerecordList?.GroupBy(x => new { x.incomeYear })
      .Select(g => new
      {
          g.Key.incomeYear,
          incomeAmount = g.Sum(x => x.incomeAmount)
      })
      .ToList().OrderBy(t => t.incomeYear);
                            incomeStatisticsDataGrid.ItemsSource = matchList3;
                            statisticsSummary.Text = matchList3?.Sum(x => x.incomeAmount).ToString();
                            break;
                    }
                }
            }
        }
        private void cboxStatisticsYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int statisticsYear = Convert.ToInt16(cboxStatisticsYear.SelectedValue);
            int statisticsMonth = Convert.ToInt16(cboxStatisticsMonth.SelectedValue);

            var tag = cboxStatistics.SelectedIndex;
            switch (tag)
            {
                case 0:
                    var matchList = incomerecordList?.Where(r => r.incomeYear == statisticsYear && r.incomeMonth == statisticsMonth).GroupBy(x => new { x.incomeYear, x.incomeMonth, x.categoryName })
        .Select(g => new
        {
            g.Key.incomeYear,
            g.Key.incomeMonth,
            g.Key.categoryName,
            incomeAmount = g.Sum(x => x.incomeAmount)
        })
        .ToList();
                    incomeStatisticsDataGrid.ItemsSource = matchList;
                    statisticsSummary.Text = matchList?.Sum(x => x.incomeAmount).ToString();
                    break;
                case 1:
                    var matchList1 = incomerecordList?.Where(r => r.incomeYear == statisticsYear).GroupBy(x => new { x.incomeYear, x.incomeMonth })
        .Select(g => new
        {
            g.Key.incomeYear,
            g.Key.incomeMonth,
            incomeAmount = g.Sum(x => x.incomeAmount)
        })
        .ToList();
                    incomeStatisticsDataGrid.ItemsSource = matchList1;
                    statisticsSummary.Text = matchList1?.Sum(x => x.incomeAmount).ToString();
                    break;
                case 2:
                    var matchList2 = incomerecordList?.Where(r => r.incomeYear == statisticsYear).GroupBy(x => new { x.incomeYear, x.categoryName })
        .Select(g => new
        {
            g.Key.incomeYear,
            g.Key.categoryName,
            incomeAmount = g.Sum(x => x.incomeAmount)
        })
        .ToList().OrderByDescending(t => t.incomeAmount);
                    incomeStatisticsDataGrid.ItemsSource = matchList2;
                    statisticsSummary.Text = matchList2?.Sum(x => x.incomeAmount).ToString();
                    break;
            }
        }
        private void cboxStatisticsMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int statisticsYear = Convert.ToInt16(cboxStatisticsYear.SelectedValue);
            int statisticsMonth = Convert.ToInt16(cboxStatisticsMonth.SelectedValue);
            var tag = cboxStatistics.SelectedIndex;
            switch (tag)
            {
                case 0:
                    var matchList = incomerecordList?.Where(r => r.incomeYear == statisticsYear && r.incomeMonth == statisticsMonth).GroupBy(x => new { x.incomeYear, x.incomeMonth, x.categoryName })
        .Select(g => new
        {
            g.Key.incomeYear,
            g.Key.incomeMonth,
            g.Key.categoryName,
            incomeAmount = g.Sum(x => x.incomeAmount)
        })
        .ToList();
                    incomeStatisticsDataGrid.ItemsSource = matchList;
                    statisticsSummary.Text = matchList?.Sum(x => x.incomeAmount).ToString();
                    break;
            }
        }
    }
}
