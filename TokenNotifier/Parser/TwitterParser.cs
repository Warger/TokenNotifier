using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using TokenNotifier.Data;
using TokenNotifier.Models;
using System.Net.Http;
using System.Text;
using System.Web;

namespace TokenNotifier.Parser
{
    public class Twitt
    {
        public DateTime dateTime;
        public string content;
    }


    public static class TwitterParser
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static void Update(DbCryptoContext db)
        {
            List<TwitterAccount> cachAccs = db.TwitterAccount.ToList();
            foreach (TwitterAccount ta in cachAccs)
            {

                DateTime currentTime = DateTime.Now;

                string resResponse;
                System.Threading.Thread.Sleep(3000);

                using (WebClient wc = new WebClient())
                {
                    resResponse = wc.DownloadString("https://twitter.com/" + ta.Name);
                }

                // get content
                List<Twitt> tweets = new List<Twitt>();
                string rs = resResponse;

                while (true)
                {
                    string tag = GetStringByTag(rs, "<div class=\"js-tweet-text-container\">", "</p>");
                    if (tag != "")
                    {
                        int ind = rs.IndexOf("<div class=\"js-tweet-text-container\">");
                        // cut inner tag : <p class="TweetTextSize TweetTextSize--normal js-tweet-text tweet-text" lang="tl" data-aria-label-part="0">
                        rs = rs.Substring(ind + 20);
                        HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(tag);
                        string result = htmlDoc.DocumentNode.InnerText;

                        Twitt t = new Twitt() { content = result.ToLower() };
                        tweets.Add(t);
                    }
                    else
                    {
                        break;
                    }
                }

                rs = resResponse;
                int index = 0;
                while (true)
                {
                    string tag = GetStringByTag(rs, "_timestamp js-short-timestamp", "</span>", false);
                    if (tag != "")
                    {
                        int ind = rs.IndexOf("_timestamp js-short-timestamp");
                        int localind = tag.IndexOf("data-time=\"") + "data-time=\"".Length;

                        rs = rs.Substring(ind + 50);

                        tweets[index].dateTime = epoch.AddSeconds(long.Parse(GetUntilOrEmpty(tag.Substring(localind), "\"")));
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }

                List<Twitt> newTwitts = tweets.Where(t => t.dateTime > ta.LastUpdated).ToList();

                Wallet w = db.Wallets.FirstOrDefault();

                //Get filter words
                List<string> filter = ta.Template.Split(new string[] { "^^" }, StringSplitOptions.None).ToList();
                List<Notification> nots = new List<Notification>();
                newTwitts.ForEach(t => {
                    if (filter.Any(t.content.Contains))
                    {
                        nots.Add(new Notification()
                        {
                            Action = Actions.NewTweet,
                            DateTime = t.dateTime,
                            Description = "New tweet from " + ta.Name + ". " + t.content,
                            LinkedWallet = w
                        });
                        //       Call(nots.Last().Description, "79997135966"); 79250813700
                        Call(nots.Last().Description, "79250813700");
                    }
                });

                nots.ForEach(d => db.Notifications.Add(d));
                db.SaveChanges();

                ta.LastUpdated = currentTime;
                db.SaveChanges();
            }
        }

        private static string BuildURLForTwitter(string twitterName)
        {
            return "https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name=" + twitterName + "&count=100";
        }

        private static DateTime ConvertToDateTime(string s)
        {
            return DateTime.Parse(s);
        }

        public static void Call(string text, string number)
        {
            string myString = HttpUtility.UrlEncode(text);

            string httpRequest = "https://calltools.ru/lk/cabapi_external/api/v1/phones/call/?public_key=4fe95003e3c5d18acaec94c0ba99ab58"+
                "&phone=%2B"+number+"&campaign_id=187588389&text="+ new string(myString.Take(70).ToArray()) + "&speaker=Tatyana";

            try
            {
                using (var client = new WebClient())
                {
                    var responseString = client.DownloadString(httpRequest);
                }
            }
            catch (Exception e)
            {

            }
        }

        private static string GetStringByTag(string page, string startTag, string endTag = "<", bool enableFirstTag = true)
        {
            string res = "";

            int index = page.IndexOf(startTag);

            if (index < 0)
                return res;

            index += startTag.Length;
            
            if (enableFirstTag)
                while (page[index] != '>')
                    index++;

            index++;

            string substring = page.Substring(index);
            res = GetUntilOrEmpty(substring, endTag);

            return res;
        }

        public static string GetUntilOrEmpty(string text, string stopAt = "<")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return String.Empty;
        }
    }
}
