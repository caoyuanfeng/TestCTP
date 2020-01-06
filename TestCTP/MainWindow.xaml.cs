using CTPTradeApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestCTP
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 交易接口实例
        /// </summary>
        private TradeApi _api;

        /// <summary>
        /// 交易服务器地址
        /// 180.168.146.187:10000
        /// 180.168.146.187:10001
        /// 218.202.237.33:10002
        /// </summary>
        private string _frontAddr = "tcp://180.168.146.187:10101";

        /// <summary>
        /// 经纪商代码
        /// </summary>
        private string _brokerID = "9999";

        /// <summary>
        /// 投资者账号
        /// </summary>
        private string _investorID = "127580";

        /// <summary>
        /// 密码
        /// </summary>
        private string _password = "231231qq";

        /// <summary>
        /// 是否连接
        /// </summary>
        private bool _isConnected;

        /// <summary>
        /// 是否登录
        /// </summary>
        private bool _isLogin;

        private Dictionary<string, AccountDataManager> _accountDataManagerInstanceDic;

        public MainWindow()
        {
            InitializeComponent();

            _accountDataManagerInstanceDic = new Dictionary<string, AccountDataManager>();

            List<Account> accounts = new List<Account>();
            //Account account1 = new Account();
            //account1.AccountID = "50720819";// "131294";
            //account1.AccountPW = "kong123456";// "326100";
            //account1.BrokerID = "9080";// "6666";
            //accounts.Add(account1);

            Account account2 = new Account();
            account2.BrokerID = "9999";
            account2.AccountPW = "231231qq";
            account2.AccountID = "127580";
            accounts.Add(account2);


            foreach (var account in accounts)
            {
                //根据账户的brokerid获取broker的信息
                Broker broker = new Broker();
               
                broker.BrokerID = account.BrokerID;
               // broker.TradeIP = "tcp://61.186.254.137:33433" ;//仿真系统
                broker.MarketIP = "";

                broker.TradeIP = "180.168.146.187:10101";// simnow

                AccountDataManager accountDataManager = new AccountDataManager(account,broker, true);
                _accountDataManagerInstanceDic.Add(account.AccountID, accountDataManager);
            }

            //Initialize();
            //TestGetApiVersion();

            //_api.OnRspQryInvestorPosition += new TradeApi.RspQryInvestorPosition(RspQryInvestorPosition);
            //_api.OnRspQryTradingAccount += RspQryTradingAccount;
            //_api.OnRspQryInvestorPositionDetail += RspQryInvestorPositionDetail;
            //_api.OnRspQryTrade += RspQryTrade;
            //_api.OnRtnTrade += RtnTrade;
            //_api.OnRspQryInstrument += RspQryInstrument;

           // TestQueryInstrument();
            //TestQueryInvestorPosition();
            //TestQueryTrade();
            //TestQueryInvestorPositionDetail();
            //TestQueryTradingAccount();
        }


        ////////////////////////////////////////////////////
        
        public void RspQryInstrument(ref CThostFtdcInstrumentField pInstrument, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast)
        {
            Console.WriteLine("查询成功, TradeID: {0}", pInstrument.InstrumentID);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTrade"></param>
        public void RtnTrade(ref CThostFtdcTradeField pTrade)
        {
            Console.WriteLine("成交查询成功, TradeID: {0}", pTrade.TradeID);
        }

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
        ///////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            _api = new TradeApi(_brokerID, _frontAddr);
            _api.OnRspError += new TradeApi.RspError((ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast) =>
            {
                Console.WriteLine("ErrorID: {0}, ErrorMsg: {1}", pRspInfo.ErrorID, pRspInfo.ErrorMsg);
            });

            _api.OnFrontConnect += new TradeApi.FrontConnect(() =>
            {
                _isConnected = true;
               _api.Authenticate(-5, _investorID, "", "0000000000000000", "simnow_client_test");
            });

            _api.OnRspAuthenticate += new TradeApi.RspAuthenticate((ref CThostFtdcRspAuthenticateField pRspAuthenticate,
            ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast) =>
            {
                if (pRspInfo.ErrorID == 0)
                {
                    _api.UserLogin(-3, _investorID, _password);
                }
                else
                {
                    Console.WriteLine("Authenticate error: " + pRspInfo.ErrorMsg);
                    throw new Exception("Authenticate error:" + pRspInfo.ErrorMsg);
                }
            });

            _api.OnRspUserLogin += new TradeApi.RspUserLogin((ref CThostFtdcRspUserLoginField pRspUserLogin,
            ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast) =>
            {
                _isLogin = true;
                _api.SettlementInfoConfirm(-4);
            });

            _api.OnDisconnected += new TradeApi.Disconnected((int nReasion) =>
            {
              _isConnected = false;
            });
            _api.OnRspUserLogout += new TradeApi.RspUserLogout((ref CThostFtdcUserLogoutField pUserLogout,
            ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast) =>
            {
                _isLogin = false;
                _api.Disconnect();
            });

            _api.Connect();
            Thread.Sleep(500);
        }


        /// <summary>
        /// 测试获取接口版本号
        /// </summary>
        public void TestGetApiVersion()
        {
            string result = _api.GetApiVersion();
            Console.WriteLine("Api version: " + result);
        }


        /// <summary>
        /// 测试获取交易日
        /// </summary>
        public void TestGetTradingDay()
        {
            string result = _api.GetTradingDay();
            Console.WriteLine("交易日：" + result);
        }


        /// <summary>
        /// 测试查询报单
        /// </summary>
        public void TestQueryOrder()
        {
            _api.OnRspQryOrder += new TradeApi.RspQryOrder((ref CThostFtdcOrderField pOrder,
                ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast) =>
            {
                if (pRspInfo.ErrorID == 0)
                {
                    Console.WriteLine("报单查询成功, 合约代码：{0}，价格：{1}", pOrder.InstrumentID, pOrder.LimitPrice);
                }
                else
                {
                    Console.WriteLine(pRspInfo.ErrorMsg);
                }
            });
            _api.QueryOrder(1);
            Thread.Sleep(200);
        }

        /// <summary>
        /// 测试查询成交
        /// </summary>
        public void TestQueryTrade()
        {
            //_api.OnRspQryTrade += new TradeApi.RspQryTrade((ref CThostFtdcTradeField pTrade,
            //    ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast) =>
            //{
            //    if (pRspInfo.ErrorID == 0)
            //    {
            //        Console.WriteLine("成交查询成功, TradeID: {0}", pTrade.TradeID);
            //    }
            //    else
            //    {
            //        Console.WriteLine(pRspInfo.ErrorMsg);
            //    }
            //});
            _api.QueryTrade(1);
            Thread.Sleep(200);
        }

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
        /// 测试查询投资者持仓
        /// </summary>
        public void TestQueryInvestorPosition()
        {
            //_api.OnRspQryInvestorPosition += new TradeApi.RspQryInvestorPosition((
            //    ref CThostFtdcInvestorPositionField pInvestorPosition, ref CThostFtdcRspInfoField pRspInfo,
            //    int nRequestID, byte bIsLast) =>
            //{
            //    if (pRspInfo.ErrorID == 0)
            //    {
            //        Console.WriteLine("投资者持仓查询成功, 合约代码：{0}", pInvestorPosition.InstrumentID);
            //    }
            //    else
            //    {
            //        Console.WriteLine(pRspInfo.ErrorMsg);
            //    }
            //});

            _api.QueryInvestorPosition(1);
            Thread.Sleep(200);
        }

      

        /// <summary>
        /// 测试查询帐户资金
        /// </summary>
        public void TestQueryTradingAccount()
        {
            //_api.OnRspQryTradingAccount += new TradeApi.RspQryTradingAccount((
            //    ref CThostFtdcTradingAccountField pTradingAccount, ref CThostFtdcRspInfoField pRspInfo,
            //    int nRequestID, byte bIsLast) =>
            //{
            //    if (pRspInfo.ErrorID == 0)
            //    {
            //        Console.WriteLine("帐户资金查询成功, Available: {0}", pTradingAccount.Available);
            //    }
            //    else
            //    {
            //        Console.WriteLine(pRspInfo.ErrorMsg);
            //    }
            //});
            _api.QueryTradingAccount(1);
            Thread.Sleep(200);
        }


        public void RspQryTradingAccount(ref CThostFtdcTradingAccountField pTradingAccount, ref CThostFtdcRspInfoField pRspInfo,
                int nRequestID, byte bIsLast)
        {
            if (pRspInfo.ErrorID == 0)
            {
                Console.WriteLine("帐户资金查询成功, Available: {0}", pTradingAccount.Available);
            }
            else
            {
                Console.WriteLine(pRspInfo.ErrorMsg);
            }
        }


        public void TestQueryInstrument()
        {
            _api.QueryInstrument(-4);
        }

        /// <summary>
        /// 测试查询投资者
        /// </summary>
        public void TestQueryInvestor()
        {
            _api.OnRspQryInvestor += new TradeApi.RspQryInvestor((ref CThostFtdcInvestorField pInvestor,
                ref CThostFtdcRspInfoField pRspInfo, int nRequestID, byte bIsLast) =>
            {
                if (pRspInfo.ErrorID == 0)
                {
                    Console.WriteLine("投资者查询成功, InvestorID: {0}", pInvestor.InvestorID);
                }
                else
                {
                    Console.WriteLine(pRspInfo.ErrorMsg);
                }
            });
            _api.QueryInvestor(1);
            Thread.Sleep(200);
        }

        /// <summary>
        /// 测试查询投资者持仓明细
        /// </summary>
        public void TestQueryInvestorPositionDetail()
        {
            //_api.OnRspQryInvestorPositionDetail += new TradeApi.RspQryInvestorPositionDetail((
            //    ref CThostFtdcInvestorPositionDetailField pInvestorPositionDetail, ref CThostFtdcRspInfoField pRspInfo,
            //    int nRequestID, byte bIsLast) =>
            //{
            //    if (pRspInfo.ErrorID == 0)
            //    {
            //        Console.WriteLine("投资者持仓明细查询成功, 合约代码：{0}", pInvestorPositionDetail.InstrumentID);
            //    }
            //    else
            //    {
            //        Console.WriteLine(pRspInfo.ErrorMsg);
            //    }
            //});
            _api.QueryInvestorPositionDetail(1);
            Thread.Sleep(200);
        }

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
        /// 测试查询行情
        /// </summary>
        public void TestQryDepthMarketData()
        {
            _api.OnRspQryDepthMarketData += new TradeApi.RspQryDepthMarketData((
                ref CThostFtdcDepthMarketDataField pDepthMarketData, ref CThostFtdcRspInfoField pRspInfo,
                int nRequestID, byte bIsLast) =>
            {
                if (pRspInfo.ErrorID == 0)
                {
                    Console.WriteLine("行情查询成功, 合约代码：{0}", pDepthMarketData.InstrumentID);
                }
                else
                {
                    Console.WriteLine(pRspInfo.ErrorMsg);
                }
            });
            _api.QueryMarketData(1, "bu1712");
            Thread.Sleep(200);
        }

        /// <summary>
        /// 测试查询投资者结算结果
        /// </summary>
        public void TestQuerySettlementInfo()
        {
            _api.OnRspQrySettlementInfo += new TradeApi.RspQrySettlementInfo((
                ref CThostFtdcSettlementInfoField pSettlementInfo, ref CThostFtdcRspInfoField pRspInfo,
                int nRequestID, byte bIsLast) =>
            {
                if (pRspInfo.ErrorID == 0)
                {
                    Console.WriteLine("投资者结算结果查询成功, SettlementID: {0}", pSettlementInfo.SettlementID);
                }
                else
                {
                    Console.WriteLine(pRspInfo.ErrorMsg);
                }
            });
            _api.QuerySettlementInfo(1);
            Thread.Sleep(200);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(_accountDataManagerInstanceDic.Count > 0)
            {
                AccountDataManager accountDataManager = _accountDataManagerInstanceDic.Values.FirstOrDefault();

                accountDataManager.QueryInstrument();
            }
        }
    }
}
