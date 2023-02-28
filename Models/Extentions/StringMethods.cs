using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DevExpress.XtraPrinting.Native;

namespace WebDtt.Models.Extentions
{
    public static class StringMethods
    {
        private static readonly char[] SpecialCharacters = new[]
        {
            ' ', '.', ',', ':', ';', '"', '!', '?',
            '>', '<', '_', '|', '@', '#', '$', '&',
            '*', '=', '+', '/', '^', '%', '-',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };
        public static string RemSpCh(string value)
        {
            var ret = "";
            if (value == null) return ret;
            var tempString = value.Where(ch => !SpecialCharacters.Contains(ch))
                .Aggregate("", (current, ch) => current + ch);

            switch (tempString.Length)
            {
                case 0: break;
                case 1: ret = tempString.ToUpper(); break;
                default:
                    ret = tempString.Substring(0, 1).ToUpper() +
                          tempString.Substring(1, tempString.Length - 1).ToLower();
                    break;
            }
            return ret;
        }
    }
}