using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace SinaFinance_7X24
{
    public static class Evt
    {
        public static event EventHandler<String> NewsKeyAdd = null;
        public static event EventHandler<String> NewsKeyDel = null;


        public static void AddNewsKey(string key)
        {
            NewsKeyAdd(null, key);
        }
        public static void DelNewsKey(string key)
        {
            NewsKeyDel(null, key);
        }

    }
}
