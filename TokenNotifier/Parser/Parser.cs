using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TokenNotifier.Data;
using TokenNotifier.Models;

namespace TokenNotifier.Parser
{
    public class Parser
    {
        public async void Update(DbCryptoContext db)
        {
            foreach (Token t in db.Tokens)
            {
                if (!t.IsObserved)
                    continue;

                Transfer lastTransfer = db.Transfers.Where(ft => ft.Token == t.Contract).OrderBy(ft => ft.Date).LastOrDefault();
                DateTime startTime = lastTransfer == null ? ConvertTime(DateTime.Now.AddHours(-24)) : lastTransfer.Date.AddMinutes(-20);

                List<Transfer> transfers = await GetTransactionsForToken(t.Contract, t.Decimals, startTime, ConvertTime(DateTime.Now));
                AddTransactions(db, t, transfers);

                UpdateTotalSupply(t.Contract);
            }
            AddNotifications(db, ConvertTime(DateTime.Now.AddHours(-24)), ConvertTime(DateTime.Now));

        }

        private void AddNotifications(DbCryptoContext db, DateTime startTime, DateTime endTime)
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
                    if (db.Notifications.Where(n => gt.Wallet == n.LinkedWallet.Address && n.DateTime.AddHours(24) > ConvertTime(DateTime.Now) && gt.Value <= n.Value*2).Count() != 0)
                        continue;

                    Wallet w = db.Wallets.SingleOrDefault(wal => wal.Address == gt.Wallet);
                    if (w == null) {
                        w = new Wallet() { Address = gt.Wallet };
                        db.Wallets.Add(w);
                        db.SaveChanges();
                    }

                    db.Notifications.Add(new Notification() { Action = Actions.BigDailySum, LinkedWallet = w, Description = "[" + t.Name + "]. Transfer summ per day: " + gt.Value + 
                        " on the wallet: " + gt.Wallet, DateTime = ConvertTime(DateTime.Now), Value = gt.Value});
                    db.SaveChanges();
                }
            }
        }

        private void AddTransactions(DbCryptoContext db, Token t, List<Transfer> transfers)
        {
            foreach (Transfer tr in transfers)
            {
                if (db.Transfers.Where(trans => trans.Token == t.Contract && tr.TransactionHash == trans.TransactionHash).Count() == 0)
                    db.Transfers.Add(tr);
            }
            db.SaveChanges();
        }

        private async Task<List<Transfer>> GetTransactionsForToken(string tokenContract, double dec, DateTime startTime, DateTime endTime)
        {
            int page = 1;
            int trasactionsPerPage = 100;
            List<Transfer> result = new List<Transfer>();
            System.Threading.Thread.Sleep(1000);

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
                        json = wc.DownloadString(BuildURL(tokenContract, page, trasactionsPerPage));
                    }

                    dynamic dso = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    var transfers = dso.transfers;
         

                    foreach (var transfer in transfers)
                    {
                        DateTime transTime = UnixTimeStampToDateTime((string)transfer.timestamp);
                        if (transTime <= startTime)
                        {
                            isLast = true;
                            continue;
                        }
                        if (transTime >= startTime && transTime < endTime)
                        {
                            double de = Math.Pow(10, dec);
                            double val = (double)transfer.value / de;
                            result.Add(new Transfer()
                            {
                                Date = transTime,
                                IncomingAddress = transfer.to,
                                OutgoingAddress = transfer.from,
                                Token = tokenContract,
                                UsdValue = transfer.usdPrice != null ?((double)transfer.usdPrice * val) : 0,
                                Value = val,
                                TransactionHash = transfer.transactionHash
                            });
                        }
                    }

                    page++;
                }

                return result;
            }
            catch (Exception e)
            {
                return result;
            }
        }

        private string BuildURL(string tokenContract, int page, int transPerPage)
        {
            return "https://ethplorer.io/service/service.php?refresh=transfers&data=" + tokenContract + "&page=tab-tab-transfers%26transfers%3D" + page +
                "%26pageSize%3D100&showTx=all";
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

        private void UpdateTotalSupply(string contract)
        {
            // TODO: ADD IT
        }

        private double GetTokenNotificationValue(Token t)
        {
            return (t.TotalSupply * 0.01) * t.PercentForNotification;
        }
    }
}
