using CefsharpTest.Module;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefsharpTest.Controllers
{
    public class InitController
    {
        public object initForDpatCpFile()
        {
            string sql = string.Empty;

            try
            {
                sql = "select customer_product_name, version, test_name, sigma_multiplier, guid from dpat_item where state = 1 order by customer_product_name, version, item_id";
                DataTable dpat_items = GlobalVaries.mySqlHelper_dpat.ExecuteDataTable(sql);

                return Newtonsoft.Json.JsonConvert.SerializeObject(new { actionresult = new { code = "0" }, data = dpat_items, remark = "" });
            }
            catch (Exception ex)
            {
                Log4netHelper.Error(this.GetType(), ex);

                return Newtonsoft.Json.JsonConvert.SerializeObject(new { actionresult = new { code = "1" }, remark = "异常：" + ex.Message });
            }
        }
    }
}
