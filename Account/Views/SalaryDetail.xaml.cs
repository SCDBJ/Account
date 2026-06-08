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
            BindingYear();
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
            IList<string>? list = FileIOHelper.FindDirectory(path);
            if (list == null)
            {
                return null;
            }

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
                salaryItem.datacyear = rootobject.salaryList?.wa_datacyear?.content;
                salaryItem.datacperiod = rootobject.salaryList?.wa_datacperiod?.content;
                salaryItem.dataf_32 = rootobject.salaryList?.wa_dataf_32?.content;

                sumDecdataf_32 += decimal.Parse(salaryItem.dataf_32??"0");

                salaryItem.dataf_131 = rootobject.salaryList?.wa_dataf_131?.content;
                salaryItem.dataf_134 = rootobject.salaryList?.wa_dataf_134?.content;
                salaryItem.dataf_40 = rootobject.salaryList?.wa_dataf_40?.content;
                salaryItem.dataf_95 = rootobject.salaryList?.wa_dataf_95?.content;

                decimal? dataf_94 = 0.00M;
                var fdate = int.Parse(salaryItem.datacyear + salaryItem.datacperiod??"0".PadLeft(2,'0'));
                if (fdate <= 202110)
                {
                    dataf_94 = 13000M*0.15M;
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

                salaryItem.dataf_96 = decimal.Parse(salaryItem.dataf_95 ?? "0") - salaryItem.dataf_94;

                salaryItem.dataf_97 = (salaryItem.dataf_96 / salaryItem.dataf_94 * 100)?.ToString("F2") + "%";

                if (rootobject.salaryList?.wa_dataf_63 != null)
                {
                    salaryItem.dataf_63 = rootobject.salaryList?.wa_dataf_63?.content;
                }
                else
                {
                    salaryItem.dataf_63 = "0";
                }

                salaryItem.dataf_79 = rootobject.salaryList?.wa_dataf_79?.content;
                salaryItem.dataf_158 = rootobject.salaryList?.wa_dataf_158?.content;
                salaryItem.dataf_159 = rootobject.salaryList?.wa_dataf_159?.content;
                if (rootobject.salaryList?.wa_dataf_5 != null)
                {
                    salaryItem.dataf_5 = rootobject.salaryList?.wa_dataf_5?.content;
                }
                else
                {
                    salaryItem.dataf_5 = "0";
                }

                salaryItem.dataf_3 = rootobject.salaryList?.wa_dataf_3?.content;
                sumDecdataf_3 += decimal.Parse(salaryItem.dataf_3 ?? "0");

                salaryItem.dataf_157 = rootobject.salaryList?.wa_dataf_157?.content;
                salaryItem.dataf_162 = rootobject.salaryList?.wa_dataf_162?.content;

                var totalDeduction = -salaryItem.dataf_96 + decimal.Parse(salaryItem.dataf_63 ?? "0") + decimal.Parse(salaryItem.dataf_158 ?? "0") + decimal.Parse(salaryItem.dataf_5 ?? "0");
                salaryItem.dataf_163 = totalDeduction.ToString();
                sumDecdataf_163 += decimal.Parse(salaryItem.dataf_163 ?? "0");
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
            int statisticsYear = Convert.ToInt16(cboxStatisticsYear.SelectedValue);

        }
        private void BindingYear()
        {
            cboxStatisticsYear.Items.Clear();
            for (int i = DateTime.Now.Year; i >= 2014; i--)
            {
                cboxStatisticsYear.Items.Add(i.ToString());
            }
            cboxStatisticsYear.SelectedIndex = 0;
        }
    }
}
