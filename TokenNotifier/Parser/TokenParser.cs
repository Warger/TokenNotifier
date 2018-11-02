using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TokenNotifier.Models;

namespace TokenNotifier.Parser
{
    public static class TokenParser
    {
        public static Token GetTokenByContract(string contract)
        {
            string resResponse;
            Token res = new Token();
            using (WebClient wc = new WebClient())
            {
                resResponse = wc.DownloadString("https://etherscan.io/token/" + contract);
            }

            res.Contract = contract;

            // Get long name
            string fullName = GetStringByTag(resResponse, "<title>\r\n\t");
            res.Name = fullName.Split(' ')[0];

            // Get short name
            res.ShortName = fullName.Split(' ')[1];
            res.ShortName = res.ShortName.Substring(1, res.ShortName.Length - 2);

            // Get decimals
            string decimals = GetStringByTag(resResponse, "Decimals:&nbsp;\n</td>\n<td>\n", "\n");
            res.Decimals = int.Parse(decimals);

            // Get total supply
            string supply = GetStringByTag(resResponse, "Supply:</span>\n</td>\n<td class=\"tditem\">\n").Split(' ')[0].Replace(",", string.Empty);
            res.TotalSupply = ulong.Parse(supply);

            // Set percent to notification
            res.PercentForNotification = 0.1;

            // Set is observerd
            res.IsObserved = true;

            return res;
        }

        private static string GetStringByTag(string page, string startTag, string endTag = "<")
        {
            string res = "";

            int index = page.IndexOf(startTag);

            if (index < 0)
                return res;

            index += +startTag.Length;

            string substring = page.Substring(index, page.Length - index);
            res = GetUntilOrEmpty(substring, endTag);

            return res;
        }

        public static double GetPriceByContract(string contract)
        {
            string resResponse;
            using (WebClient wc = new WebClient())
            {
                resResponse = wc.DownloadString("https://etherscan.io/token/" + contract);
            }

            // Get long name
            string price = GetStringByTag(resResponse, "Price:</span></td>\n<td>\n$");
            //   price = price.Split(' ')[0].Replace('.', ',');
            price = price.Split(' ')[0];

            return double.Parse(price);
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
