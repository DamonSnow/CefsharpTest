using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefsharpTest.Model
{
    public class CpPointItemClass
    {
        public long id { get; set; }
        public string created_datetime { get; set; }
        public int file_id { get; set; }
        public int number { get; set; }
        public int x_coord { get; set; }
        public int y_coord { get; set; }
        public int site_num { get; set; }
        public int bin_code { get; set; }

        public int test_no { get; set; }
        public string test_name { get; set; }
        public decimal usl { get; set; }
        public decimal lsl { get; set; }
        public string unit { get; set; }
        public int item_no { get; set; }

        public decimal value { get; set; }
        public int sub_bin_code { get; set; }

        public int dpat_sub_bin_code = 0;
        public long p_id { get; set; }
        public long item_id { get; set; }

        public string guid { get; set; }

        public CpPointClass cp_point = new CpPointClass();
        public CpItemClass cp_item = new CpItemClass();
    }
}
