using System;
using System.Collections.Generic;
using System.Text;

namespace Account.Models.Income.Request
{
    public class IncomerecordRequest
    {
        public string? startTime
        {
            get; set;
        }
        public string? endTime
        {
            get; set;
        }
    }
}
