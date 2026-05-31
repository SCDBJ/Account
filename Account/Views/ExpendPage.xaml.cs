using Account.Models.Consump.Request;
using Account.Models.Consump.Response;

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
    /// ExpendPage.xaml 的交互逻辑
    /// </summary>
    public partial class ExpendPage : Page
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private string consumprecordItems = "/api/consumprecord-items";
        private string consumprecordDelete = "/api/consumprecord-delete";
        private string consumprecordAdd = "/api/consumprecord-add";
        List<ConsumprecordResponse>? consumprecordList;
        public ExpendPage()
        {
            InitializeComponent();
            // 计算本月第一天并赋值
            DateTime now = DateTime.Now;
            startDatePicker.SelectedDate = new DateTime(now.Year, now.Month, 1);
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            consumprecordList = new List<ConsumprecordResponse>();
            HttpRequest();
            BindingYearMonth();
        }
        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDate = startDatePicker.SelectedDate ?? DateTime.Today;
            DateTime endDate = endDatePicker.SelectedDate ?? DateTime.Today;
            var matchList = consumprecordList?.Where(x => x.consumpTime >= startDate && x.consumpTime <= endDate);
            consumpDataGrid.ItemsSource = matchList;
            var sumAmount = matchList?.Sum(t => t.consumpAmount);
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
            string currentMonth = DateTime.Now.Month.ToString();
            cboxStatisticsMonth.SelectedValue = currentMonth;
        }
        private async void HttpRequest()
        {
            DateTime startDate = startDatePicker.SelectedDate ?? DateTime.Today;
            //var startDateStr = startDate.ToString("yyyy-MM-dd");
            var startDateStr = "2014-01-01";
            DateTime endDate = endDatePicker.SelectedDate ?? DateTime.Today;
            var endDateStr = endDate.ToString("yyyy-MM-dd");
            var postJson = new ConsumprecordRequest { startTime = startDateStr, endTime = endDateStr };

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(App.host + consumprecordItems, postJson);

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
                List<ConsumprecordResponse>? consumprecordResponse = JsonSerializer.Deserialize<List<ConsumprecordResponse>>(responseJson);
                if (consumprecordResponse != null)
                {
                    consumprecordList = consumprecordResponse;
                    consumpDataGrid.ItemsSource = consumprecordResponse.Where(t => t.consumpTime >= startDate).OrderByDescending(t => t.consumpTime);


                    int statisticsYear = Convert.ToInt16(cboxStatisticsYear.SelectedValue);
                    int statisticsMonth = Convert.ToInt16(cboxStatisticsMonth.SelectedValue);
                    var matchList = consumprecordList?.Where(r => r.consumpYear == statisticsYear && r.consumpMonth == statisticsMonth).GroupBy(x => new { x.consumpYear, x.consumpMonth, x.categoryName })
                .Select(g => new
                {
                    g.Key.consumpYear,
                    g.Key.consumpMonth,
                    g.Key.categoryName,
                    consumpAmount = g.Sum(x => x.consumpAmount)
                })
                .ToList();
                    consumpStatisticsDataGrid.ItemsSource = matchList;
                    var sumAmount = matchList?.Sum(t => t.consumpAmount);
                    tblockSummary.Text = sumAmount.ToString();
                    statisticsSummary.Text = sumAmount.ToString();
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
                ConsumprecordResponse? currentItem = button.DataContext as ConsumprecordResponse;

                if (hc.MessageBox.Show("确定要删除吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    HttpResponseMessage response = await _httpClient.DeleteAsync(App.host + consumprecordDelete + $"/{currentItem?.consumpId}");
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
            ExpendAdd expendAdd = new ExpendAdd();
            if (expendAdd.ShowDialog() == true)
            {
                ConsumprecordResponse newConsumprecord = expendAdd.ConsumprecordData;
                ConsumprecordAddRequest consumprecordAddRequest = new ConsumprecordAddRequest() { categoryId = newConsumprecord.categoryId, consumpAmount = newConsumprecord.consumpAmount, consumpNote = newConsumprecord.consumpNote, consumpTime = newConsumprecord.consumpTime };
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(App.host + consumprecordAdd, consumprecordAddRequest);
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
                            dgtc_consumpYear.Visibility = Visibility.Visible;
                            dgtc_consumpMonth.Visibility = Visibility.Visible;
                            dgtc_consumpType.Visibility = Visibility.Visible;

                            if (cboxStatisticsYear != null)
                                cboxStatisticsYear.Visibility = Visibility.Visible;
                            if (cboxStatisticsMonth != null)
                                cboxStatisticsMonth.Visibility = Visibility.Visible;

                            if (cboxStatisticsYear != null && cboxStatisticsMonth != null)
                            {
                                int statisticsYear1 = Convert.ToInt16(cboxStatisticsYear.SelectedValue);
                                int statisticsMonth1 = Convert.ToInt16(cboxStatisticsMonth.SelectedValue);
                                var matchList = consumprecordList?.Where(r => r.consumpYear == statisticsYear1 && r.consumpMonth == statisticsMonth1).GroupBy(x => new { x.consumpYear, x.consumpMonth, x.categoryName })
           .Select(g => new
           {
               g.Key.consumpYear,
               g.Key.consumpMonth,
               g.Key.categoryName,
               consumpAmount = g.Sum(x => x.consumpAmount)
           })
           .ToList();
                                consumpStatisticsDataGrid.ItemsSource = matchList;
                                statisticsSummary.Text = matchList?.Sum(x => x.consumpAmount).ToString();
                            }

                            break;
                        case "2"://年份+月份
                            dgtc_consumpYear.Visibility = Visibility.Visible;
                            dgtc_consumpMonth.Visibility = Visibility.Visible;
                            dgtc_consumpType.Visibility = Visibility.Collapsed;
                            cboxStatisticsMonth.Visibility = Visibility.Collapsed;

                            cboxStatisticsYear.Visibility = Visibility.Visible;

                            int statisticsYear2 = Convert.ToInt16(cboxStatisticsYear.SelectedValue);
                            var matchList1 = consumprecordList?.Where(r => r.consumpYear == statisticsYear2).GroupBy(x => new { x.consumpYear, x.consumpMonth })
        .Select(g => new
        {
            g.Key.consumpYear,
            g.Key.consumpMonth,
            consumpAmount = g.Sum(x => x.consumpAmount)
        })
        .ToList();
                            consumpStatisticsDataGrid.ItemsSource = matchList1;
                            statisticsSummary.Text = matchList1?.Sum(x => x.consumpAmount).ToString();
                            break;
                        case "3"://年份+类别
                            dgtc_consumpYear.Visibility = Visibility.Visible;
                            dgtc_consumpType.Visibility = Visibility.Visible;
                            dgtc_consumpMonth.Visibility = Visibility.Collapsed;

                            cboxStatisticsYear.Visibility = Visibility.Visible;
                            cboxStatisticsMonth.Visibility = Visibility.Collapsed;

                            int statisticsYear3 = Convert.ToInt16(cboxStatisticsYear.SelectedValue);
                            var matchList2 = consumprecordList?.Where(r => r.consumpYear == statisticsYear3).GroupBy(x => new { x.consumpYear, x.categoryName })
        .Select(g => new
        {
            g.Key.consumpYear,
            g.Key.categoryName,
            consumpAmount = g.Sum(x => x.consumpAmount)
        })
        .ToList().OrderByDescending(t => t.consumpAmount);
                            consumpStatisticsDataGrid.ItemsSource = matchList2;
                            statisticsSummary.Text = matchList2?.Sum(x => x.consumpAmount).ToString();
                            break;
                        case "4"://年份
                            dgtc_consumpYear.Visibility = Visibility.Visible;
                            dgtc_consumpType.Visibility = Visibility.Collapsed;
                            dgtc_consumpMonth.Visibility = Visibility.Collapsed;

                            cboxStatisticsYear.Visibility = Visibility.Collapsed;
                            cboxStatisticsMonth.Visibility = Visibility.Collapsed;

                            var matchList3 = consumprecordList?.GroupBy(x => new { x.consumpYear })
      .Select(g => new
      {
          g.Key.consumpYear,
          consumpAmount = g.Sum(x => x.consumpAmount)
      })
      .ToList().OrderBy(t => t.consumpYear);
                            consumpStatisticsDataGrid.ItemsSource = matchList3;
                            statisticsSummary.Text = matchList3?.Sum(x => x.consumpAmount).ToString();
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
                    var matchList = consumprecordList?.Where(r => r.consumpYear == statisticsYear && r.consumpMonth == statisticsMonth).GroupBy(x => new { x.consumpYear, x.consumpMonth, x.categoryName })
        .Select(g => new
        {
            g.Key.consumpYear,
            g.Key.consumpMonth,
            g.Key.categoryName,
            consumpAmount = g.Sum(x => x.consumpAmount)
        })
        .ToList();
                    consumpStatisticsDataGrid.ItemsSource = matchList;
                    statisticsSummary.Text = matchList?.Sum(x => x.consumpAmount).ToString();
                    break;
                case 1:
                    var matchList1 = consumprecordList?.Where(r => r.consumpYear == statisticsYear).GroupBy(x => new { x.consumpYear, x.consumpMonth })
        .Select(g => new
        {
            g.Key.consumpYear,
            g.Key.consumpMonth,
            consumpAmount = g.Sum(x => x.consumpAmount)
        })
        .ToList();
                    consumpStatisticsDataGrid.ItemsSource = matchList1;
                    statisticsSummary.Text = matchList1?.Sum(x => x.consumpAmount).ToString();
                    break;
                case 2:
                    var matchList2 = consumprecordList?.Where(r => r.consumpYear == statisticsYear).GroupBy(x => new { x.consumpYear, x.categoryName })
        .Select(g => new
        {
            g.Key.consumpYear,
            g.Key.categoryName,
            consumpAmount = g.Sum(x => x.consumpAmount)
        })
        .ToList().OrderByDescending(t => t.consumpAmount);
                    consumpStatisticsDataGrid.ItemsSource = matchList2;
                    statisticsSummary.Text = matchList2?.Sum(x => x.consumpAmount).ToString();
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
                    var matchList = consumprecordList?.Where(r => r.consumpYear == statisticsYear && r.consumpMonth == statisticsMonth).GroupBy(x => new { x.consumpYear, x.consumpMonth, x.categoryName })
        .Select(g => new
        {
            g.Key.consumpYear,
            g.Key.consumpMonth,
            g.Key.categoryName,
            consumpAmount = g.Sum(x => x.consumpAmount)
        })
        .ToList();
                    consumpStatisticsDataGrid.ItemsSource = matchList;
                    statisticsSummary.Text = matchList?.Sum(x => x.consumpAmount).ToString();
                    break;
            }
        }
    }
}
