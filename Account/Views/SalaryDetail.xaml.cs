using Account.Common;
using Account.Models.Consump.Request;
using Account.Models.Consump.Response;
using Account.Models.SalaryDetail;

using HandyControl.Controls;

using System;
using System.Collections.Generic;
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
    /// SalaryDetail.xaml 的交互逻辑
    /// </summary>
    public partial class SalaryDetail : Page
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private string salaryrecordItems = "/api/salaryrecord-items";
        // 1. 将服务器返回的原始数据缓存到类级别，方便切换年份时直接使用，不用重新请求网络
        private List<SalaryItem>? _cachedSalaryItems;
        private MainViewModel _viewModel;
        public SalaryDetail()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            BindingYear();
           
            if (cboxStatisticsYear.SelectedItem == null)
                return;
            int statisticsYear = int.Parse(cboxStatisticsYear.SelectedItem.ToString()!);

            var postJson = new SalaryrecordRequest { startYear = 2021, endYear = DateTime.Now.Year };

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(App.host + salaryrecordItems, postJson);
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
            if (responseJson != null)
            {
                List<SalaryItem>? salaryItem = JsonSerializer.Deserialize<List<SalaryItem>>(responseJson);

                SalaryDatagrid.ItemsSource = salaryItem?.OrderByDescending(t => t.datacyear).ThenByDescending(t => t.datacperiod);

                Dispatcher.Invoke(new Action(() => sumdataf_32.Text = salaryItem?.Sum(t=>t.dataf_32).ToString()));//核定工资总额合计
                Dispatcher.Invoke(new Action(() => sumdataf_3.Text = salaryItem?.Sum(t => t.dataf_3).ToString()));//实发合计
                Dispatcher.Invoke(new Action(() => sumdataf_163.Text = salaryItem?.Sum(t => t.dataf_163).ToString()));//扣减合计
                _cachedSalaryItems = salaryItem;
                if (salaryItem != null)
                {
                    var viewModel = new MainViewModel();
                    _viewModel = viewModel;
                    this.DataContext = viewModel;

                    List<SalaryItem>? matchList = salaryItem.Where(t => t.datacyear == statisticsYear).ToList();
                    decimal? total = 0.00M;
                    if (matchList != null)
                    {
                        var rawData = new List<RawDataObject>();
                        RawDataObject rawDataObject = new RawDataObject();
                        foreach (var item in matchList)
                        {
                            rawDataObject = new RawDataObject
                            {
                                datacyear = item.datacyear.ToString(),
                                //dataf_95 = item.dataf_95,
                                dataf_96 = -item.dataf_96,
                                dataf_63 = item.dataf_63,
                                dataf_158 = item.dataf_158,
                                //dataf_159 = item.dataf_159,
                                dataf_5 = item.dataf_5,
                                //dataf_3 = item.dataf_3
                            };
                            rawData.Add(rawDataObject);
                            total += /*item.dataf_95 +*/ -item.dataf_96 + item.dataf_63 + item.dataf_158 /*+ item.dataf_159 */+ item.dataf_5 /*+ item.dataf_3*/;
                        }
                        var rawDatas= rawData.GroupBy(t=>new { t.datacyear}).
                            Select(g=> new
                            {
                                g.Key.datacyear,
                                //dataf_95 = g.Sum(x => x.dataf_95),
                                dataf_96 = g.Sum(x => -x.dataf_96),
                                dataf_63 = g.Sum(x => x.dataf_63),
                                dataf_158 = g.Sum(x => x.dataf_158),
                                //dataf_159 = g.Sum(x => x.dataf_159*2),
                                dataf_5 = g.Sum(x => x.dataf_5),
                                //dataf_3 = g.Sum(x => x.dataf_3),
                            }).ToList();
                        List<RawDataObject> rawList = new List<RawDataObject>();
                        foreach (var raw in rawDatas)
                        {
                            rawList.Add(new RawDataObject { datacyear=raw.datacyear, /*dataf_95 = raw.dataf_95,*/ dataf_96=-raw.dataf_96, dataf_63=raw.dataf_63, dataf_158=raw.dataf_158, /*dataf_159=raw.dataf_159, */dataf_5=raw.dataf_5/*, dataf_3=raw.dataf_3 */});
                        }
                        viewModel.LoadData(rawList);

                        txtTotalAmount.Text = total.ToString();
                    }
                }
            }
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void cboxStatisticsYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 【防崩开关 1】如果缓存数据还没拿到，或者是初始化引起的切换，直接拦截
            if (_cachedSalaryItems == null || cboxStatisticsYear.SelectedItem == null)
                return;

            try
            {
                // 安全拿到年份
                int statisticsYear = int.Parse(cboxStatisticsYear.SelectedItem.ToString()!);

                // 🔍 【修复】直接从缓存里筛选指定年份的所有期间数据
                List<SalaryItem> matchList = _cachedSalaryItems.Where(t => t.datacyear == statisticsYear).ToList();

                if (matchList.Count > 0)
                {
                    // 1. 将该年份下多条月份数据进行汇总（按年分组）
                    var yearGroup = matchList.GroupBy(t => t.datacyear)
                        .Select(g => new RawDataObject
                        {
                            datacyear = g.Key.ToString(),
                            //dataf_95 = g.Sum(x => x.dataf_95),
                            dataf_96 = g.Sum(x => -x.dataf_96), // 绩效扣款在界面上要体现为负数
                            dataf_63 = g.Sum(x => x.dataf_63),
                            dataf_158 = g.Sum(x => x.dataf_158),
                            //dataf_159 = g.Sum(x => x.dataf_159*2),
                            dataf_5 = g.Sum(x => x.dataf_5),
                            //dataf_3 = g.Sum(x => x.dataf_3),
                        }).ToList();

                    // 2. 🔍 【修复】改用类级别的 _viewModel，而不是局部 new，确保前端能收到通知
                    _viewModel.LoadData(yearGroup);

                    // 3. 🔍 【修复】合计金额应该直接等于 DataGrid 里当前展示的所有项的 Amount 总和，避免符号陷阱
                    decimal? total = _viewModel.GridData.Sum(item => item.Amount);

                    // 格式化输出带两位小数的金额
                    txtTotalAmount.Text = total.ToString();
                }
                else
                {
                    // 没找到数据就清空
                    _viewModel.GridData.Clear();
                    txtTotalAmount.Text = "0.00";
                }
            }
            catch (Exception ex)
            {
                Growl.Error($"切换年份时发生错误: {ex.Message}");
            }
        }
        private void BindingYear()
        {
            cboxStatisticsYear.Items.Clear();
            for (int i = DateTime.Now.Year; i >= 2014; i--)
            {
                cboxStatisticsYear.Items.Add(i);
            }
            cboxStatisticsYear.SelectedIndex = 0;
        }
    }
}
