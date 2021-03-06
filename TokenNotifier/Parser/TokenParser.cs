﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TokenNotifier.Models;
using Microsoft.Extensions.Logging;


namespace TokenNotifier.Parser
{
    public static class TokenParser
    {
        public static Token GetTokenByContract(string contract)
        {
            string resResponse;
            Token res = new Token();
            System.Threading.Thread.Sleep(5000);

            using (WebClient wc = new WebClient())
            {
                resResponse = wc.DownloadString("https://etherscan.io/token/" + contract);
            }

            res.Contract = contract;

            // Get long name
            string fullName = GetStringByTag(resResponse, "<title>\r\n\t");
            res.Name = fullName.Replace("\r\n", string.Empty);

            // Get short name
          //  res.ShortName = fullName.Split(' ')[1];
           // res.ShortName = res.ShortName.Substring(1, res.ShortName.Length - 2);

            // Get decimals
            string decimals = GetStringByTag(resResponse, "Decimals:</div>\n<div class=\"col-md-8\">\n", "\n");
            res.Decimals = int.Parse(decimals);

            // Get total supply
            string supply = GetStringByTag(resResponse, "Total Supply:</span></div>\n<div class=\"col-md-8 font-weight-medium\">");
            res.ShortName = supply.Split(' ')[1];
            supply = supply.Split(' ')[0].Replace(",", string.Empty);
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

        public static double GetPriceByContract(string contract, ILogger _logger)
        {
            System.Threading.Thread.Sleep(5000);

            string resResponse;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    resResponse = wc.DownloadString("https://etherscan.io/token/" + contract);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Contract getting etherscan error. Address " + "https://etherscan.io/token/" + contract + " Exception message: " +e.Message);
            }

            // Get long name
            string price = GetStringByTag(resResponse, "Price:</span></div>\n<span class=\"d-block\">\n$");
            //   price = price.Split(' ')[0].Replace('.', ',');
            price = price.Split(' ')[0];

            try
            {
                return double.Parse(price);
            }
            catch (Exception e)
            {
                throw new Exception("Contract error: "+ contract +". price string = " +price);
            }
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
