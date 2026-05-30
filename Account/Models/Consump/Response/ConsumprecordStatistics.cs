using System;
using System.Collections.Generic;
using System.Text;

namespace Account.Models.Consump.Response
{
    public class ConsumprecordStatistics
    {
        public int? consumpYear
        {
            get;set;
        }
        public int? consumpMonth
        {
            get; set;
        }
        public string? categoryName
        {
            get; set;
        }
        public decimal? consumpAmount
        {
            get; set;
        }
    }
}
