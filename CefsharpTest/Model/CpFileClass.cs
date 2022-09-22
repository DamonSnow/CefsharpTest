using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefsharpTest.Model
{
    public class CpFileClass
    {
        public int file_id { get; set; }
        public string created_datetime { get; set; }

        public long file_size { get; set; }
        public string customer_product_name { get; set; }
        public string version { get; set; }
        public string wafer_no { get; set; }
        public string file_name { get; set; }
        public string file_ext { get; set; }
        public string file_src { get; set; }
        public string file_full_path { get; set; }
        

        public int cp_item_cnt { get; set; }
        public int cp_point_cnt { get; set; }
        public int cp_point_item_cnt { get; set; }
        public int cp_point_bin1_cnt { get; set; }
        public int cp_point_dpat_bin1_cnt { get; set; }
        public string guid { get; set; }
        public int state { get; set; }
        //public string remark { get; set; }
        //public string updated_datetime { get; set; }
        //public int updated_user_id { get; set; }
    }
}
