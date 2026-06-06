using Account.Common;
using Account.Models.SalaryDetail;

using System;
using System.Collections.Generic;
using System.Text;
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
        private decimal sumDecdataf_32 = 0.00M;//核定工资总额合计
        private decimal sumDecdataf_3 = 0.00M;//实发合计
        private decimal sumDecdataf_163 = 0.00M;//扣减合计
        public SalaryDetail()
        {
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            List<SalaryItem>? salaryItems = GetSalary();
            Dispatcher.Invoke(new Action(() => SalaryDatagrid.ItemsSource = salaryItems?.OrderByDescending(t => t.datacyear).ThenByDescending(t => t.datacperiod)));

            Dispatcher.Invoke(new Action(() => sumdataf_32.Text = sumDecdataf_32.ToString()));//核定工资总额合计
            Dispatcher.Invoke(new Action(() => sumdataf_3.Text = sumDecdataf_3.ToString()));//实发合计
            Dispatcher.Invoke(new Action(() => sumdataf_163.Text = sumDecdataf_163.ToString()));//扣减合计
        }
        /// <summary>
        /// 获取薪资明细
        /// </summary>
        /// <returns></returns>
        private List<SalaryItem>? GetSalary()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "SalaryUrls";
#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
            IList<string> list = FileIOHelper.FindDirectory(path);
#pragma warning restore CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
            if (list == null)
            {
                return null;
            }

            List<SalaryItem> salaryItems = new List<SalaryItem>();
            foreach (string file in list)
            {
#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
                Rootobject rootobject = Newtonsoft.Json.JsonConvert.DeserializeObject<Rootobject>(file);
#pragma warning restore CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。

                if (rootobject == null)
                {
                    continue;
                }
                if (rootobject.salaryList == null)
                {
                    continue;
                }
#pragma warning disable CS8602 // 解引用可能出现空引用。
                SalaryItem salaryItem = new SalaryItem();
                salaryItem.datacyear = rootobject.salaryList.wa_datacyear.content;
                salaryItem.datacperiod = rootobject.salaryList.wa_datacperiod.content;
                salaryItem.dataf_32 = rootobject.salaryList.wa_dataf_32.content;

#pragma warning disable CS8604 // 引用类型参数可能为 null。
                sumDecdataf_32 += decimal.Parse(salaryItem.dataf_32);
#pragma warning restore CS8604 // 引用类型参数可能为 null。

                salaryItem.dataf_131 = rootobject.salaryList.wa_dataf_131.content;
                salaryItem.dataf_134 = rootobject.salaryList.wa_dataf_134.content;
                salaryItem.dataf_40 = rootobject.salaryList.wa_dataf_40.content;
                salaryItem.dataf_95 = rootobject.salaryList.wa_dataf_95.content;

                #region 每年加司龄工资50，这部分不算在基本工资和绩效奖金计算公式里
                int dateEntryYear = 2021;//入职年份
                int dateEntryMonth = 8;//入职月份

#pragma warning disable CS8604 // 引用类型参数可能为 null。
                int datacyear = int.Parse(salaryItem.datacyear);
#pragma warning restore CS8604 // 引用类型参数可能为 null。
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                int datacperiod = int.Parse(salaryItem.datacperiod);
#pragma warning restore CS8604 // 引用类型参数可能为 null。

                int diffmonth = Common.Common.DiffMonth(datacyear, datacperiod, dateEntryYear, dateEntryMonth);

                int entryYears = diffmonth / 12;//入职年数
                #endregion

                salaryItem.dataf_94 = ((decimal.Parse(salaryItem.dataf_32) - 50 * entryYears) * 0.15M).ToString();

#pragma warning disable CS8604 // 引用类型参数可能为 null。
                salaryItem.dataf_96 = (decimal.Parse(salaryItem.dataf_95) - decimal.Parse(salaryItem.dataf_94)).ToString();
#pragma warning restore CS8604 // 引用类型参数可能为 null。
                salaryItem.dataf_97 = (decimal.Round(decimal.Parse(salaryItem.dataf_96) / decimal.Parse(salaryItem.dataf_94), 2) * 100).ToString() + "%";

                if (rootobject.salaryList.wa_dataf_63 != null)
                {
                    salaryItem.dataf_63 = rootobject.salaryList.wa_dataf_63.content;
                }
                else
                {
                    salaryItem.dataf_63 = "0";
                }

                salaryItem.dataf_79 = rootobject.salaryList.wa_dataf_79.content;
                salaryItem.dataf_158 = rootobject.salaryList.wa_dataf_158.content;
                salaryItem.dataf_159 = rootobject.salaryList.wa_dataf_159.content;
                if (rootobject.salaryList.wa_dataf_5 != null)
                {
                    salaryItem.dataf_5 = rootobject.salaryList.wa_dataf_5.content;
                }
                else
                {
                    salaryItem.dataf_5 = "0";
                }

                salaryItem.dataf_3 = rootobject.salaryList.wa_dataf_3.content;
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                sumDecdataf_3 += decimal.Parse(salaryItem.dataf_3);
#pragma warning restore CS8604 // 引用类型参数可能为 null。

                salaryItem.dataf_157 = rootobject.salaryList.wa_dataf_157.content;
                salaryItem.dataf_162 = rootobject.salaryList.wa_dataf_162.content;

#pragma warning restore CS8602 // 解引用可能出现空引用。

#pragma warning disable CS8604 // 引用类型参数可能为 null。
                var totalDeduction = -decimal.Parse(salaryItem.dataf_96) + decimal.Parse(salaryItem.dataf_63) + decimal.Parse(salaryItem.dataf_158) + decimal.Parse(salaryItem.dataf_5);
                salaryItem.dataf_163 = totalDeduction.ToString();
                sumDecdataf_163 += decimal.Parse(salaryItem.dataf_163);
#pragma warning restore CS8604 // 引用类型参数可能为 null。
                salaryItems.Add(salaryItem);
            }
            return salaryItems;
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
