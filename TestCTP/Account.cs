using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCTP
{
    public class Account
    {
        #region 属性
        /// <summary>
        /// 基金产品ID
        /// </summary>
        public string FundProductID { get; set; }
        /// <summary>
        /// 账户ID
        /// </summary>
        public string AccountID { get; set; }
        /// <summary>
        /// 经纪商ID
        /// </summary>
        public string BrokerID { get; set; }
        /// <summary>
        /// 账户密码
        /// </summary>
        public string AccountPW { get; set; }
        /// <summary>
        /// 账户利息基数
        /// </summary>
        public double AccountInterestBase { get; set; }
        /// <summary>
        /// 账户利息
        /// </summary>
        public double AccountInterest { get; set; }
        /// <summary>
        /// 账户入金
        /// </summary>
        public double AccountDeposit { get; set; }
        /// <summary>
        /// 账户出金
        /// </summary>
        public double AccountWithdraw { get; set; }
        /// <summary>
        /// 账户初始权益
        /// </summary>
        public double AccountInitialCapital { get; set; }
        /// <summary>
        /// 账户当前权益
        /// </summary>
        public double AccountCurrentCapital { get; set; }
        /// <summary>
        /// 账户手续费
        /// </summary>
        public double AccountRetCommssion { get; set; }
        /// <summary>
        /// 冻结的保证金
        /// </summary>
        public double FrozenMargin { get; set; }
        /// <summary>
        /// 冻结的资金
        /// </summary>
        public double FrozenCash { get; set; }
        /// <summary>
        /// 冻结的手续费
        /// </summary>
        public double FrozenCommission { get; set; }
        /// <summary>
        /// 当前保证金总额
        /// </summary>
        public double CurrMargin { get; set; }
        /// <summary>
        /// 当前保证金占用率
        /// </summary>
        public string CurrMarginRate { get; set; }
        /// <summary>
        /// 手续费
        /// </summary>
        public double Commission { get; set; }
        /// <summary>
        /// 平仓盈亏
        /// </summary>
        public double CloseProfit { get; set; }
        /// <summary>
        /// 持仓盈亏
        /// </summary>
        public double PositionProfit { get; set; }
        /// <summary>
        /// 用户权益
        /// </summary>
        public double Balance { get; set; }
        /// <summary>
        /// 可用资金
        /// </summary>
        public double Available { get; set; }
        /// <summary>
        /// 可取资金
        /// </summary>
        public double WithdrawQuota { get; set; }
        /// <summary>
        /// 基本准备金
        /// </summary>
        public double Reserve { get; set; }

        /// <summary>
        /// 轧差
        /// 
        /// </summary>
        private double _netting;
        public double Netting
        {
            get { return _netting; }
            set { }
        }

        #endregion
    }
}
