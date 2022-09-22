using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefsharpTest.Model
{
    public class CpItemClass
    {
        public long item_id { get; set; }
        public string created_datetime { get; set; }
        public int file_id { get; set; }
        public int test_no { get; set; }
        public string test_name { get; set; }
        public decimal usl { get; set; }
        public decimal lsl { get; set; }
        public string unit { get; set; }

        public int item_no { get; set; }

        public string test_type { get; set; }

        public string guid { get; set; }
    }
}
