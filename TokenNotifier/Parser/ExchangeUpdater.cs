using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TokenNotifier.Data;
using TokenNotifier.Models;

namespace TokenNotifier.Parser
{
    public static class ExchangeUpdater
    {
        public static List<WatchedToken> Update(DbCryptoContext db)
        {
            List<WatchedToken> res = new List<WatchedToken>();
            List<WatchedToken> resTwoEntries = new List<WatchedToken>();

            List<WatchedToken> counterTokens = new List<WatchedToken>();

            foreach (Exchange ex in db.Exchange)
            {
                List<WatchedToken> wt = db.WatchedTokens.Where(w => w.Exchange.ID == ex.ID).ToList();

                List<Wallet> wallets = db.Wallets.Where(w => w.Exchange.ID == ex.ID).ToList();
                if (wallets.Count == 0)
                    continue;

                foreach (Wallet w in wallets)
                {
                    BuildURLForWalletBalance(w.Address);
                    List<WatchedToken> responsedTokens = GetWalletTokens(w.Address);

                    foreach (WatchedToken token in responsedTokens)
                    {
                        if (wt.FirstOrDefault(t => t.Name == token.Name) == null)
                        {
                            WatchedToken watchToken = new WatchedToken()
                            {
                                DateTime = token.DateTime,
                                Exchange = ex,
                                Name = token.Name,
                                ShortName = token.ShortName,
                                Comment = w.Address
                            };

                            res.Add(watchToken);
                        }
                        else
                        {
                            if (wt.FirstOrDefault(t => t.Name == token.Name) != null && counterTokens.Where(t=>t.Name == token.Name).Count() == 0 && 
                                res.Where(t => t.Name == token.Name).Count() == 0)
                            {
                                counterTokens.Add(wt.FirstOrDefault(t => t.Name == token.Name));
                            }
                        }
                    }

                    wt = db.WatchedTokens.Where(wtc => wtc.Exchange.ID == ex.ID).ToList();
                }

                foreach (WatchedToken token in counterTokens)
                {
                    WatchedToken tfu = db.WatchedTokens.Where(wtc => wtc.Exchange.ID == ex.ID).FirstOrDefault(t => t.Name == token.Name);

                    if (tfu != null)
                    {
                        tfu.Counter++;
                        db.SaveChanges();
                        if (tfu.Counter == 2)
                            resTwoEntries.Add(tfu);
                    }
                }

              /*  foreach (WatchedToken tkn in tokensToRemove)
                {
                    if (res.Where(t=>t.ShortName == tkn.ShortName).Count() != 0)
                        tokensToRemove.Remove(tkn);
                }*/
                db.WatchedTokens.RemoveRange(db.WatchedTokens.Where(t => t.Counter == 0));
                db.SaveChanges();
                res.ForEach(rt => db.WatchedTokens.Add(rt));
                db.SaveChanges();
            }

            //тут фильтровать, если дохуя возвращается новых токенов
            return resTwoEntries;
        }

        public static void RemoveAllWatchedTokens(DbCryptoContext db, int excId)
        {
            var wt = db.WatchedTokens.Where(w => w.Exchange.ID == excId).ToList();
            db.WatchedTokens.RemoveRange(wt);
            Exchange ex = db.Exchange.First(e => e.ID == excId);
            ex.NotifyOnNextUpdate = false;
            db.SaveChanges();
        }

        private static string BuildURLForWalletBalance(string walletAddress)
        {
            return "http://api.ethplorer.io/getAddressInfo/" + walletAddress + "?apiKey=freekey";
        }

        private static List<WatchedToken> GetWalletTokens(string walletAddresss)
        {
            List<WatchedToken> result = new List<WatchedToken>();

            System.Threading.Thread.Sleep(5000);

            //Get JSON
            string json;

            using (WebClient wc = new WebClient())
            {
                json = wc.DownloadString(BuildURLForWalletBalance(walletAddresss));
            }

            dynamic dso = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            var balance = dso.tokens;
            DateTime currentTime = ConvertTime(DateTime.Now);

            foreach (var token in balance)
            {
                result.Add(new WatchedToken()
                {
                    DateTime = currentTime,
                    Name = token.tokenInfo.name,
                    ShortName = token.tokenInfo.symbol,
                });
            }

            return result;
        }

        private static DateTime UnixTimeStampToDateTime(string unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(long.Parse(unixTimeStamp)).ToLocalTime();

            return ConvertTime(dtDateTime);
        }

        private static DateTime ConvertTime(DateTime utc)
        {
            TimeZoneInfo timeInfo = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            DateTime utcKind = DateTime.SpecifyKind(utc, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utcKind, timeInfo);
        }
    }
}
