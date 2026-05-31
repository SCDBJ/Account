using System;
using System.Collections.Generic;
using System.Text;

namespace Account.Models.Income.Request
{
    public class IncomerecordAddRequest
    {
        public int categoryId
        {
            get; set;
        }
        public decimal? incomeAmount
        {
            get; set;
        }
        public string? incomeNote
        {
            get; set;
        }
        public DateTime incomeTime
        {
            get; set;
        }
    }
}
