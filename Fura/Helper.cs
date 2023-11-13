using System;
using System.Numerics;
using MongoDB.Bson;

namespace Neo.Plugins
{
    public static class Helper
    {
        public static string WipeNumStrToFitDecimal128(this string str)
        {
            if(BigInteger.TryParse(str, out BigInteger bigint))
            {
                var maxLength = 34;
                if (str.Length > maxLength)
                {
                    var t = bigint / (BigInteger)Math.Pow(10, str.Length - maxLength);
                    t = t * (BigInteger)Math.Pow(10, str.Length - maxLength);
                    str = t.ToString();
                }
            }
            return str;
        }
    }
}

