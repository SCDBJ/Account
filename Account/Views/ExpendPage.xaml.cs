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

namespace Account.Views
{
    /// <summary>
    /// ExpendPage.xaml 的交互逻辑
    /// </summary>
    public partial class ExpendPage : Page
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private string consumprecordUrl = "/api/consumprecord-items";
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

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(App.host + consumprecordUrl, postJson);

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
                List<ConsumprecordResponse>? consumprecordResponse = JsonSerializer.Deserialize<List<ConsumprecordResponse>>(responseJson);
                if (consumprecordResponse != null)
                {
                    consumpDataGrid.Items.Clear();
                    consumpDataGrid.ItemsSource = consumprecordResponse;
                }
            }
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // 获取当前行的索引（从 0 开始，所以加 1 变成序号）
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
