using Account.Models.Consump.Request;
using Account.Models.Consump.Response;

using HandyControl.Controls;

using System;
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
        public ExpendPage()
        {
            InitializeComponent();
            // 计算本月第一天并赋值
            DateTime now = DateTime.Now;
            startDatePicker.SelectedDate = new DateTime(now.Year, now.Month, 1);
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HttpRequest();
        }
        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            HttpRequest();
        }
        private async void HttpRequest()
        {
            DateTime startDate = startDatePicker.SelectedDate ?? DateTime.Today;
            var startDateStr = startDate.ToString("yyyy-MM-dd");
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
                Growl.Error("数据获取失败！StatusCode："+ statusCode+ "，ErrorDetails："+ errorDetails);

                return;
            }

            string responseJson = await response.Content.ReadAsStringAsync();
            if (responseJson != null)
            {
                BindingDataGrid(responseJson);
            }
        }
        private void BindingDataGrid(string responseJson)
        {
            List<ConsumprecordResponse>? consumprecordResponse = JsonSerializer.Deserialize<List<ConsumprecordResponse>>(responseJson);
            if (consumprecordResponse != null)
            {
                consumpDataGrid.ItemsSource = consumprecordResponse.OrderByDescending(t => t.consumpTime);
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
                    HttpResponseMessage response = await _httpClient.DeleteAsync(App.host + consumprecordDelete +$"/{currentItem?.consumpId}");
                    if (response.IsSuccessStatusCode)
                    {
                        hc.MessageBox.Show("删除成功！");
                    }
                    else
                    {
                        hc.MessageBox.Show($"删除失败，状态码: {response.StatusCode}");
                    }
                }
            }
        }
    }
}
