using Account.Models.Consump.Request;
using Account.Models.Consump.Response;

using HandyControl.Controls;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using hc = HandyControl.Controls;

namespace Account.Views
{
    /// <summary>
    /// ExpendAdd.xaml 的交互逻辑
    /// </summary>
    public partial class ExpendAdd : System.Windows.Window
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private string categoryItems = "/api/category-items";
        public ConsumprecordResponse ConsumprecordData
        {
            get; private set;
        }
        // 1. 定义一个可以被通知的动态集合（必须是属性，即带 get; set;）
        public ObservableCollection<CategoryResponse>? CategoryTypes
        {
            get; set;
        }
        public ExpendAdd()
        {
            InitializeComponent();
            CategoryTypes = new ObservableCollection<CategoryResponse>();

            ConsumprecordData = new ConsumprecordResponse() { consumpTime = DateTime.Now };
            this.DataContext = this;
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // 这里可以加入数据校验，比如判断姓名是否为空
            if (ConsumprecordData.categoryId == 0)
            {
                hc.MessageBox.Show("支出类型不能为空！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.DialogResult = true; // 设置为 true 会自动关闭窗口，并代表用户确认保存
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // 关闭窗口，代表取消
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<CategoryResponse>? result = await ApiService.GetCategoryTypesAsync();

            // 5. 将请求到的数据填充到绑定的集合中
            foreach (var consump in result.Where(t=>t.categoryType.Equals("支出")))
            {
                CategoryTypes.Add(consump);
            }
            // 3. 核心步骤：判断接口是否返回了数据
            if (CategoryTypes.Count > 0)
            {
                // 获取第一项的 ID
                int firstId = CategoryTypes[0].categoryId;

                // 4. 使用 Dispatcher 异步把赋值操作推进 UI 队列的末尾，
                // 确保 ComboBox 的 Items 已经完全加载渲染完毕后，再进行“选中”操作。
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    ConsumprecordData.categoryId = firstId;
                }));
            }
        }
    }
}
