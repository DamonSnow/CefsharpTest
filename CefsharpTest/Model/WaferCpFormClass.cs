using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefsharpTest.Model
{
    public class WaferCpFormClass
    {
        public string file_full_path { get; set; }
        public string customer_product_name { get; set; }
        public string version { get; set; }
        public string wafer_no { get; set; }

        public int sheet_index { get; set; }

        public int cp_item_first_col_index { get; set; }
        public int cp_item_test_no_row_index { get; set; }
        public int cp_item_test_name_row_index { get; set; }
        public int cp_item_usl_row_index { get; set; }
        public int cp_item_lsl_row_index { get; set; }
        public int cp_item_unit_row_index { get; set; }

        public int cp_point_first_row_index { get; set; }
        public int cp_point_number_col_index { get; set; }
        public int cp_point_x_coord_col_index { get; set; }
        public int cp_point_y_coord_col_index { get; set; }
        public int cp_point_site_num_col_index { get; set; }
        public int cp_point_bin_code_col_index { get; set; }
    }
}
