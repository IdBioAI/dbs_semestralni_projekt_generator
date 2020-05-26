using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator
{
    class Tenzo
    {
        String sn;
        decimal value;

        double minimum = 0;
        double maximum = 5;

        private static Random random = new Random();

        public Tenzo()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            sn = new string(Enumerable.Repeat(chars, 7)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public decimal GetValue()
        {
            value = System.Convert.ToDecimal(random.NextDouble() * (maximum - minimum) + minimum);
            return value;
        }

        public String GetSn()
        {
            return sn;
        }


    }
}
