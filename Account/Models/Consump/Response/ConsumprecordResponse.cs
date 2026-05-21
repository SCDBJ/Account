using System;
using System.Collections.Generic;
using System.Text;

namespace Account.Models.Consump.Response
{
    public class ConsumprecordResponse
    {
        public int consumpId
        {
            get;set;
        }
        public string consumpType
        {
            get; set;
        }
        public decimal consumpAmount
        {
            get; set;
        }
        public DateTime consumpTime
        {
            get; set;
        }
        public DateTime createTime
        {
            get; set;
        }
        public string consumpNote
        {
            get; set;
        }
    }
}
