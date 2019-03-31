using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TokenNotifier.Data;
using TokenNotifier.Models;

namespace TokenNotifier.Parser
{
    public class Parser
    {
        private ILogger logger;

        public void Update(DbCryptoContext db, ILogger _logger)
        {
            logger = _logger;
            logger.LogDebug("Parser.Update starting...");

            foreach (Token t in db.Tokens)
            {
                if (!t.IsObserved)
                    continue;

                // Update Total Supply
                double nts = UpdateTotalSupply(t.Contract, t.Decimals);
                if (nts != -1)
                {
                    t.TotalSupply = Convert.ToUInt64(nts);
                    db.SaveChanges();
                }

                double price = TokenParser.GetPriceByContract(t.Contract);

                // Add new trascations
                Transfer lastTransfer = db.Transfers.Where(ft => ft.Token == t.Contract).OrderBy(ft => ft.Date).LastOrDefault();
                DateTime startTime = lastTransfer == null ? ConvertTime(DateTime.Now.AddHours(-24)) : lastTransfer.Date.AddMinutes(-20);

                List<Transfer> transfers = GetTransactionsForToken(t, t.Decimals, startTime, ConvertTime(DateTime.Now), price);

                AddTransactions(db, t, transfers);
            }

            // Add new notifications
            AddTransactionNotifications(db, ConvertTime(DateTime.Now.AddHours(-24)), ConvertTime(DateTime.Now));

            logger.LogDebug("Exchange Update start...");
            List<WatchedToken> wt = ExchangeUpdater.Update(db);
            logger.LogDebug("Exchange Update:" + wt.Count+ " new tokens");

            logger.LogDebug("New Token Notifications start...");
            AddNewTokenNotifications(db, wt);
            logger.LogDebug("Add New Token Notifications finished");

            logger.LogDebug("New Token Tweet Notifications updating...");
            TwitterParser.Update(db);
            logger.LogDebug("Add New Token Tweet Notifications finished");

            logger.LogDebug("Parser.Update finished");
        }

        private void SetUsdPriceToZero(DbCryptoContext db)
        {
            db.Transfers.Where(t => t.UsdValue != 0).ToList().ForEach(t => t.UsdValue = 0);
            db.SaveChanges();
        }

        private void AddNewTokenNotifications(DbCryptoContext db, List<WatchedToken> wt)
        {
            // Удаление нетребующих уведомлений обменников
            List<Exchange> exchangeToNotNotify = db.Exchange.Where(ex => !ex.NotifyOnNextUpdate).ToList();
            List<Exchange> changeExchangeStatus = new List<Exchange>();

            exchangeToNotNotify.ForEach(etnn => {
                if (wt.Where(wte => wte.Exchange.ID == etnn.ID).Count() > 0)
                    changeExchangeStatus.Add(etnn);
                });

            changeExchangeStatus.ForEach(ces => wt.RemoveAll(t => t.Exchange.ID == ces.ID));

            db.Exchange.Where(ex => changeExchangeStatus.Where(ces => ces.ID == ex.ID).Count() > 0).ToList().ForEach(ex => ex.NotifyOnNextUpdate = true);
            db.SaveChanges();

            var groupedTokens = wt.GroupBy(t => new { t.Exchange }).Select(g => new
            {
                Exchange = g.Key.Exchange,
                NumberOfTokens = g.Count()
            });
            /*
            foreach (var gt in groupedTokens)
            {
                if (gt.NumberOfTokens == 1)
                    SendSMS(wt.First(watchedToken => watchedToken.Exchange.ID == gt.Exchange.ID).Name + " was added to " + gt.Exchange.Title);
                else
                    SendSMS(gt.NumberOfTokens + " new tokens was added to " + gt.Exchange.Title);
            }
            */

            foreach (WatchedToken token in wt)
            {
                Wallet w = db.Wallets.FirstOrDefault(wal => wal.Address == token.Comment);
                db.Notifications.Add(new Notification()
                {
                    Action = Actions.NewToken,
                    LinkedWallet = w,
                    Description = "[" + token.Name + "]. New token on excange " +token.Exchange.Title+ " . Wallet address: " + token.Comment,
                    DateTime = ConvertTime(DateTime.Now),
                  });
                db.SaveChanges();
            }
        }

