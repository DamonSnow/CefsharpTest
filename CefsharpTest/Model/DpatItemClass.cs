using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefsharpTest.Model
{
    public class DpatItemClass
    {
        public int item_id { get; set; }
        public string created_datetime { get; set; }
        public string customer_product_name { get; set; }
        public string version { get; set; }
        public string test_name { get; set; }
        public decimal sigma_multiplier { get; set; }
        public int bin_code { get; set; }
        public int state { get; set; }
        public string guid { get; set; }
    }
}
