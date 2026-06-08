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
        private decimal? sumDecdataf_32 = 0.00M;//核定工资总额合计
        private decimal? sumDecdataf_3 = 0.00M;//实发合计
        private decimal? sumDecdataf_163 = 0.00M;//扣减合计
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
            List<SalaryItem>? salaryItems = GetSalary();
            Dispatcher.Invoke(new Action(() => SalaryDatagrid.ItemsSource = salaryItems?.OrderByDescending(t => t.datacyear).ThenByDescending(t => t.datacperiod)));

            Dispatcher.Invoke(new Action(() => sumdataf_32.Text = sumDecdataf_32.ToString()));//核定工资总额合计
            Dispatcher.Invoke(new Action(() => sumdataf_3.Text = sumDecdataf_3.ToString()));//实发合计
            Dispatcher.Invoke(new Action(() => sumdataf_163.Text = sumDecdataf_163.ToString()));//扣减合计

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
                if (salaryItem != null)
                {
                    var viewModel = new MainViewModel();
                    this.DataContext = viewModel;
                    // 使用 FirstOrDefault 简化查询
                    SalaryItem? matchList = salaryItem.FirstOrDefault(t => t.datacyear == statisticsYear);

                    if (matchList != null)
                    {
                        var rawData = new List<RawDataObject>
                {
                    new RawDataObject
                    {
                        datacyear = matchList.datacyear.ToString(),
                        dataf_95 = matchList.dataf_95,
                        dataf_96 = matchList.dataf_96,
                        dataf_63 = matchList.dataf_63,
                        dataf_158 = matchList.dataf_158,
                        dataf_159 = matchList.dataf_159,
                        dataf_5 = matchList.dataf_5,
                        dataf_3 = matchList.dataf_3
                    }
                };
                        viewModel.LoadData(rawData);
                        decimal? total = matchList.dataf_95 + matchList.dataf_96 + matchList.dataf_63 + matchList.dataf_158 + matchList.dataf_159 + matchList.dataf_5 + matchList.dataf_3;
                        txtTotalAmount.Text = total.ToString();
                    }
                }
            }
        }
        /// <summary>
        /// 获取薪资明细
        /// </summary>
        /// <returns></returns>
        private List<SalaryItem>? GetSalary()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "SalaryUrls";
            IList<string>? list = FileIOHelper.FindDirectory(path);
            if (list == null)
            {
                return null;
            }
            sumDecdataf_32 = 0;
            sumDecdataf_3 = 0;
            sumDecdataf_163 = 0;
            List<SalaryItem> salaryItems = new List<SalaryItem>();

            foreach (string file in list)
            {
                Rootobject? rootobject = Newtonsoft.Json.JsonConvert.DeserializeObject<Rootobject>(file);

                if (rootobject == null)
                {
                    continue;
                }
                if (rootobject.salaryList == null)
                {
                    continue;
                }
                SalaryItem? salaryItem = new SalaryItem();
                salaryItem.datacyear = int.Parse(rootobject.salaryList?.wa_datacyear?.content ?? "0");
                salaryItem.datacperiod = int.Parse(rootobject.salaryList?.wa_datacperiod?.content ?? "0");
                salaryItem.dataf_32 = decimal.Parse(rootobject.salaryList?.wa_dataf_32?.content ?? "0");

                sumDecdataf_32 += salaryItem.dataf_32;

                salaryItem.dataf_131 = double.Parse(rootobject.salaryList?.wa_dataf_131?.content ?? "0");
                salaryItem.dataf_134 = double.Parse(rootobject.salaryList?.wa_dataf_134?.content ?? "0");
                salaryItem.dataf_40 = decimal.Parse(rootobject.salaryList?.wa_dataf_40?.content ?? "0");
                salaryItem.dataf_95 = decimal.Parse(rootobject.salaryList?.wa_dataf_95?.content ?? "0");

                // 【关键修复】优化年份期间拼接逻辑，确保月份永远是两位的“01-12”，避免出现 20215 导致无法比对
                string periodStr = salaryItem.datacperiod.ToString().PadLeft(2, '0');
                int fdate = int.Parse($"{salaryItem.datacyear}{periodStr}");

                decimal? dataf_94 = 0.00M;
                if (fdate <= 202110)
                {
                    dataf_94 = 13000M * 0.15M;
                }
                else if (fdate >= 202111 && fdate <= 202303)
                {
                    dataf_94 = 14000.00M * 0.15M;
                }
                else if (fdate >= 202304)
                {
                    dataf_94 = 14500.00M * 0.15M;
                }

                salaryItem.dataf_94 = dataf_94;

                salaryItem.dataf_96 = salaryItem.dataf_95 - salaryItem.dataf_94;

                salaryItem.dataf_97 = (salaryItem.dataf_96 / salaryItem.dataf_94 * 100)?.ToString("F2") + "%";

                if (rootobject.salaryList?.wa_dataf_63 != null)
                {
                    salaryItem.dataf_63 = decimal.Parse(rootobject.salaryList?.wa_dataf_63?.content ?? "0");
                }
                else
                {
                    salaryItem.dataf_63 = 0;
                }

                salaryItem.dataf_79 = decimal.Parse(rootobject.salaryList?.wa_dataf_79?.content ?? "0");
                salaryItem.dataf_158 = decimal.Parse(rootobject.salaryList?.wa_dataf_158?.content ?? "0");
                salaryItem.dataf_159 = decimal.Parse(rootobject.salaryList?.wa_dataf_159?.content ?? "0");
                if (rootobject.salaryList?.wa_dataf_5 != null)
                {
                    salaryItem.dataf_5 = decimal.Parse(rootobject.salaryList?.wa_dataf_5?.content ?? "0");
                }
                else
                {
                    salaryItem.dataf_5 = 0;
                }

                salaryItem.dataf_3 = decimal.Parse(rootobject.salaryList?.wa_dataf_3?.content ?? "0");
                sumDecdataf_3 += salaryItem.dataf_3;

                salaryItem.dataf_157 = decimal.Parse(rootobject.salaryList?.wa_dataf_157?.content ?? "0");
                salaryItem.dataf_162 = decimal.Parse(rootobject.salaryList?.wa_dataf_162?.content ?? "0");

                var totalDeduction = -salaryItem.dataf_96 + salaryItem.dataf_63 + salaryItem.dataf_158 + salaryItem.dataf_5;
                salaryItem.dataf_163 = totalDeduction;
                sumDecdataf_163 += salaryItem.dataf_163;
                salaryItems.Add(salaryItem);
            }
            return salaryItems;
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

            // 安全拿到年份
            int statisticsYear = int.Parse(cboxStatisticsYear.SelectedItem.ToString()!);

            // 直接从缓存里筛选，不用反复去读网络，秒级响应
            SalaryItem? matchList = _cachedSalaryItems.FirstOrDefault(t => t.datacyear == statisticsYear);

            if (matchList != null)
            {
                var rawData = new List<RawDataObject>
        {
            new RawDataObject
            {
                datacyear = matchList.datacyear.ToString(),
                dataf_95 = matchList.dataf_95,
                dataf_96 = matchList.dataf_96,
                dataf_63 = matchList.dataf_63,
                dataf_158 = matchList.dataf_158,
                dataf_159 = matchList.dataf_159,
                dataf_5 = matchList.dataf_5,
                dataf_3 = matchList.dataf_3
            }
        };
                _viewModel.LoadData(rawData); // 刷新第二张 DataGrid
                decimal? total = matchList.dataf_95 + matchList.dataf_96 + matchList.dataf_63 + matchList.dataf_158 + matchList.dataf_159 + matchList.dataf_5 + matchList.dataf_3;
                txtTotalAmount.Text = total.ToString();
            }
            else
            {
                _viewModel.GridData.Clear(); // 没找到数据就清空
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
