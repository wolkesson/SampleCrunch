using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample_Crunch
{
    public static class Helpers
    {
        public static double Clamp(this double value, double lower, double upper)
        {
            return Math.Min(Math.Max(value, lower), upper);
        }


        public static void AddUnique(this Dictionary<string, string> dictionary, string newKey, string newValue)
        {
            string append = string.Empty;
            int index = 0;
            retry:
            if (dictionary.ContainsKey(newKey + append))
            {
                index++;
                append = "_" + index.ToString();
                goto retry;
            }
            dictionary.Add(newKey + append, newValue);
        }

    }

}
