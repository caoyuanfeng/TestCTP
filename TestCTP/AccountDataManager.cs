using CTPTradeApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TestCTP
{
    /// <summary>
    /// 交易账户级别的数据维护
    /// </summary>
    public class AccountDataManager
    {
        #region 变量

        private readonly AutoResetEvent _loginTradeEvent = new AutoResetEvent(false);
        private AutoResetEvent _settlementInfoEvent = new AutoResetEvent(false);
        private AutoResetEvent _exitEvent = new AutoResetEvent(false);
        private AutoResetEvent _exchangeLoadFinishedFlag = new AutoResetEvent(false);
        private AutoResetEvent _contractLoadFinishedFlag = new AutoResetEvent(false);
        private AutoResetEvent _positionLoadFinishedFlag = new AutoResetEvent(false);
        private AutoResetEvent _contractUpdateFinishedFlag = new AutoResetEvent(false);
        private AutoResetEvent _accountLoadFinishedFlag = new AutoResetEvent(false);
        private AutoResetEvent _instrumentLoadFinishedFlag = new AutoResetEvent(false);

        private readonly object lockRevocation = new object();
        private readonly object _lockAccount = new object();
        private Account _currentAccount;
        private Broker _broker;
        private TradeApi _trader;
        public string Today = DateTime.Today.ToString("yyyy-MM-dd");
        private System.Timers.Timer timer;
        private static readonly object lockFrontConnected = new object();
        private static readonly object lockFrontDisConnected = new object();
        private static readonly object lockLogined = new object();
        private string _AuthCode = "03TZUHWOZ41VEK8N";
        private string _AppID =  "client_ljtzctp_2.0";

        //private string _AuthCode = "0000000000000000";
        //private string _AppID = "simnow_client_test";
        #endregion

        #region 单一性[账户唯一]

        public AccountDataManager(Account account,Broker broker, bool isLogin)
        {
            _currentAccount = account;
            _broker = broker;

            _DataManager(isLogin);
        }

        private void _DataManager(bool isLogin)
        {
            _trader = CTPHelper.GetInstance().GetCTPTraderInstance(_currentAccount,_broker);
            #region
            _morning0900 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 0, 0);
            _morning0915 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 15, 0);
            _morning1015 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 10, 15, 0);
            _morning1030 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 10, 30, 0);
            _morning1130 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 11, 30, 0);
            _non1300 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 0, 0);
            _non1330 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 0);
            _non1500 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 0, 0);
            _non1515 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 15, 0);
            #endregion

            InitEvent();

            if (isLogin)
                InitData();
        }

        public TradeApi GetCtpTrader()
        {
            return _trader;
        }

        #endregion

        #region 初始化

        public void InitEvent()
        {
            _trader.OnRspQryInstrument += RspQryInstrument;
            _trader.OnRspQrySettlementInfo += RspQrySettlementInfo;
            _trader.OnRspSettlementInfoConfirm += RspSettlementInfoConfirm;

            _trader.OnRspQryTradingAccount += RspQryTradingAccount;
            _trader.OnRspQryInvestorPosition += RspQryInvestorPosition;
            _trader.OnRspQryInvestorPositionDetail += RspQryInvestorPositionDetail;
            _trader.OnRspQryTrade += RspQryTrade;
            _trader.OnRtnTrade += RtnTrade;

            _trader.OnRspUserLogin += RspUserLogin;
            _trader.OnRspUserLogout += RspUserLogout;
           // _trader.OnRspAuthenticate += RspAuthenticate;
            _trader.OnFrontConnect += RspFrontConnection;
            _trader.OnDisconnected += RspDisconnection;
            _trader.OnHeartBeatWarning += RspHeartBeatWarning;

            
        }



        public void InitData()
        {
            _trader.Connect();

            _loginTradeEvent.WaitOne(2000);

            QueryAccount();

           QueryInstrument();

            timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = 10000;
            timer.Elapsed += timer_Elapsed;
           // timer.Start();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this._trader != null && this._isTraderFrontConnected && this._isTraderLoginSucceed)
                QueryAccount();
        }

        public void LoadData()
        {
            LoadPostion();
        }

        #endregion

        #region CTPManager

        #region CTP查询

        private bool _isTraderLoginSucceed;
        private bool _isTraderFrontConnected;

        
        /// <summary>
        /// 查询结算单信息
        /// </summary>
        public void QuerySettlementInfo()
        {
            _trader.QuerySettlementInfo(-9);
            _settlementInfoEvent.WaitOne(1000);

            Thread.Sleep(1000);
        }
        
        /// <summary>
        /// 查询头寸信息
        /// </summary>
        public void QueryPosition()
        {
            Thread.Sleep(1200);
            _trader.QueryInvestorPosition(1);

            // 等待5分钟,超时则重新登陆
            if (!_positionLoadFinishedFlag.WaitOne(5000))
            {
                return;
            }

            //_UpdatePositionToDb();
        }

        /// <summary>
        /// 查询账户信息
        /// </summary>
        public void QueryAccount()
        {
            _trader.QueryTradingAccount(1);

            _accountLoadFinishedFlag.WaitOne(2000);
        }


        public void QueryInstrument()
        {
            _trader.QueryInstrument(-11);
            _instrumentLoadFinishedFlag.WaitOne(2000);
        }

        #endregion

        #region CTP反馈

        /// <summary>
        /// 登录反馈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void RspUserLogin(ref CThostFtdcRspUserLoginField pRspUserLogin,
            ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast)
        {
            lock (lockLogined)
            {
                _trader.SettlementInfoConfirm(-4);

                _isTraderLoginSucceed = true;

                DateTime now = DateTime.Now;

                try
                {
                    _czceDiff = now - DateTime.Parse(pRspUserLogin.CZCETime);
                    _cffexDiff = now - DateTime.Parse(pRspUserLogin.FFEXTime);
                    _dceDiff = now - DateTime.Parse(pRspUserLogin.DCETime);
                    _shfeDiff = now - DateTime.Parse(pRspUserLogin.SHFETime);
                }
                catch (Exception)
                {

                }

                _loginTradeEvent.Set();
            }
        }

        /// <summary>
        /// 连接反馈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void RspFrontConnection()
        {
            lock (lockFrontConnected)
            {
                _isTraderFrontConnected = true;
                _trader.Authenticate(-5, _currentAccount.AccountID, "", _AuthCode, _AppID);

                //_trader.UserLogin(-3, _currentAccount.AccountID, _currentAccount.AccountPW);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRspAuthenticate"></param>
        /// <param name="pRspInfo"></param>
        /// <param name="nRequestID"></param>
        /// <param name="bIsLast"></param>
        void RspAuthenticate(ref CThostFtdcRspAuthenticateField pRspAuthenticate,
            ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast)
        {
            if (pRspInfo.ErrorID == 0)
            {
                _trader.UserLogin(-3, _currentAccount.AccountID, _currentAccount.AccountPW);
            }
            else
            {
                Console.WriteLine("Authenticate error: " + pRspInfo.ErrorMsg);
                throw new Exception("Authenticate error:" + pRspInfo.ErrorMsg);
            }
        }

        static int num;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInstrument"></param>
        /// <param name="pRspInfo"></param>
        /// <param name="nRequestID"></param>
        /// <param name="bIsLast"></param>
       public void RspQryInstrument(ref CThostFtdcInstrumentField pInstrument, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast)
        {
            //Console.WriteLine("回调处理, TradeID: {0}", pInstrument.InstrumentID);

            num++;

            if((int)bIsLast > 0)
            {
                MessageBox.Show(num.ToString());
                _instrumentLoadFinishedFlag.Set();

            }

        }


        /// <summary>
        /// 成交回调处理
        /// </summary>
        /// <param name="pTrade"></param>
        void RtnTrade(ref CThostFtdcTradeField pTrade)
        {
            Console.WriteLine("成交回调处理, TradeID: {0}", pTrade.TradeID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTrade"></param>
        /// <param name="pRspInfo"></param>
        /// <param name="nRequestID"></param>
        /// <param name="bIsLast"></param>
        public void RspQryTrade(ref CThostFtdcTradeField pTrade,
               ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast)
        {
            if (pRspInfo.ErrorID == 0)
            {
                Console.WriteLine("成交查询成功, TradeID: {0}", pTrade.TradeID);
            }
            else
            {
                Console.WriteLine(pRspInfo.ErrorMsg);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInvestorPositionDetail"></param>
        /// <param name="pRspInfo"></param>
        /// <param name="nRequestID"></param>
        /// <param name="bIsLast"></param>
        public void RspQryInvestorPositionDetail(ref CThostFtdcInvestorPositionDetailField pInvestorPositionDetail, ref CThostFtdcRspInfoField pRspInfo,
               int nRequestID, byte bIsLast)
        {
            if (pRspInfo.ErrorID == 0)
            {
                Console.WriteLine("投资者持仓明细查询成功, 合约代码：{0}", pInvestorPositionDetail.InstrumentID);
            }
            else
            {
                Console.WriteLine(pRspInfo.ErrorMsg);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInvestorPosition"></param>
        /// <param name="pRspInfo"></param>
        /// <param name="nRequestID"></param>
        /// <param name="bIsLast"></param>
        public void RspQryInvestorPosition(ref CThostFtdcInvestorPositionField pInvestorPosition, ref CThostFtdcRspInfoField pRspInfo,
             int nRequestID, byte bIsLast)
        {
            if (pRspInfo.ErrorID == 0)
            {
                Console.WriteLine("投资者持仓查询成功, 合约代码：{0}", pInvestorPosition.InstrumentID);
            }
            else
            {
                Console.WriteLine(pRspInfo.ErrorMsg);
            }
        }

        /// <summary>
        /// 结算反馈
        /// </summary>
        /// <param name="trader"></param>
        /// <param name="args"></param>
        void RspQrySettlementInfo(ref CThostFtdcSettlementInfoField pSettlementInfo, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast)
        {
            if (bIsLast > 0)
            {
                _trader.SettlementInfoConfirm(-5);
                _settlementInfoEvent.Set();
            }
        }

        /// <summary>
        /// 结算确认反馈
        /// </summary>
        /// <param name="trader"></param>
        /// <param name="args"></param>
        void RspSettlementInfoConfirm(ref CThostFtdcSettlementInfoConfirmField pSettlementInfoConfirm, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast)
        {
            if (bIsLast > 0)
            {
                _settlementInfoEvent.Set();
            }
        }


        void RspQryTradingAccount(ref CThostFtdcTradingAccountField pTradingAccount, ref CThostFtdcRspInfoField pRspInfo,
                int nRequestID, byte bIsLast)
        {
            lock (_lockAccount)
            {
                try
                {
                    //if (BaseData.GetInstance().AccountCollection.ContainsKey(new Account.AccountKey(pTradingAccount.BrokerID, _currentAccount.FundProductID, pTradingAccount.AccountID)))
                    {
                       // _currentAccount = BaseData.GetInstance().AccountCollection[new Account.AccountKey(pTradingAccount.BrokerID, _currentAccount.FundProductID, pTradingAccount.AccountID)];
                        _currentAccount.Available = Math.Round(pTradingAccount.Available);
                        _currentAccount.Balance = Math.Round(pTradingAccount.Balance);
                        _currentAccount.AccountWithdraw = Math.Round(pTradingAccount.CashIn);
                        _currentAccount.CloseProfit = Math.Round(pTradingAccount.CloseProfit);
                        _currentAccount.Commission = Math.Round(pTradingAccount.Commission);
                        _currentAccount.CurrMargin = Math.Round(pTradingAccount.CurrMargin);
                        _currentAccount.AccountDeposit = Math.Round(pTradingAccount.Deposit);
                        _currentAccount.FrozenCash = Math.Round(pTradingAccount.FrozenCash);
                        _currentAccount.FrozenCommission = Math.Round(pTradingAccount.FrozenCommission);
                        _currentAccount.FrozenMargin = Math.Round(pTradingAccount.FrozenMargin);
                        _currentAccount.AccountInterest = Math.Round(pTradingAccount.Interest);;
                        _currentAccount.PositionProfit = Math.Round(pTradingAccount.PositionProfit);
                        _currentAccount.Reserve = Math.Round(pTradingAccount.Reserve);
                        _currentAccount.WithdrawQuota = Math.Round(pTradingAccount.WithdrawQuota);

                       // DataBaseHelper.GetInstance().UpdateAccount(_currentAccount);
                    }
                }
                catch (Exception ee)
                {
                   // LogHelper.GetInstance().ExceptionLogger.Info("[QueryTradingAccount]" + ee.StackTrace);
                }
            }

            _accountLoadFinishedFlag.Set();
        }

        /// <summary>
        /// 登出反馈
        /// </summary>
        /// <param name="_trader"></param>
        /// <param name="args"></param>
        void RspUserLogout(ref CThostFtdcUserLogoutField pUserLogout,
            ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast)
        {
            _isTraderLoginSucceed = false;
            _trader.Disconnect();
            _exitEvent.Set();
        }

       

        private Thread _currenThread = null;
        /// <summary>
        /// 断线反馈
        /// </summary>
        /// <param name="_trader"></param>
        /// <param name="args"></param>
        void RspDisconnection(int nReasion)
        {
            lock (lockFrontDisConnected)
            {
                _isTraderFrontConnected = false;
                //LogHelper.GetInstance().SessionLogger.Info(_currentAccount.AccountID + " trader_FrontDisconnected");

                if (_currenThread != null && _currenThread.IsAlive)
                    _currenThread.Abort();

                _currenThread = new Thread(new ThreadStart(DoSound));
                _currenThread.IsBackground = true;
                _currenThread.Start();
            }
        }

        private void DoSound()
        {
            int count = 0;
            while (count < 10 && (_isTraderFrontConnected == false || _isTraderLoginSucceed == false))
            {
                //Utilities.SoundPlayer.SoundDisConnected();
                Thread.Sleep(3000);
                count++;
            }
        }

        /// <summary>
        /// 心跳
        /// </summary>
        /// TimeLapes:
        //     超时时间
        /// 
        void RspHeartBeatWarning(int TimeLapes)
        {
            lock (lockRevocation)
            {
                if (TimeLapes > 1000)
                {
                    _settlementInfoEvent.Set();
                }
            }
        }


        private readonly object _lockObj = new object();

        #endregion

        #endregion

        #region RealPositionManager

        private void LoadPostion()
        {
            //从CTP查询持仓数据
            QueryPosition();
        }

       
        #endregion

        #region 方法

        public void Exit()
        {
           // DataBaseHelper.GetInstance().UpdateAccount(_currentAccount);

            if (timer.Enabled)
            {
                timer.Enabled = false;
                timer.Stop();
                timer.Close();
            }
        }

        

        private TimeSpan _czceDiff;
        private TimeSpan _cffexDiff;
        private TimeSpan _dceDiff;
        private TimeSpan _shfeDiff;
        private DateTime _morning0900, _morning0915, _morning1015, _morning1030, _morning1130, _non1330, _non1300, _non1500, _non1515;



        #endregion

    }
}
