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

namespace Account
{
    public class ApiService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static string categoryItems = "/api/category-items";
        // 模拟一个异步请求接口，延迟 1 秒后返回数据
        public static async Task<List<CategoryResponse>?> GetCategoryTypesAsync()
        {
            await Task.Delay(500); // 模拟网络延迟
            CategoryRequest categoryRequest = new CategoryRequest() { categoryType = "ALL" };
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(App.host + categoryItems, categoryRequest);
            if (!response.IsSuccessStatusCode)
            {
                // 专门读取服务器返回的错误文本
                string errorDetails = await response.Content.ReadAsStringAsync();
                var statusCode = response.StatusCode;
                // 可以在这里根据 errorDetails 进一步调试
                Growl.Error("数据获取失败！StatusCode：" + statusCode + "，ErrorDetails：" + errorDetails);
                return null;
            }
            string responseJson = await response.Content.ReadAsStringAsync();
            if (responseJson != null)
            {
                ObservableCollection<CategoryResponse>? CategoryResponses = JsonSerializer.Deserialize<ObservableCollection<CategoryResponse>>(responseJson);
                if (CategoryResponses != null)
                {
                    return new List<CategoryResponse>(CategoryResponses.Select(x => new CategoryResponse { categoryId = x.categoryId, categoryType = x.categoryType,categoryName=x.categoryName }));
                }
            }
            return null;
        }
    }
}
