using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CefsharpTest.Module
{
    public class GlobalVaries
    {
        public static string app_name = "福联DPAT辅助工具";
        public static string app_name_en = "unicompound_dpat_assist_tool";
        public static string app_version = "1.202205262017.001";

        #region 相关数据库参数设置

        public static string conn_efab = "server=127.0.0.1;user=root;database=usc_efab;port=3306;password=123abc456;Charset=utf8";
        public static MySqlHelper mySqlHelper_efab = new MySqlHelper(conn_efab);//本机 efab db

        public static string conn_dpat = "server=127.0.0.1;user=root;database=dpat;port=3306;password=123abc456;Charset=utf8";
        public static MySqlHelper mySqlHelper_dpat = new MySqlHelper(conn_dpat);//本机 dpat db

        #endregion

        public static DPATForm dpatForm;

        public static string temp_csv_path = Application.StartupPath + @"\temp\csv";
    }
}
