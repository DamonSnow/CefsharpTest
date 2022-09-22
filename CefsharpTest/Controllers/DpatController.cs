using CefsharpTest.Model;
using CefsharpTest.Module;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CefsharpTest.Controllers
{
    public class DpatController
    {
        public object selectWaferCpFile()
        {
            try
            {
                string file_full_path = GlobalVaries.dpatForm.selectWaferCpFile();

                return Newtonsoft.Json.JsonConvert.SerializeObject(new { actionresult = new { code = "0" }, data = file_full_path, remark = "" });
            }
            catch (Exception ex)
            {
                Log4netHelper.Error(this.GetType(), ex);

                return Newtonsoft.Json.JsonConvert.SerializeObject(new { actionresult = new { code = "1" }, remark = "异常：" + ex.Message });
            }
        }

        public object uploadWaferCpData(string form_upload_json)
        {
            try
            {
                if (string.IsNullOrEmpty(form_upload_json))
                {
                    throw new Exception("表单数据不能为空");
                }

                WaferCpFormClass waferCpForm = Newtonsoft.Json.JsonConvert.DeserializeObject<WaferCpFormClass>(form_upload_json);

                if (waferCpForm == null)
                {
                    throw new Exception("表单数据错误");
                }

                GlobalVaries.dpatForm.UploadWaferCpData(waferCpForm);

                return Newtonsoft.Json.JsonConvert.SerializeObject(new { actionresult = new { code = "0" }, data = "", remark = "" });
            }
            catch (Exception ex)
            {
                Log4netHelper.Error(this.GetType(), ex);

                return Newtonsoft.Json.JsonConvert.SerializeObject(new { actionresult = new { code = "1" }, remark = "异常：" + ex.Message });
            }
        }

        public object getFiles(int pageNumber = 1,
            int pageSize = 10,
            string col_name = "",
            string search_text = "",
            string customer_product_name = "",
            string version = "",
            string date_name = "created_day",
            string dateRange = "")
        {
            string sql = string.Empty;

            try
            {
                List<string> whereClauseArr = new List<string>();
                whereClauseArr.Add(" state = 1 ");//默认

                if (!string.IsNullOrEmpty(search_text.Trim()))
                {
                    whereClauseArr.Add(string.Format(" {0} like '%{1}%' ", col_name, search_text.Trim().Replace("'", "''")));
                }

                if(!customer_product_name.Trim().Equals("all"))
                {
                    whereClauseArr.Add(string.Format(" customer_product_name = '{0}' ", customer_product_name));
                }

                if(!version.Trim().Equals("all"))
                {
                    whereClauseArr.Add(string.Format(" version = '{0}' ", version));
                }

                DateTime dateRangeStart = DateTime.Now;
                DateTime dateRangeEnd = DateTime.Now;

                string dateRangeStartStr = string.Empty;
                string dateRangeEndStr = string.Empty;

                dateRange = dateRange.Trim();
                if (!string.IsNullOrEmpty(dateRange))
                {
                    if (dateRange.IndexOf(",") == -1)
                    {
                        throw new Exception("日期参数错误1！");
                    }

                    string[] temp = dateRange.Split(',');
                    if (temp.Length != 2)
                    {
                        throw new Exception("日期参数错误2！");
                    }

                    if (!DateTime.TryParse(temp[0], out dateRangeStart))
                    {
                        throw new Exception("起始日期参数错误");
                    }
                    else
                    {
                        dateRangeStartStr = dateRangeStart.ToString("yyyy-MM-dd");
                    }

                    if (!DateTime.TryParse(temp[1], out dateRangeEnd))
                    {
                        throw new Exception("结束日期参数错误");
                    }
                    else
                    {
                        dateRangeEndStr = dateRangeEnd.ToString("yyyy-MM-dd");
                    }

                    whereClauseArr.Add(string.Format(" {0} >= '{1}' and {0} <= '{2}' ", date_name, dateRangeStartStr, dateRangeEndStr));
                }

                string whereClauseStr = string.Empty;
                if (whereClauseArr.Count > 0)
                {
                    whereClauseStr = string.Join(" and ", whereClauseArr.ToArray());
                }
                else
                {
                    whereClauseStr = " 1 = 1 ";
                }

                int offset = (pageNumber - 1) * pageSize;

                string related_table = "cp_file";
                string primary_key = "file_id";
                string order_by = "file_id desc";

                sql = string.Format("select count({0}) from {1} where {2}", primary_key, related_table, whereClauseStr);
                object total_obj = GlobalVaries.mySqlHelper_dpat.ExecuteScalar(sql);
                if(total_obj == null || total_obj == DBNull.Value)
                {
                    throw new Exception("服务器忙，请稍后再试！");
                }
                int total = int.Parse(total_obj.ToString());

                List<string> column_list = new List<string>();

                PropertyInfo[] cp_file_props = typeof(CpFileClass).GetProperties();

                for(int i = 0, len = cp_file_props.Length; i < len; i++)
                {
                    column_list.Add(cp_file_props[i].Name);
                }

                //List<string> column_list = new List<string>() {
                //    "file_id",
                //    "created_datetime",
                //    "customer_product_name",
                //    "version",
                //    "wafer_no",
                //    "file_name",
                //    "file_ext",
                //    "file_src",
                //    "file_full_path",
                //    "file_size",
                //    "cp_item_cnt",
                //    "cp_point_cnt",
                //    "cp_point_item_cnt",
                //    "cp_point_bin1_cnt",
                //    "cp_point_dpat_bin1_cnt",
                //    "guid",
                //    "state"
                //};
                sql = string.Format("select {0} from {1} where {2} order by {3} limit {4}, {5}", string.Join(",", column_list.ToArray()), related_table, whereClauseStr, order_by, offset, pageSize);
                DataTable dt = GlobalVaries.mySqlHelper_dpat.ExecuteDataTable(sql);

                List<Dictionary<string, object>> cp_file_list = new List<Dictionary<string, object>>();
                //List<object> cp_file_list = new List<object>();

                for(int i = 0, len = dt.Rows.Count; i < len; i++)
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    
                    for(int j = 0, jlen = column_list.Count; j < jlen; j++)
                    {
                        string column_name = column_list[j];

                        dic.Add(column_name, dt.Rows[i][column_name]);
                    }

                    cp_file_list.Add(dic);
                }

                return Newtonsoft.Json.JsonConvert.SerializeObject(new { actionresult = new { code = "0" }, total = total, data = cp_file_list, remark = "" });
            }
            catch (Exception ex)
            {
                ex = new Exception("执行" + System.Reflection.MethodBase.GetCurrentMethod().Name + "发生异常，异常信息：" + ex.Message, ex);

                Log4netHelper.Error(this.GetType(), ex);

                return Newtonsoft.Json.JsonConvert.SerializeObject(new { actionresult = new { code = "1" }, remark = "异常：" + ex.Message });
            }
        }

        public object runDpat(int file_id)
        {
            try
            {
                GlobalVaries.dpatForm.RunDpat(file_id);

                return Newtonsoft.Json.JsonConvert.SerializeObject(new { actionresult = new { code = "0" }, data = "", remark = "" });
            }
            catch (Exception ex)
            {
                Log4netHelper.Error(this.GetType(), ex);

                return Newtonsoft.Json.JsonConvert.SerializeObject(new { actionresult = new { code = "1" }, remark = "异常：" + ex.Message });
            }
        }

        public object testDpat()
        {
            try
            {
                //DpatHelper.run(
                //    @"D:\DPAT\20220525\P12076-10_Data_202205090006.xlsx",
                //    "HF0071A",
                //    "V4",
                //    new List<DpatItemClass>()
                //    {
                //        new DpatItemClass(){ test_name = "VCC3_True_LKG", sigma_multiplier = 4, bin_code = 98 },
                //        new DpatItemClass(){ test_name = "VCC1_LKG_Again", sigma_multiplier = 4, bin_code = 97 },
                //        new DpatItemClass(){ test_name = "VBAT_LKG_Again", sigma_multiplier = 4, bin_code = 96 },
                //        new DpatItemClass(){ test_name = "VCC2_True_LKG_Again", sigma_multiplier = 4, bin_code = 95 },
                //        new DpatItemClass(){ test_name = "VCC3_True_LKG_Again", sigma_multiplier = 4, bin_code = 94 },
                //        new DpatItemClass(){ test_name = "VCC2_LKG_Again", sigma_multiplier = 5, bin_code = 93 },
                //    });

                DpatHelper.run(
                    @"D:\DPAT\20220614\P11737-18_Data_202202250347.xlsx",
                    "HF0071A",
                    "V2",
                    new List<DpatItemClass>()
                    {
                        new DpatItemClass(){ test_name = "VCC1_LKG_Again", sigma_multiplier = 7, bin_code = 98 },
                        new DpatItemClass(){ test_name = "VCC2_LKG_Again", sigma_multiplier = 7, bin_code = 97 },
                        new DpatItemClass(){ test_name = "VBAT_LKG_Again", sigma_multiplier = 7, bin_code = 96 }
                    });

                return Newtonsoft.Json.JsonConvert.SerializeObject(new { actionresult = new { code = "0" }, data = "", remark = "" });
            }
            catch (Exception ex)
            {
                Log4netHelper.Error(this.GetType(), ex);

                return Newtonsoft.Json.JsonConvert.SerializeObject(new { actionresult = new { code = "1" }, remark = "异常：" + ex.Message });
            }
        }
    }
}