        private void AddTransactionNotifications(DbCryptoContext db, DateTime startTime, DateTime endTime)
        {
            var groupedTransfers = db.Transfers.Where(t => startTime <= t.Date && endTime > t.Date).GroupBy(t => new { t.Token, t.OutgoingAddress }).Select(g => new {
                Token = g.Key.Token,
                Wallet = g.Key.OutgoingAddress,
                Value = g.Sum(x => x.Value),
                UsdValue = g.Sum(x => x.UsdValue)
            });

            foreach (var gt in groupedTransfers)
            {
                // get token
                Token t = db.Tokens.First(tkn => tkn.Contract == gt.Token);

                if (GetTokenNotificationValue(t) <= gt.Value)
                {
                    if (db.Notifications.Where(n => gt.Wallet == n.LinkedWallet.Address && n.DateTime.AddHours(24) > ConvertTime(DateTime.Now) && gt.Value <= n.Value * 2).Count() != 0)
                        continue;

                    Wallet w = db.Wallets.SingleOrDefault(wal => wal.Address == gt.Wallet);
                    if (w == null) {
                        w = new Wallet() { Address = gt.Wallet };
                        w.Name = GetWalletName(w.Address);
                        db.Wallets.Add(w);
                        db.SaveChanges();
                    }

                    // Send SMS for big transaction
                    if ((gt.Value / t.TotalSupply) > 0.01)
                        SendSMS("Trascation for token " + t.Name + " - " + String.Format("{0:#,##0}", gt.UsdValue) + "$");

                    db.Notifications.Add(new Notification()
                    {
                        Action = Actions.BigDailySum,
                        LinkedWallet = w,
                        Description = "[" + t.Name + "]. Transfer summ per day: " + String.Format("{0:#,##0}", gt.Value) +
                        " (" + String.Format("{0:#,##0}", gt.UsdValue) + "$)" + " on the wallet: " + gt.Wallet,
                        DateTime = ConvertTime(DateTime.Now),
                        Value = gt.Value,
                        PercentOfSupply = gt.Value / t.TotalSupply * 100,
                        USDValue = gt.UsdValue
                    });
                    db.SaveChanges();
                }
            }
        }

        private void AddTransactions(DbCryptoContext db, Token t, List<Transfer> transfers)
        {
            int i = 0;

            foreach (Transfer tr in transfers)
            {
                if (db.Transfers.Where(trans => trans.Token == t.Contract && tr.TransactionHash == trans.TransactionHash).Count() == 0)
                {
                    db.Transfers.Add(tr);
                    i++;
                }
            }
            db.SaveChanges();

            logger.LogDebug("Token: " + t.Name + ". Added " + i + " transfers");
        }

        private List<Transfer> GetTransactionsForToken(Token token, double dec, DateTime startTime, DateTime endTime, double price)
        {
            int page = 1;
            int trasactionsPerPage = 50;
            List<Transfer> result = new List<Transfer>();

            System.Threading.Thread.Sleep(500);

            try
            {
                //JsonConvert.DeserializeObject.<List<DateTimeValue>>
                //Get JSON
                string json;

                bool isLast = false;

                while (!isLast)
                {
                    using (WebClient wc = new WebClient())
                    {
                        json = wc.DownloadString(BuildURLForTransactionDownload(token.Contract, page, trasactionsPerPage));
                    }

                    dynamic dso = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    var transfers = dso.result;

                    foreach (var transfer in transfers)
                    {
                        DateTime transTime = ConvertTime(UnixTimeStampToDateTime((string)transfer.timeStamp));
                        if (transTime <= startTime)
                        {
                            isLast = true;
                            continue;
                        }

                        double value = transfer.value / Math.Pow(10, token.Decimals);
                        double usdPrice = price * value;

                        if (transTime >= startTime && transTime < endTime)
                        {
                            result.Add(new Transfer()
                            {
                                Date = transTime,
                                IncomingAddress = transfer.to,
                                OutgoingAddress = transfer.from,
                                Value = value,
                                TransactionHash = transfer.hash,
                                Token = token.Contract,
                                UsdValue = usdPrice
                            });
                        }
                    }
                    System.Threading.Thread.Sleep(1000);

                    page++;
                }

                return result;
            }
            catch (Exception e)
            {
                logger.LogDebug("Exception on GetTransactionsForToken method: " + e.Message + " Stacktrace: " + e.StackTrace);

                return result;
            }
        }

