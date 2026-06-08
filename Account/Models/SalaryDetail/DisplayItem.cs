using System;
using System.Collections.Generic;
using System.Text;

namespace Account.Models.SalaryDetail
{
    public class DisplayItem
    {
        public string? DataCYear
        {
            get; set;
        } // 年份
        public string? AmountType
        {
            get; set;
        } // 类型（例如：dataf_95, dataf_96 等）
        public decimal? Amount
        {
            get; set;
        }    // 金额
    }
}
