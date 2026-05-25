using System;
using System.Collections.Generic;
using System.Text;

namespace Account.Models.Consump.Request
{
    public class ConsumprecordAddRequest
    {
        public int categoryId
        {
            get;set;
        }
        public decimal? consumpAmount
        {
            get; set;
        }
        public string? consumpNote
        {
            get; set;
        }
        public DateTime consumpTime
        {
            get; set;
        }
    }
}