        private string BuildURLForTransactionDownload(string tokenContract, int page, int transPerPage)
        {
            return "https://api.etherscan.io/api?module=account&action=tokentx&contractaddress=" + tokenContract + "&page=" + page + "&offset=" + transPerPage +
                "&sort=desc&apikey=6DVU9CM2YCK9IWTCV7I57UABDV1KEBU845";
        }

        private DateTime UnixTimeStampToDateTime(string unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(long.Parse(unixTimeStamp)).ToLocalTime();

            return ConvertTime(dtDateTime);
        }

        private DateTime ConvertTime(DateTime utc)
        {
            TimeZoneInfo timeInfo = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            DateTime utcKind = DateTime.SpecifyKind(utc, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utcKind, timeInfo);
        }

        private double UpdateTotalSupply(string contract, int decimals)
        {
            string resResponse;
            double res = -1;
            System.Threading.Thread.Sleep(500);

            try
            {
                using (WebClient wc = new WebClient())
                {
                    resResponse = wc.DownloadString("https://api.etherscan.io/api?module=stats&action=tokensupply&contractaddress=" + contract 
                        + "&apikey=6DVU9CM2YCK9IWTCV7I57UABDV1KEBU845");
                }

                string searchingTag = "result\":\"";

                int index = resResponse.IndexOf(searchingTag);

                if (index < 0)
                    return res;

                index += +searchingTag.Length;

                string substring = resResponse.Substring(index, resResponse.Length-index);

                res = Convert.ToDouble(TokenParser.GetUntilOrEmpty(substring, "\"")) / Math.Pow(10, decimals);
            }
            catch (Exception e)
            {
                logger.LogDebug("Exception on GetWalletName method: " + e.Message + " Stacktrace: " + e.StackTrace);

                return res;
            }

            return res;
        }

        private void SendSMS(string message, string phoneNumber = "79250813700")
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

                    queryString["api_id"] = "540DEBD9-3F8B-CE62-60F3-B9726D0B0ACA";
                    queryString["to"] = phoneNumber;
                    queryString["msg"] = message;
                    queryString["json"] = "1";

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://sms.ru/sms/send?"
                        + queryString.ToString());

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    logger.LogDebug("SendSMS method was called. Status code: " + response.StatusCode + " Status description: " + response.StatusDescription + " SMS text: " + message);
                }
            }
            catch (Exception e)
            {
                logger.LogDebug("Exception on SendSMS method: " + e.Message + " Stacktrace: " + e.StackTrace);
            }
        }

        private double GetTokenNotificationValue(Token t)
        {
            return (t.TotalSupply * 0.01) * t.PercentForNotification;
        }

        private string GetWalletName(string address)
        {
            string walletPage;
            string result = String.Empty;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    walletPage = wc.DownloadString("https://etherscan.io/address/" + address);
                }

                string searchingTag = "'NameTag'>";

                int index =  walletPage.IndexOf(searchingTag);

                if (index < 0)
                    return result;

                string substring = walletPage.Substring(index+searchingTag.Length, 100);

                result = TokenParser.GetUntilOrEmpty(substring);
            }
            catch (Exception e)
            {
                logger.LogDebug("Exception on GetWalletName method: " + e.Message + " Stacktrace: " + e.StackTrace);

                return result;
            }

            return result;
        }
    }
}
