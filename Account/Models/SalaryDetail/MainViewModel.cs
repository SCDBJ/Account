using Account.Models.Income.Response;

using HandyControl.Controls;

using Newtonsoft.Json.Linq;

using RestSharp;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;

namespace Account.Models.SalaryDetail
{
    public class MainViewModel
    {
        // DataGrid 最终绑定的数据源
        public ObservableCollection<DisplayItem> GridData { get; set; } = new ObservableCollection<DisplayItem>();
        public ObservableCollection<DisplayItem> GridBaseData { get; set; } = new ObservableCollection<DisplayItem>();
        public void LoadData(List<RawDataObject> rawList)
        {
            GridData.Clear();

            foreach (var raw in rawList)
            {
                GridData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "绩效扣款", Amount = raw.dataf_96 });
                GridData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "税前扣款", Amount = raw.dataf_63 });
                GridData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "社保", Amount = raw.dataf_158 });
                GridData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "个税", Amount = raw.dataf_5 });
            }
        }
        public void LoadBaseData(List<RawBaseDataObject> rawList)
        {
            GridBaseData.Clear();

            foreach (var raw in rawList)
            {
                GridBaseData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "核定工资", Amount = raw.dataf_32 });
                GridBaseData.Add(new DisplayItem { DataCYear = raw.datacyear, AmountType = "公积金单位", Amount = raw.dataf_162 });
            }
            if (rawList.Count > 0)
            {
                List<IncomerecordResponse> incomerecordList = RequestBonus(rawList[0].datacyear);
                if (incomerecordList.Count > 0)
                {
                    GridBaseData.Add(new DisplayItem { DataCYear = rawList[0].datacyear, AmountType = "年终奖", Amount = incomerecordList[0].incomeAmount });
                }
                else
                {
                    GridBaseData.Add(new DisplayItem { DataCYear = rawList[0].datacyear, AmountType = "年终奖", Amount = 0 });
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private List<IncomerecordResponse> RequestBonus(string datacyear)
        {
            string incomerecordItems = "/api/incomerecord-items";

            var endDateStr = DateTime.Today.ToString("yyyy-MM-dd");
            string jsonRequestBody = "{\"startTime\": \"2014-01-01\", \"endTime\": \""+ endDateStr + "\"}";

            // 1. 创建客户端与请求对象
            var client = new RestClient();
            var request = new RestRequest(App.host+ incomerecordItems, Method.Post);

            // 2. 添加 Header 和 JSON 请求体
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(jsonRequestBody);

            try
            {
                // 3. 纯同步执行请求（没有 Async，也没有过时警告）
                RestResponse response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    if (response.Content != null)
                    {
                        List<IncomerecordResponse>? incomerecordResponse = JsonSerializer.Deserialize<List<IncomerecordResponse>>(response.Content);
                        if (incomerecordResponse != null)
                        {
                            List<IncomerecordResponse> incomerecordList=incomerecordResponse.Where(o => o.incomeYear.ToString() == datacyear&o.categoryName=="年终奖").ToList();
                            return incomerecordList;
                        }
                    }
                }
                else
                {
                    Growl.Error("数据获取失败！StatusCode：" + response.StatusCode + "，ErrorDetails：" + response.Content);
                }
            }
            catch (Exception ex)
            {
                Growl.Error($"发生异常: {ex.Message}");
            }
            return null;
        }
    }

    public class RawDataObject
    {
        public string? datacyear
        {
            get; set;
        }
        public decimal? dataf_96
        {
            get; set;
        }
        public decimal? dataf_63
        {
            get; set;
        }
        public decimal? dataf_158
        {
            get; set;
        }
        public decimal? dataf_5
        {
            get; set;
        }
    }
    public class RawBaseDataObject
    {
        public string? datacyear
        {
            get; set;
        }
        public decimal? dataf_32
        {
            get; set;
        }
        public decimal? dataf_162
        {
            get; set;
        }
    }
}
