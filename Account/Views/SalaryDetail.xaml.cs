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
           
            if (cboxYear.SelectedItem == null)
                return;
            int statisticsYear = int.Parse(cboxYear.SelectedItem.ToString()!);
            int selectYear = int.Parse(cboxYear.SelectedItem.ToString()!);

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
                List<SalaryItem>? salaryItemFilter = salaryItem?.Where(o => o.datacyear == selectYear).OrderByDescending(t => t.datacyear).ThenByDescending(t => t.datacperiod).ToList();

                SalaryDatagrid.ItemsSource = salaryItemFilter;

                Dispatcher.Invoke(new Action(() => sumdataf_32.Text = salaryItemFilter?.Sum(t=>t.dataf_32).ToString()));//核定工资总额合计
                Dispatcher.Invoke(new Action(() => sumdataf_159.Text = salaryItemFilter?.Sum(t => t.dataf_159).ToString()));//公积金个人
                Dispatcher.Invoke(new Action(() => sumdataf_162.Text = salaryItemFilter?.Sum(t => t.dataf_162).ToString()));//公积金单位
                Dispatcher.Invoke(new Action(() => sumdataf_3.Text = salaryItemFilter?.Sum(t => t.dataf_3).ToString()));//实发合计
                Dispatcher.Invoke(new Action(() => sumdataf_163.Text = salaryItemFilter?.Sum(t => t.dataf_163).ToString()));//扣减合计
                _cachedSalaryItems = salaryItem;
                if (salaryItemFilter != null)
                {
                    var viewModel = new MainViewModel();
                    _viewModel = viewModel;
                    this.DataContext = viewModel;

                    List<SalaryItem>? matchList = salaryItemFilter.Where(t => t.datacyear == statisticsYear).ToList();
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
                                dataf_96 = -item.dataf_96,
                                dataf_63 = item.dataf_63,
                                dataf_158 = item.dataf_158,
                                dataf_5 = item.dataf_5,
                            };
                            rawData.Add(rawDataObject);
                            total += -item.dataf_96 + item.dataf_63 + item.dataf_158 + item.dataf_5;
                        }
                        var rawDatas= rawData.GroupBy(t=>new { t.datacyear}).
                            Select(g=> new
                            {
                                g.Key.datacyear,
                                dataf_96 = g.Sum(x => -x.dataf_96),
                                dataf_63 = g.Sum(x => x.dataf_63),
                                dataf_158 = g.Sum(x => x.dataf_158),
                                dataf_5 = g.Sum(x => x.dataf_5),
                            }).ToList();
                        List<RawDataObject> rawList = new List<RawDataObject>();
                        foreach (var raw in rawDatas)
                        {
                            rawList.Add(new RawDataObject { datacyear=raw.datacyear,dataf_96=-raw.dataf_96, dataf_63=raw.dataf_63, dataf_158=raw.dataf_158,dataf_5=raw.dataf_5});
                        }
                        viewModel.LoadData(rawList);
                        txtTotalAmount.Text = total.ToString();


                        decimal? basetotal = 0.00M;
                        var rawBaseData = new List<RawBaseDataObject>();
                        RawBaseDataObject rawBaseDataObject = new RawBaseDataObject();
                        foreach (var item in matchList)
                        {
                            rawBaseDataObject = new RawBaseDataObject
                            {
                                datacyear = item.datacyear.ToString(),
                                dataf_32 = item.dataf_32,
                                dataf_162 = item.dataf_162
                            };
                            rawBaseData.Add(rawBaseDataObject);
                            basetotal += item.dataf_32 +item.dataf_162 ;
                        }
                        var rawBaseDatas = rawBaseData.GroupBy(t => new { t.datacyear }).
                            Select(g => new
                            {
                                g.Key.datacyear,
                                dataf_32 = g.Sum(x => x.dataf_32),
                                dataf_162 = g.Sum(x => x.dataf_162)
                            }).ToList();

                        List<RawBaseDataObject> rawBaseList = new List<RawBaseDataObject>();
                        foreach (var raw in rawBaseDatas)
                        {
                            rawBaseList.Add(new RawBaseDataObject { datacyear = raw.datacyear, dataf_32 = raw.dataf_32, dataf_162 = raw.dataf_162});
                        }
                        viewModel.LoadBaseData(rawBaseList);
                        txtBaseTotalAmount.Text = basetotal.ToString();


                        decimal? actualtotal = 0.00M;
                        var rawActualData = new List<RawActualDataObject>();
                        RawActualDataObject rawActualDataObject = new RawActualDataObject();
                        foreach (var item in matchList)
                        {
                            rawActualDataObject = new RawActualDataObject
                            {
                                datacyear = item.datacyear.ToString(),
                                dataf_3 = item.dataf_3,
                                dataf_159 = item.dataf_159,
                                dataf_162 = item.dataf_162
                            };
                            rawActualData.Add(rawActualDataObject);
                            actualtotal += item.dataf_3 + item.dataf_159 + item.dataf_162;
                        }
                        var rawActualDatas = rawActualData.GroupBy(t => new { t.datacyear }).
                           Select(g => new
                           {
                               g.Key.datacyear,
                               dataf_3 = g.Sum(x => x.dataf_3),
                               dataf_159 = g.Sum(x => x.dataf_159),
                               dataf_162 = g.Sum(x => x.dataf_162)
                           }).ToList();
                        List<RawActualDataObject> rawActualList = new List<RawActualDataObject>();
                        foreach (var raw in rawActualDatas)
                        {
                            rawActualList.Add(new RawActualDataObject { datacyear = raw.datacyear, dataf_3 = raw.dataf_3, dataf_159 = raw.dataf_159, dataf_162 = raw.dataf_162 });
                        }
                        viewModel.LoadActualData(rawActualList);
                        txtActualTotalAmount.Text = actualtotal.ToString();
                    }
                }
            }
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
        private void BindingYear()
        {
            cboxYear.Items.Clear();
            for (int i = DateTime.Now.Year; i >= 2014; i--)
            {
                cboxYear.Items.Add(i);
            }
            cboxYear.SelectedIndex = 0;
        }

        private void cboxYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboxYear.SelectedItem == null)
                return;
            int selectYear = int.Parse(cboxYear.SelectedItem.ToString()!);
            List<SalaryItem>? salaryItemFilter = _cachedSalaryItems?.Where(o => o.datacyear == selectYear).OrderByDescending(t => t.datacyear).ThenByDescending(t => t.datacperiod).ToList();
            SalaryDatagrid.ItemsSource = salaryItemFilter;

            Dispatcher.Invoke(new Action(() => sumdataf_32.Text = salaryItemFilter?.Sum(t => t.dataf_32).ToString()));//核定工资总额合计
            Dispatcher.Invoke(new Action(() => sumdataf_159.Text = salaryItemFilter?.Sum(t => t.dataf_159).ToString()));//公积金个人
            Dispatcher.Invoke(new Action(() => sumdataf_162.Text = salaryItemFilter?.Sum(t => t.dataf_162).ToString()));//公积金单位
            Dispatcher.Invoke(new Action(() => sumdataf_3.Text = salaryItemFilter?.Sum(t => t.dataf_3).ToString()));//实发合计
            Dispatcher.Invoke(new Action(() => sumdataf_163.Text = salaryItemFilter?.Sum(t => t.dataf_163).ToString()));//扣减合计


            if (_cachedSalaryItems == null)
                return;

            try
            {
                int statisticsYear = int.Parse(cboxYear.SelectedItem.ToString()!);

                List<SalaryItem> matchList = _cachedSalaryItems.Where(t => t.datacyear == statisticsYear).ToList();

                if (matchList.Count > 0)
                {
                    var yearGroup = matchList.GroupBy(t => t.datacyear)
                        .Select(g => new RawDataObject
                        {
                            datacyear = g.Key.ToString(),
                            dataf_96 = g.Sum(x => -x.dataf_96), // 绩效扣款在界面上要体现为负数
                            dataf_63 = g.Sum(x => x.dataf_63),
                            dataf_158 = g.Sum(x => x.dataf_158),
                            dataf_5 = g.Sum(x => x.dataf_5),
                        }).ToList();

                    _viewModel.LoadData(yearGroup);
                    decimal? total = _viewModel.GridData.Sum(item => item.Amount);
                    txtTotalAmount.Text = total.ToString();


                    var yearBaseGroup = matchList.GroupBy(t => t.datacyear)
                        .Select(g => new RawBaseDataObject
                        {
                            datacyear = g.Key.ToString(),
                            dataf_32 = g.Sum(x => x.dataf_32),
                            dataf_162 = g.Sum(x => x.dataf_162)
                        }).ToList();

                    _viewModel.LoadBaseData(yearBaseGroup);
                    decimal? basetotal = _viewModel.GridBaseData.Sum(item => item.Amount);
                    txtBaseTotalAmount.Text = basetotal.ToString();


                    var yearActualGroup = matchList.GroupBy(t => t.datacyear)
                        .Select(g => new RawActualDataObject
                        {
                            datacyear = g.Key.ToString(),
                            dataf_3 = g.Sum(x => x.dataf_3),
                            dataf_159 = g.Sum(x => x.dataf_159),
                            dataf_162 = g.Sum(x => x.dataf_162)
                        }).ToList();

                    _viewModel.LoadActualData(yearActualGroup);
                    decimal? actualtotal = _viewModel.GridActualData.Sum(item => item.Amount);
                    txtActualTotalAmount.Text = actualtotal.ToString();
                }
                else
                {
                    // 没找到数据就清空
                    _viewModel.GridData.Clear();
                    txtTotalAmount.Text = "0.00";

                    _viewModel.GridBaseData.Clear();
                    txtBaseTotalAmount.Text = "0.00";

                    _viewModel.GridActualData.Clear();
                    txtActualTotalAmount.Text = "0.00";
                }
            }
            catch (Exception ex)
            {
                Growl.Error($"切换年份时发生错误: {ex.Message}");
            }
        }
    }
}
