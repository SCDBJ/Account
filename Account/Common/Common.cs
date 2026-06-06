using System;
using System.Collections.Generic;
using System.Text;

namespace Account.Common
{
    public class Common
    {
        /// <summary>
        /// 两个日期之间相差的月数
        /// </summary>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <returns></returns>
        public static int DiffMonth(int dateEndYear, int dateEndMonth, int dateStartYear, int dateStartMonth)
        {
            int diffmonth = (dateEndYear - dateStartYear) * 12 + dateEndMonth - dateStartMonth;
            return diffmonth;
        }
    }
}
