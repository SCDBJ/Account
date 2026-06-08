using System;
using System.Collections.Generic;
using System.Text;

namespace Account.Models.SalaryDetail
{
    public class SalaryItem
    {
        /// <summary>
        /// 年度
        /// </summary>
        public string? datacyear
        {
            get; set;
        }
        /// <summary>
        /// 期间
        /// </summary>
        public string? datacperiod
        {
            get; set;
        }
        /// <summary>
        /// 核定工资总额
        /// </summary>
        public string? dataf_32
        {
            get; set;
        }
        /// <summary>
        /// 核算满勤天数
        /// </summary>
        public string? dataf_131
        {
            get; set;
        }
        /// <summary>
        /// 核算出勤天数
        /// </summary>
        public string? dataf_134
        {
            get; set;
        }
        /// <summary>
        /// 基本工资
        /// </summary>
        public string? dataf_40
        {
            get; set;
        }
        /// <summary>
        /// 核定绩效奖金
        /// </summary>
        public decimal? dataf_94
        {
            get; set;
        }
        /// <summary>
        /// 绩效奖金
        /// </summary>
        public string? dataf_95
        {
            get; set;
        }
        /// <summary>
        /// 绩效奖金差异
        /// </summary>
        public decimal? dataf_96
        {
            get; set;
        }
        /// <summary>
        /// 绩效奖金差异百分比
        /// </summary>
        public string? dataf_97
        {
            get; set;
        }
        /// <summary>
        /// 税前扣款合计
        /// </summary>
        public string? dataf_63
        {
            get; set;
        }
        /// <summary>
        /// 应付工资
        /// </summary>
        public string? dataf_79
        {
            get; set;
        }
        /// <summary>
        /// 社保个人合计
        /// </summary>
        public string? dataf_158
        {
            get; set;
        }
        /// <summary>
        /// 公积金个人
        /// </summary>
        public string? dataf_159
        {
            get; set;
        }
        /// <summary>
        /// 本次扣税
        /// </summary>
        public string? dataf_5
        {
            get; set;
        }
        /// <summary>
        /// 实发合计
        /// </summary>
        public string? dataf_3
        {
            get; set;
        }
        /// <summary>
        /// 社保单位合计
        /// </summary>
        public string? dataf_157
        {
            get; set;
        }
        /// <summary>
        /// 公积金单位
        /// </summary>
        public string? dataf_162
        {
            get; set;
        }
        /// <summary>
        /// 扣减合计(奖金扣减+扣税合计+社保)
        /// </summary>
        public string? dataf_163
        {
            get; set;
        }
    }
}
