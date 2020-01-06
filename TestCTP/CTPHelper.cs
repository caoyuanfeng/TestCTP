using CTPMarketApi;
using CTPTradeApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCTP
{
    public class CTPHelper
    {
        private Dictionary<string, TradeApi> _traderInstanceDic = new Dictionary<string, TradeApi>();
        private MarketApi _marketDataInstance = null;
        private readonly object _lockInit = new object();
        private static readonly object _lockInit1 = new object();
        private static CTPHelper _instance;

        private CTPHelper()
        {
        }

        public static CTPHelper GetInstance()
        {
            lock (_lockInit1)
            {
                if (_instance == null)
                {
                    _instance = new CTPHelper();
                }
            }
            return _instance;
        }

        public TradeApi GetCTPTraderInstance(Account account,Broker broker)
        {
            lock (_lockInit)
            {
                if (!_traderInstanceDic.ContainsKey(account.AccountID))
                {
                    TradeApi traderInstance = new TradeApi(account.BrokerID, broker.TradeIP);
                    _traderInstanceDic.Add(account.AccountID, traderInstance);
                }

                return _traderInstanceDic[account.AccountID];
            }
        }

        public MarketApi GetCTPMarketDataInstance()
        {
            lock (_lockInit)
            {
                if (_marketDataInstance == null)
                {
                    _marketDataInstance = new MarketApi();
                }

                return _marketDataInstance;
            }
        }
    }
}
