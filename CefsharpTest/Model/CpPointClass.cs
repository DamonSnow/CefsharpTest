using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefsharpTest.Model
{
    public class CpPointClass
    {
        public long p_id { get; set; }
        public string created_datetime { get; set; }
        public int file_id { get; set; }
        public int number { get; set; }
        public int x_coord { get; set; }
        public int y_coord { get; set; }
        public int site_num { get; set; }
        public int bin_code { get; set; }

        public int dpat_bin_code { get; set; }

        public string guid { get; set; }

        //public List<CpPointItemClass> cp_point_item_list = new List<CpPointItemClass>();

        public Dictionary<string, CpPointItemClass> cp_point_item_dic = new Dictionary<string, CpPointItemClass>();
    }
}
