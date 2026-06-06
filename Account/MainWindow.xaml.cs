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
        }
    }
}