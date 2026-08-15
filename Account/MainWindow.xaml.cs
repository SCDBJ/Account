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
    public partial class MainWindow : System.Windows.Window
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private string consumpAutoAccount = "/api/consumprecord-autoaccount";
        private string salaryrecordAdd = "/api/salaryrecord-add";
        // 1. 声明成员变量，并直接将默认值指向你 XAML 中初始选中的项 (ItemExpendDetail)
        private SideMenuItem? _currentSelectedItem;
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
                // 如果是鼠标点击触发的，也同步更新 _currentSelectedItem 的记录
                _currentSelectedItem = selectedItem;

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
            HttpResponseMessage responses = await _httpClient.PostAsync(App.host + salaryrecordAdd, null);
            if (!responses.IsSuccessStatusCode)
            {
                // 专门读取服务器返回的错误文本
                string errorDetails = await responses.Content.ReadAsStringAsync();
                var statusCode = responses.StatusCode;
                // 可以在这里根据 errorDetails 进一步调试
                //Growl.Error("数据获取失败！StatusCode：" + statusCode + "，ErrorDetails：" + errorDetails);

                return;
            }

            string responseJson = await responses.Content.ReadAsStringAsync();
            if (responseJson != null && responseJson.Contains("保存成功"))
            {

            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                SideMenuItem? targetItem = null;

                switch (e.Key)
                {
                    case Key.D1:
                    case Key.NumPad1:
                        targetItem = ItemExpendDetail;
                        break;
                    case Key.D2:
                    case Key.NumPad2:
                        targetItem = ItemIncomeDetail;
                        break;
                    case Key.D3:
                    case Key.NumPad3:
                        targetItem = ItemIncomeCategory;
                        break;
                    case Key.D4:
                    case Key.NumPad4:
                        targetItem = ItemSalaryDetail;
                        break;
                }

                // 如果按下的快捷键对应的项与当前选中项不同
                if (targetItem != null && targetItem != _currentSelectedItem)
                {
                    // A. 取消上一个项的选中（恢复正常字体，解决“多项同时粗体”的问题）
                    if (_currentSelectedItem != null)
                    {
                        _currentSelectedItem.IsSelected = false;
                    }

                    // B. 选中当前新项（变粗体）
                    targetItem.IsSelected = true;

                    // C. 更新记录
                    _currentSelectedItem = targetItem;

                    // D. 手动触发你的 SelectionChanged 跳转逻辑
                    var args = new FunctionEventArgs<object>(targetItem);
                    SideMenu_SelectionChanged(MainMenu, args);

                    e.Handled = true;
                }
            }
        }
    }
}