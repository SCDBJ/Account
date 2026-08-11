using System;
using System.Collections.Generic;
using System.Text;

namespace Account.Models.SalaryDetail
{
    public class SalaryItem
    {
        public int salaryid
        {
            get; set;
        }
        /// <summary>
        /// 年度
        /// </summary>
        public int datacyear
        {
            get; set;
        }
        /// <summary>
        /// 期间
        /// </summary>
        public int datacperiod
        {
            get; set;
        }
        private decimal? _dataf_32;

        /// <summary>
        /// 核定工资总额（自动四舍五入保留整数）
        /// </summary>
        public decimal? dataf_32
        {
            get => _dataf_32;
            set => _dataf_32 = value.HasValue ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero) : null;
        }
        /// <summary>
        /// 核算满勤天数
        /// </summary>
        public double? dataf_131
        {
            get; set;
        }
        /// <summary>
        /// 核算出勤天数
        /// </summary>
        public double? dataf_134
        {
            get; set;
        }
        private decimal? _dataf_40;
        /// <summary>
        /// 基本工资
        /// </summary>
        public decimal? dataf_40
        {
            get => _dataf_40;
            set => _dataf_40 = value.HasValue ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero) : null;
        }
        private decimal? _dataf_94;
        /// <summary>
        /// 核定绩效奖金
        /// </summary>
        public decimal? dataf_94
        {
            get => _dataf_94;
            set => _dataf_94 = value.HasValue ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero) : null;
        }
        private decimal? _dataf_95;
        /// <summary>
        /// 绩效奖金
        /// </summary>
        public decimal? dataf_95
        {
            get => _dataf_95;
            set => _dataf_95 = value.HasValue ? Math.Round(value.Value, 2, MidpointRounding.AwayFromZero) : null;
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
        public decimal? dataf_63
        {
            get; set;
        }
        private decimal? _dataf_79;
        /// <summary>
        /// 应付工资
        /// </summary>
        public decimal? dataf_79
        {
            get => _dataf_79;
            set => _dataf_79 = value.HasValue ? Math.Round(value.Value, 2, MidpointRounding.AwayFromZero) : null;
        }
        /// <summary>
        /// 社保个人合计
        /// </summary>
        public decimal? dataf_158
        {
            get; set;
        }
        private decimal? _dataf_159;
        /// <summary>
        /// 公积金个人
        /// </summary>
        public decimal? dataf_159
        {
            get => _dataf_159;
            set => _dataf_159 = value.HasValue ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero) : null;
        }
        /// <summary>
        /// 本次扣税
        /// </summary>
        public decimal? dataf_5
        {
            get; set;
        }
        /// <summary>
        /// 实发合计
        /// </summary>
        public decimal? dataf_3
        {
            get; set;
        }
        /// <summary>
        /// 社保单位合计
        /// </summary>
        public decimal? dataf_157
        {
            get; set;
        }
        private decimal? _dataf_162;
        /// <summary>
        /// 公积金单位
        /// </summary>
        public decimal? dataf_162
        {
            get => _dataf_162;
            set => _dataf_162 = value.HasValue ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero) : null;
        }
        /// <summary>
        /// 扣减合计(奖金扣减+扣税合计+社保)
        /// </summary>
        public decimal? dataf_163
        {
            get; set;
        }
    }
}
