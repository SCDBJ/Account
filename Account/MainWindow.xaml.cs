using Account.Common;
using Account.Models.Consump.Request;
using Account.Models.SalaryDetail;
using Account.Views;

using HandyControl.Controls;
using HandyControl.Data;

using System.Net.Http;
using System.Net.Http.Json;
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

namespace Account
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: System.Windows.Window
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private string consumpAutoAccount = "/api/consumprecord-autoaccount";
        private string salaryrecordAdd = "/api/salaryrecord-autoaccount";
        public MainWindow()
        {
            InitializeComponent();
            // 初始化时，默认跳转到第一个页面
            MainFrame.Navigate(new Views.ExpendPage());
        }
        private void SideMenu_SelectionChanged(object sender, HandyControl.Data.FunctionEventArgs<object> e)
        {
            // 1. 健壮性检查：确保窗口初始化未完成时（MainFrame还不存在时）不崩溃
            if (MainFrame == null || MainMenu == null)
                return;
            if (e.Info is SideMenuItem selectedItem)
            {
                // 3. 方式 B：通过 Tag（标签）判断（比用 Header 字符串判断更稳妥，不易受多语言/改名影响）
                string? menuTag = selectedItem.CommandParameter?.ToString();

                switch (menuTag)
                {
                    case "Expend":
                        MainFrame.Navigate(new ExpendPage());
                        break;
                    case "Income":
                        MainFrame.Navigate(new IncomePage());
                        break;
                    case "IncomeCategory":
                        MainFrame.Navigate(new IncomeCategory());
                        break;
                    case "SalaryDetail":
                        MainFrame.Navigate(new SalaryDetail());
                        break;
                }
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(App.host + consumpAutoAccount, null);
                if (response.IsSuccessStatusCode)
                {

                }
                else
                {

                }
            }
            catch (HttpRequestException ex)
            {

            }
            List<SalaryItem>? salaryList=GetSalary();
            if (salaryList != null)
            {
                foreach (var item in salaryList)
                {
                    HttpResponseMessage response = await _httpClient.PostAsJsonAsync(App.host + salaryrecordAdd, item);
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

            List<SalaryItem> salaryItems = new List<SalaryItem>();
            int lastdate = int.Parse(DateTime.Now.AddMonths(-1).Year.ToString() + DateTime.Now.AddMonths(-1).Month.ToString().PadLeft(2, '0'));
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
                var fdate = int.Parse(salaryItem.datacyear + salaryItem.datacperiod ?? "0".PadLeft(2, '0'));
                if (lastdate!= fdate)
                    continue;

                salaryItem.dataf_32 = rootobject.salaryList?.wa_dataf_32?.content;

                salaryItem.dataf_131 = rootobject.salaryList?.wa_dataf_131?.content;
                salaryItem.dataf_134 = rootobject.salaryList?.wa_dataf_134?.content;
                salaryItem.dataf_40 = rootobject.salaryList?.wa_dataf_40?.content;
                salaryItem.dataf_95 = rootobject.salaryList?.wa_dataf_95?.content;

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

                salaryItem.dataf_157 = rootobject.salaryList?.wa_dataf_157?.content;
                salaryItem.dataf_162 = rootobject.salaryList?.wa_dataf_162?.content;

                var totalDeduction = -salaryItem.dataf_96 + decimal.Parse(salaryItem.dataf_63 ?? "0") + decimal.Parse(salaryItem.dataf_158 ?? "0") + decimal.Parse(salaryItem.dataf_5 ?? "0");
                salaryItem.dataf_163 = totalDeduction.ToString();
                salaryItems.Add(salaryItem);
            }
            return salaryItems;
        }
    }
}