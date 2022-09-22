using CefSharp;
using CefSharp.WinForms;
using CefsharpTest.Controllers;
using CefsharpTest.Model;
using CefsharpTest.Module;
using Microsoft.WindowsAPICodePack.Dialogs;
using MySql.Data.MySqlClient;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CefsharpTest
{
    public partial class DPATForm : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);

        [DllImport("user32.dll", EntryPoint = "ExitWindowsEx", CharSet = CharSet.Ansi)]
        private static extern int ExitWindowsEx(int uFlags, int dwReserved);

        public ChromiumWebBrowser chromeBrowser;
        public DPATForm()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            this.initializeChromium();
        }

        private void DPATForm_Load(object sender, EventArgs e)
        {
            this.lblCopyrightYear.Text = "© " + DateTime.Now.Year.ToString();

            this.lblNow.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            GlobalVaries.dpatForm = this;
        }

        private void DPATForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("确定退出？", "Prompt", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Cef.Shutdown();

                this.Dispose();
            }
            else
            {
                e.Cancel = true;
            }
        }

        public void initializeChromium()
        {
            CefSharpSettings.LegacyJavascriptBindingEnabled = true;

            CefSettings settings = new CefSettings();
            settings.CefCommandLineArgs.Add("disable-gpu", "1"); // 禁用gpu
            settings.CefCommandLineArgs.Add("--disable-web-security", "1");//关闭同源策略,允许跨域
            settings.SetOffScreenRenderingBestPerformanceArgs();

            string page = string.Format(@"{0}\www\html\dpat_cp_file.html", Application.StartupPath);

            if (!File.Exists(page))
            {
                MessageBox.Show("页面不存在！");
            }

            Cef.Initialize(settings);

            chromeBrowser = new ChromiumWebBrowser(page);

            this.panelBody.Controls.Add(chromeBrowser);
            chromeBrowser.Dock = DockStyle.Fill;

            chromeBrowser.FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(chromeBrowser_FrameLoadEnd);

            chromeBrowser.Load(page);

            //chromeBrowser.RegisterJsObject("AppController", new AppController(), new CefSharp.BindingOptions { CamelCaseJavascriptNames = false });
            chromeBrowser.RegisterJsObject("CommonController", new CommonController(), new CefSharp.BindingOptions { CamelCaseJavascriptNames = false });
            chromeBrowser.RegisterJsObject("InitController", new InitController(), new CefSharp.BindingOptions { CamelCaseJavascriptNames = false });
            chromeBrowser.RegisterJsObject("DpatController", new DpatController(), new CefSharp.BindingOptions { CamelCaseJavascriptNames = false });

            chromeBrowser.KeyboardHandler = new CEFKeyBoardHander();
        }

        void chromeBrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            string url = e.Url.ToLower();

            string sql = string.Empty;

            if (e.Frame.IsValid && e.Frame.IsMain)
            {
                //
            }

            if (e.Frame.IsValid)
            {

            }
        }

        private void timerClearMemory_Tick(object sender, EventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }

        public void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }

        private void timerInterval_Tick(object sender, EventArgs e)
        {
            this.lblNow.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void pbCloseProgress_Click(object sender, EventArgs e)
        {
            this.panelProgress.Visible = false;
        }

        public string selectWaferCpFile()
        {
            string file_full_path = string.Empty;

            try
            {
                this.Invoke(new Action(() =>
                {
                    if (CommonFunction.IsLaterThanVista() == true)
                    {
                        OpenFileDialog fileDialog = new OpenFileDialog();
                        fileDialog.Multiselect = false;
                        fileDialog.Title = "请选择具体Wafer的CP数据表格";
                        fileDialog.Filter = "Excel|*.xls;*.xlsx";
                        if (fileDialog.ShowDialog() == DialogResult.OK)
                        {
                            file_full_path = fileDialog.FileName;//返回文件的完整路径                
                        }
                    }
                    else
                    {
                        CommonOpenFileDialog fileDialog = new CommonOpenFileDialog();
                        fileDialog.IsFolderPicker = false;  // 这里一定要设置false，不然就是选择文件夹
                        fileDialog.Title = "请选择具体Wafer的CP数据表格";
                        fileDialog.Filters.Add(new CommonFileDialogFilter("Excel", "*.xls"));
                        fileDialog.Filters.Add(new CommonFileDialogFilter("Excel", "*.xlsx"));
                        if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                        {
                            file_full_path = fileDialog.FileName;
                        }
                    }
                }));

                return file_full_path;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UploadWaferCpData(WaferCpFormClass waferCpForm)
        {
            Log4netHelper.Info(this.GetType(), string.Format("开始执行 {0}", System.Reflection.MethodBase.GetCurrentMethod().Name));

            string progress_msg = "";
            progress_msg = "开始上传数据......";
            this.Invoke(new Action(() =>
            {
                this.panelProgress.Visible = true;
                this.txtProgress.Text = string.Format("{0}, {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), progress_msg);
            }));

            string sql = string.Empty;

            DateTime now = DateTime.Now;
            string datetime = now.ToString("yyyy-MM-dd HH:mm:ss");
            string date = now.ToString("yyyy-MM-dd");
            int year = now.Year;
            int month = now.Month;
            int day = now.Day;

            try
            {
                //1、cp_file
                FileInfo fi = new FileInfo(waferCpForm.file_full_path);
                if (!fi.Exists)
                {
                    throw new Exception("CP文件不存在");
                }

                long file_size = fi.Length;

                //if (!File.Exists(waferCpForm.file_full_path))
                //{
                //    throw new Exception("CP文件不存在");
                //}

                string file_name = Path.GetFileNameWithoutExtension(waferCpForm.file_full_path);
                string file_ext = Path.GetExtension(waferCpForm.file_full_path);

                string file_guid = Guid.NewGuid().ToString("N");

                List<ColumnToInsertClass> cp_file_column_list = new List<ColumnToInsertClass>();
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "created_datetime", value = datetime, need_quotation = true });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "created_date", value = date, need_quotation = true });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "created_year", value = year.ToString(), need_quotation = false });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "created_month", value = month.ToString(), need_quotation = false });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "created_day", value = day.ToString(), need_quotation = false });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "customer_product_name", value = waferCpForm.customer_product_name, need_quotation = true });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "version", value = waferCpForm.version, need_quotation = true });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "wafer_no", value = waferCpForm.wafer_no, need_quotation = true });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "file_name", value = file_name, need_quotation = true });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "file_ext", value = file_ext, need_quotation = true });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "file_full_path", value = waferCpForm.file_full_path, need_quotation = true });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "file_size", value = file_size.ToString(), need_quotation = false });

                cp_file_column_list.Add(new ColumnToInsertClass() { label = "cp_item_first_col_index", value = waferCpForm.cp_item_first_col_index.ToString(), need_quotation = false });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "cp_item_test_no_row_index", value = waferCpForm.cp_item_test_no_row_index.ToString(), need_quotation = false });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "cp_item_test_name_row_index", value = waferCpForm.cp_item_test_name_row_index.ToString(), need_quotation = false });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "cp_item_usl_row_index", value = waferCpForm.cp_item_usl_row_index.ToString(), need_quotation = false });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "cp_item_lsl_row_index", value = waferCpForm.cp_item_lsl_row_index.ToString(), need_quotation = false });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "cp_item_unit_row_index", value = waferCpForm.cp_item_unit_row_index.ToString(), need_quotation = false });

                cp_file_column_list.Add(new ColumnToInsertClass() { label = "cp_point_first_row_index", value = waferCpForm.cp_point_first_row_index.ToString(), need_quotation = false });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "cp_point_number_col_index", value = waferCpForm.cp_point_number_col_index.ToString(), need_quotation = false });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "cp_point_x_coord_col_index", value = waferCpForm.cp_point_x_coord_col_index.ToString(), need_quotation = false });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "cp_point_y_coord_col_index", value = waferCpForm.cp_point_y_coord_col_index.ToString(), need_quotation = false });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "cp_point_site_num_col_index", value = waferCpForm.cp_point_site_num_col_index.ToString(), need_quotation = false });
                cp_file_column_list.Add(new ColumnToInsertClass() { label = "cp_point_bin_code_col_index", value = waferCpForm.cp_point_bin_code_col_index.ToString(), need_quotation = false });

                cp_file_column_list.Add(new ColumnToInsertClass() { label = "guid", value = file_guid, need_quotation = true });

                List<string> col_label_list = new List<string>();
                List<string> col_value_para_list = new List<string>();
                for (int i = 0, len = cp_file_column_list.Count; i < len; i++)
                {
                    col_label_list.Add(cp_file_column_list[i].label);

                    if (cp_file_column_list[i].need_quotation)
                    {
                        col_value_para_list.Add(string.Format("'{0}'", cp_file_column_list[i].value.Replace("'", "\'")));
                    }
                    else
                    {
                        col_value_para_list.Add(cp_file_column_list[i].value);
                    }
                }

                string related_table = "cp_file";
                sql = string.Format("insert into {0}({1})values({2})", related_table, string.Join(",", col_label_list.ToArray()), string.Join(",", col_value_para_list.ToArray()));
                int ret_insert = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                int repeat_insert = 0;
                while (repeat_insert < 10)
                {
                    if (ret_insert != 1)
                    {
                        repeat_insert++;

                        Thread.Sleep(300);

                        ret_insert = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);
                    }
                    else
                    {
                        break;
                    }
                }

                if (ret_insert != 1)
                {
                    throw new Exception("插入CP文件数据失败！执行数据库操作返回值错误！");
                }

                sql = string.Format("select file_id from cp_file where guid = '{0}'", file_guid);
                object file_id_obj = GlobalVaries.mySqlHelper_dpat.ExecuteScalar(sql);
                if (file_id_obj == null || file_id_obj == DBNull.Value)
                {
                    throw new Exception("获取CP文件ID值失败");
                }

                int file_id = int.Parse(file_id_obj.ToString());

                progress_msg = string.Format("插入 cp_file 完毕，file_id : {0}", file_id);
                Log4netHelper.Info(this.GetType(), progress_msg);
                this.Invoke(new Action(() =>
                {
                    this.panelProgress.Visible = true;
                    this.txtProgress.Text = string.Format("{0}, {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), progress_msg);
                }));

                //2、cp_item
                //开始读取 表格
                Workbook workbook = new Workbook();
                workbook.LoadFromFile(waferCpForm.file_full_path);

                var sheets = workbook.Worksheets;
                var sheet = sheets[waferCpForm.sheet_index];

                int rows_cnt = sheet.Rows.Length;
                int cols_cnt = sheet.Columns.Length;

                List<int> test_no_list = new List<int>();           //第一行
                List<string> test_name_list = new List<string>();   //第二行
                List<decimal> usl_list = new List<decimal>();       //第三行
                List<decimal> lsl_list = new List<decimal>();       //第四行
                List<string> unit_list = new List<string>();        //第五行

                // 到cp点数据行为止
                for (int i = 1; i < waferCpForm.cp_point_first_row_index; i++)
                {
                    int col_index = waferCpForm.cp_item_first_col_index;

                    while (true)
                    {
                        if (sheet.Range[i, col_index].Value == null || string.IsNullOrEmpty(sheet.Range[i, col_index].Value.Trim()))
                        {
                            //若为null或为空，则说明这一行已经读取完毕，则读取下一行
                            break;
                        }

                        string cell_value = sheet.Range[i, col_index].Value.Trim();

                        if (i == waferCpForm.cp_item_test_no_row_index)
                        {
                            //test_no
                            int test_no = 0;
                            if (!int.TryParse(cell_value, out test_no))
                            {
                                throw new Exception(string.Format("第{0}行第{1}列的Test No.值错误！必须为Int类型！", i, col_index));
                            }
                            test_no_list.Add(test_no);
                        }

                        if (i == waferCpForm.cp_item_test_name_row_index)
                        {
                            test_name_list.Add(cell_value);
                        }

                        if (i == waferCpForm.cp_item_usl_row_index)
                        {
                            decimal usl = 0M;
                            if (!decimal.TryParse(cell_value, out usl))
                            {
                                throw new Exception(string.Format("第{0}行第{1}列的USL值错误！必须为Decimal类型！", i, col_index));
                            }
                            usl_list.Add(usl);
                        }

                        if (i == waferCpForm.cp_item_lsl_row_index)
                        {
                            decimal lsl = 0M;
                            if (!decimal.TryParse(cell_value, out lsl))
                            {
                                throw new Exception(string.Format("第{0}行第{1}列的LSL值错误！必须为Decimal类型！", i, col_index));
                            }
                            lsl_list.Add(lsl);
                        }

                        if (i == waferCpForm.cp_item_unit_row_index)
                        {
                            unit_list.Add(cell_value);
                        }

                        col_index++;//注意写上
                    }
                }

                //各list的元素个数必须一致
                //单位单元格比较特殊，以为最后一列可能是BinCode，那么就已第一行的Count数量为准
                //if (test_no_list.Count != test_name_list.Count
                //    || test_no_list.Count != usl_list.Count
                //    || test_no_list.Count != lsl_list.Count
                //    || test_no_list.Count != unit_list.Count)
                //{
                //    throw new Exception("CP测试项没有一一对应！");
                //}

                //单位单元格比较特殊，那么就已第一行的Count数量为准
                if (test_no_list.Count > test_name_list.Count
                    || test_no_list.Count > usl_list.Count
                    || test_no_list.Count > lsl_list.Count
                    || test_no_list.Count > unit_list.Count)
                {
                    throw new Exception("CP测试项没有一一对应！");
                }

                List<CpItemClass> cp_item_list = new List<CpItemClass>();
                Dictionary<string, CpItemClass> cp_item_dic = new Dictionary<string, CpItemClass>();
                for (int i = 0, len = test_no_list.Count; i < len; i++)
                {
                    CpItemClass cp_item = new CpItemClass()
                    {
                        test_no = test_no_list[i],
                        test_name = test_name_list[i],
                        usl = usl_list[i],
                        lsl = lsl_list[i],
                        unit = unit_list[i],
                        item_no = i + 1,
                        test_type = "cp",

                        guid = Guid.NewGuid().ToString("N")
                    };
                    cp_item_list.Add(cp_item);

                    if (cp_item_dic.ContainsKey(cp_item.test_name))
                    {
                        throw new Exception(string.Format("发现重复的 Test Name : {0}", cp_item.test_name));
                    }

                    cp_item_dic.Add(cp_item.test_name, cp_item);
                }

                for (int i = 0, len = cp_item_list.Count; i < len; i++)
                {
                    CpItemClass cp_item = cp_item_list[i];

                    List<ColumnToInsertClass> cp_item_column_list = new List<ColumnToInsertClass>();
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "created_datetime", value = datetime, need_quotation = true });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "created_date", value = date, need_quotation = true });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "created_year", value = year.ToString(), need_quotation = false });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "created_month", value = month.ToString(), need_quotation = false });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "created_day", value = day.ToString(), need_quotation = false });

                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "file_id", value = file_id.ToString(), need_quotation = false });

                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "test_no", value = cp_item.test_no.ToString(), need_quotation = false });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "test_name", value = cp_item.test_name, need_quotation = true });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "usl", value = cp_item.usl.ToString(), need_quotation = false });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "lsl", value = cp_item.lsl.ToString(), need_quotation = false });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "unit", value = cp_item.unit, need_quotation = true });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "item_no", value = cp_item.item_no.ToString(), need_quotation = false });

                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "test_type", value = cp_item.test_type, need_quotation = true });// test_type = cp

                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "guid", value = Guid.NewGuid().ToString("N"), need_quotation = true });


                    col_label_list = new List<string>();
                    col_value_para_list = new List<string>();
                    for (int j = 0, jlen = cp_item_column_list.Count; j < jlen; j++)
                    {
                        col_label_list.Add(cp_item_column_list[j].label);

                        if (cp_item_column_list[j].need_quotation)
                        {
                            col_value_para_list.Add(string.Format("'{0}'", cp_item_column_list[j].value.Replace("'", "\'")));
                        }
                        else
                        {
                            col_value_para_list.Add(cp_item_column_list[j].value);
                        }
                    }

                    related_table = "cp_item";
                    sql = string.Format("insert into {0}({1})values({2})", related_table, string.Join(",", col_label_list.ToArray()), string.Join(",", col_value_para_list.ToArray()));
                    ret_insert = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                    repeat_insert = 0;
                    while (repeat_insert < 10)
                    {
                        if (ret_insert != 1)
                        {
                            repeat_insert++;

                            Thread.Sleep(300);

                            ret_insert = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (ret_insert != 1)
                    {
                        throw new Exception(string.Format("插入CP测试项{0}数据失败！执行数据库操作返回值错误！", i + 1));
                    }
                }

                progress_msg = string.Format("插入 cp_item 完毕，cp item cnt : {0}", cp_item_list.Count);
                Log4netHelper.Info(this.GetType(), progress_msg);
                this.Invoke(new Action(() =>
                {
                    this.panelProgress.Visible = true;
                    this.txtProgress.Text = string.Format("{0}, {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), progress_msg);
                }));

                //3、cp_point
                List<CpPointClass> cp_point_list = new List<CpPointClass>();
                int cp_point_bin1_cnt = 0;
                for (int i = waferCpForm.cp_point_first_row_index; i <= rows_cnt; i++)
                {
                    int number = 1;
                    int x_coord = 0;
                    int y_coord = 0;
                    int site_num = 0;
                    int bin_code = 1;
                    string guid = Guid.NewGuid().ToString("N");

                    int col_index = 1;
                    while (col_index < waferCpForm.cp_item_first_col_index)
                    {
                        string cell_value = sheet.Range[i, col_index].Value.Trim();

                        if (col_index == waferCpForm.cp_point_number_col_index)
                        {
                            if (!int.TryParse(cell_value, out number))
                            {
                                throw new Exception(string.Format("第{0}行第{1}列的 Number 值错误！必须为Int类型！", i, col_index));
                            }
                        }

                        if (col_index == waferCpForm.cp_point_x_coord_col_index)
                        {
                            if (!int.TryParse(cell_value, out x_coord))
                            {
                                throw new Exception(string.Format("第{0}行第{1}列的 X_Coord 值错误！必须为Int类型！", i, col_index));
                            }
                        }

                        if (col_index == waferCpForm.cp_point_y_coord_col_index)
                        {
                            if (!int.TryParse(cell_value, out y_coord))
                            {
                                throw new Exception(string.Format("第{0}行第{1}列的 Y_Coord 值错误！必须为Int类型！", i, col_index));
                            }
                        }

                        if (col_index == waferCpForm.cp_point_site_num_col_index)
                        {
                            if (!int.TryParse(cell_value, out site_num))
                            {
                                throw new Exception(string.Format("第{0}行第{1}列的 SiteNum 值错误！必须为Int类型！", i, col_index));
                            }
                        }
                        col_index++;
                    }

                    //bin_code列要单独处理
                    if (waferCpForm.cp_point_bin_code_col_index == 0)
                    {
                        string cell_value = sheet.Range[i, cols_cnt].Value.Trim();

                        if (!int.TryParse(cell_value, out bin_code))
                        {
                            throw new Exception(string.Format("第{0}行第{1}列的 BinCode 值错误！必须为Int类型！", i, cols_cnt));
                        }
                    }

                    cp_point_list.Add(new CpPointClass()
                    {
                        number = number,
                        x_coord = x_coord,
                        y_coord = y_coord,
                        site_num = site_num,
                        bin_code = bin_code
                    });

                    if (bin_code == 1)
                    {
                        cp_point_bin1_cnt++;
                    }
                }

                progress_msg = string.Format("获取 cp_point 完毕，cp point cnt : {0}", cp_point_list.Count);
                Log4netHelper.Info(this.GetType(), progress_msg);
                this.Invoke(new Action(() =>
                {
                    this.panelProgress.Visible = true;
                    this.txtProgress.Text = string.Format("{0}, {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), progress_msg);
                }));

                //List<string> value_list = new List<string>();
                //for (int i = 0, len = cp_point_list.Count; i < len; i++)
                //{
                //    CpPointClass cp_point = cp_point_list[i];

                //    List<ColumnToInsertClass> cp_point_column_list = new List<ColumnToInsertClass>();
                //    cp_point_column_list.Add(new ColumnToInsertClass() { label = "created_datetime", value = datetime, need_quotation = true });
                //    cp_point_column_list.Add(new ColumnToInsertClass() { label = "created_date", value = date, need_quotation = true });
                //    cp_point_column_list.Add(new ColumnToInsertClass() { label = "created_year", value = year.ToString(), need_quotation = false });
                //    cp_point_column_list.Add(new ColumnToInsertClass() { label = "created_month", value = month.ToString(), need_quotation = false });
                //    cp_point_column_list.Add(new ColumnToInsertClass() { label = "created_day", value = day.ToString(), need_quotation = false });

                //    cp_point_column_list.Add(new ColumnToInsertClass() { label = "file_id", value = file_id.ToString(), need_quotation = false });

                //    cp_point_column_list.Add(new ColumnToInsertClass() { label = "number", value = cp_point.number.ToString(), need_quotation = false });
                //    cp_point_column_list.Add(new ColumnToInsertClass() { label = "x_coord", value = cp_point.x_coord.ToString(), need_quotation = false });
                //    cp_point_column_list.Add(new ColumnToInsertClass() { label = "y_coord", value = cp_point.y_coord.ToString(), need_quotation = false });
                //    cp_point_column_list.Add(new ColumnToInsertClass() { label = "site_num", value = cp_point.site_num.ToString(), need_quotation = false });
                //    cp_point_column_list.Add(new ColumnToInsertClass() { label = "bin_code", value = cp_point.bin_code.ToString(), need_quotation = false });

                //    cp_point_column_list.Add(new ColumnToInsertClass() { label = "guid", value = Guid.NewGuid().ToString("N"), need_quotation = true });


                //    col_label_list = new List<string>();
                //    col_value_para_list = new List<string>();
                //    for (int j = 0, jlen = cp_point_column_list.Count; j < jlen; j++)
                //    {
                //        col_label_list.Add(cp_point_column_list[j].label);

                //        if (cp_point_column_list[j].need_quotation)
                //        {
                //            col_value_para_list.Add(string.Format("'{0}'", cp_point_column_list[j].value.Replace("'", "\'")));
                //        }
                //        else
                //        {
                //            col_value_para_list.Add(cp_point_column_list[j].value);
                //        }
                //    }

                //    value_list.Add(string.Format("({0})", string.Join(",", col_value_para_list.ToArray())));

                //    related_table = "cp_point";
                //    sql = string.Format("insert into {0}({1})values({2})", related_table, string.Join(",", col_label_list.ToArray()), string.Join(",", col_value_para_list.ToArray()));
                //    ret_insert = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                //    repeat_insert = 0;
                //    while (repeat_insert < 10)
                //    {
                //        if (ret_insert != 1)
                //        {
                //            repeat_insert++;

                //            Thread.Sleep(300);

                //            ret_insert = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);
                //        }
                //        else
                //        {
                //            break;
                //        }
                //    }

                //    if (ret_insert != 1)
                //    {
                //        throw new Exception(string.Format("插入CP点值数据{0}数据失败！执行数据库操作返回值错误！", i + 1));
                //    }
                //}

                //获取 cp_point 最大的 p_id
                long max_p_id = 0;

                sql = "select max(p_id) max_p_id from cp_point";
                object max_p_id_obj = GlobalVaries.mySqlHelper_dpat.ExecuteScalar(sql);

                if (max_p_id_obj != null && max_p_id_obj != DBNull.Value)
                {
                    max_p_id = long.Parse(max_p_id_obj.ToString());
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0, len = cp_point_list.Count; i < len; i++)
                {
                    CpPointClass cp_point = cp_point_list[i];

                    sb.Append((max_p_id + i + 1).ToString() + ",");
                    sb.Append(datetime + ",");
                    sb.Append(date + ",");
                    sb.Append("0" + ",");
                    sb.Append(year.ToString() + ",");
                    sb.Append(month.ToString() + ",");
                    sb.Append(day.ToString() + ",");
                    sb.Append(file_id.ToString() + ",");
                    sb.Append(cp_point.number.ToString() + ",");
                    sb.Append(cp_point.x_coord.ToString() + ",");
                    sb.Append(cp_point.y_coord.ToString() + ",");
                    sb.Append(cp_point.site_num.ToString() + ",");
                    sb.Append(cp_point.bin_code.ToString() + ",");
                    sb.Append(Guid.NewGuid().ToString("N"));

                    sb.AppendLine();
                }

                string temp_csv_path = GlobalVaries.temp_csv_path + string.Format(@"\{0}", now.ToString("yyyyMMdd"));
                if (!Directory.Exists(temp_csv_path))
                {
                    Directory.CreateDirectory(temp_csv_path);
                }

                string temp_csv_full_path = temp_csv_path + string.Format(@"\{0}_cp_point.csv", file_id.ToString());
                File.WriteAllText(temp_csv_full_path, sb.ToString(), new UTF8Encoding(false));

                progress_msg = string.Format("生成 cp_point csv 文件成功，路径 ： {0}", temp_csv_full_path);
                Log4netHelper.Info(this.GetType(), progress_msg);
                this.Invoke(new Action(() =>
                {
                    this.panelProgress.Visible = true;
                    this.txtProgress.Text = string.Format("{0}, {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), progress_msg);
                }));

                related_table = "cp_point";
                int ret_bulk = 0;
                using (MySqlConnection conn = new MySqlConnection(GlobalVaries.conn_dpat))
                {
                    conn.Open();
                    MySqlBulkLoader bulk = new MySqlBulkLoader(conn)
                    {
                        FieldTerminator = ",",//这个地方字段间的间隔方式，为逗号
                        FieldQuotationCharacter = '"',
                        EscapeCharacter = '"',
                        LineTerminator = "\r\n",//每行
                        Local = true,
                        FileName = temp_csv_full_path,//文件地址
                        NumberOfLinesToSkip = 0,
                        TableName = related_table,
                        CharacterSet = "utf8"
                    };
                    ret_bulk = bulk.Load();
                }

                if (ret_bulk != cp_point_list.Count)
                {
                    throw new Exception("批量插入 cp_point 数据失败！bulk ret :" + ret_bulk.ToString());
                }

                progress_msg = string.Format("批量插入 cp_point 成功！");
                Log4netHelper.Info(this.GetType(), progress_msg);
                this.Invoke(new Action(() =>
                {
                    this.panelProgress.Visible = true;
                    this.txtProgress.Text = string.Format("{0}, {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), progress_msg);
                }));

                //try
                //{
                //    Action act = () => { File.Delete(temp_csv_full_path); };
                //    act.BeginInvoke(null, null);
                //}
                //catch (Exception e)
                //{
                //    Log4netHelper.Error(this.GetType(), e);
                //}

                //4、cp_point_item
                List<CpPointItemClass> cp_point_item_list = new List<CpPointItemClass>();
                for (int i = waferCpForm.cp_point_first_row_index; i <= rows_cnt; i++)
                {
                    string guid = Guid.NewGuid().ToString("N");

                    for (int j = 0, jlen = cp_item_list.Count; j < jlen; j++)
                    {
                        int col_index = j + waferCpForm.cp_item_first_col_index;
                        string cell_value = sheet.Range[i, col_index].Value.Trim();

                        decimal value = 0M;
                        if (!decimal.TryParse(cell_value, out value))
                        {
                            throw new Exception(string.Format("第{0}行第{1}列的测试值错误！必须为Decimal类型！", i, col_index));
                        }

                        int sub_bin_code = 1;
                        if (value < cp_item_list[j].lsl || value > cp_item_list[j].usl)
                        {
                            sub_bin_code = cp_item_list[j].item_no + 1;
                        }

                        cp_point_item_list.Add(new CpPointItemClass()
                        {
                            cp_point = cp_point_list[i - waferCpForm.cp_point_first_row_index],
                            cp_item = cp_item_list[j],

                            value = value,
                            sub_bin_code = sub_bin_code
                        });
                    }
                }

                progress_msg = string.Format("获取 cp_point_item 完毕，cp point item cnt : {0}", cp_point_item_list.Count);
                Log4netHelper.Info(this.GetType(), progress_msg);
                this.Invoke(new Action(() =>
                {
                    this.panelProgress.Visible = true;
                    this.txtProgress.Text = string.Format("{0}, {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), progress_msg);
                }));

                //获取 cp_point_item 最大的 id
                long max_id = 0;

                sql = "select max(id) max_id from cp_point_item";
                object max_id_obj = GlobalVaries.mySqlHelper_dpat.ExecuteScalar(sql);

                if (max_id_obj != null && max_id_obj != DBNull.Value)
                {
                    max_id = long.Parse(max_id_obj.ToString());
                }

                sb = new StringBuilder();
                for (int i = 0, len = cp_point_item_list.Count; i < len; i++)
                {
                    CpPointItemClass cp_point_item = cp_point_item_list[i];

                    sb.Append((max_id + i + 1).ToString() + ",");
                    //sb.Append(datetime + ",");
                    //sb.Append(date + ",");
                    //sb.Append("0" + ",");
                    //sb.Append(year.ToString() + ",");
                    //sb.Append(month.ToString() + ",");
                    //sb.Append(day.ToString() + ",");

                    sb.Append(file_id.ToString() + ",");

                    sb.Append(cp_point_item.cp_point.number.ToString() + ",");
                    sb.Append(cp_point_item.cp_point.x_coord.ToString() + ",");
                    sb.Append(cp_point_item.cp_point.y_coord.ToString() + ",");
                    sb.Append(cp_point_item.cp_point.site_num.ToString() + ",");
                    sb.Append(cp_point_item.cp_point.bin_code.ToString() + ",");

                    sb.Append(cp_point_item.cp_item.test_no.ToString() + ",");
                    sb.Append(cp_point_item.cp_item.test_name + ",");
                    sb.Append(cp_point_item.cp_item.usl.ToString() + ",");
                    sb.Append(cp_point_item.cp_item.lsl.ToString() + ",");
                    sb.Append(cp_point_item.cp_item.unit + ",");
                    sb.Append(cp_point_item.cp_item.item_no.ToString() + ",");
                    sb.Append(cp_point_item.cp_item.test_type + ",");

                    sb.Append(cp_point_item.value.ToString() + ",");
                    sb.Append(cp_point_item.sub_bin_code.ToString());

                    //sb.Append(Guid.NewGuid().ToString("N"));

                    sb.AppendLine();
                }

                temp_csv_path = GlobalVaries.temp_csv_path + string.Format(@"\{0}", now.ToString("yyyyMMdd"));
                if (!Directory.Exists(temp_csv_path))
                {
                    Directory.CreateDirectory(temp_csv_path);
                }

                temp_csv_full_path = temp_csv_path + string.Format(@"\{0}_cp_point_item.csv", file_id.ToString());
                File.WriteAllText(temp_csv_full_path, sb.ToString(), new UTF8Encoding(false));

                progress_msg = string.Format("生成 cp_point_item csv 文件成功，路径 ： {0}", temp_csv_full_path);
                Log4netHelper.Info(this.GetType(), progress_msg);
                this.Invoke(new Action(() =>
                {
                    this.panelProgress.Visible = true;
                    this.txtProgress.Text = string.Format("{0}, {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), progress_msg);
                }));

                related_table = "cp_point_item";
                ret_bulk = 0;
                using (MySqlConnection conn = new MySqlConnection(GlobalVaries.conn_dpat))
                {
                    conn.Open();
                    MySqlBulkLoader bulk = new MySqlBulkLoader(conn)
                    {
                        FieldTerminator = ",",//这个地方字段间的间隔方式，为逗号
                        FieldQuotationCharacter = '"',
                        EscapeCharacter = '"',
                        LineTerminator = "\r\n",//每行
                        Local = true,
                        FileName = temp_csv_full_path,//文件地址
                        NumberOfLinesToSkip = 0,
                        TableName = related_table,
                        CharacterSet = "utf8"
                    };
                    ret_bulk = bulk.Load();
                }

                if (ret_bulk != cp_point_item_list.Count)
                {
                    throw new Exception("批量插入 cp_point_item 数据失败！bulk ret :" + ret_bulk.ToString());
                }

                //try
                //{
                //    Action act = () => { File.Delete(temp_csv_full_path); };
                //    act.BeginInvoke(null, null);
                //}
                //catch (Exception e)
                //{
                //    Log4netHelper.Error(this.GetType(), e);
                //}

                progress_msg = string.Format("批量插入 cp_point_item 成功！");
                Log4netHelper.Info(this.GetType(), progress_msg);
                this.Invoke(new Action(() =>
                {
                    this.panelProgress.Visible = true;
                    this.txtProgress.Text = string.Format("{0}, {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), progress_msg);
                }));

                ////5、update cp_point_item 的 p_id 和 item_id
                ////5.1 update cp_point_item by cp_point
                //sql = string.Format("update cp_point_item t, cp_point v set t.p_id = v.p_id where t.file_id = v.file_id and t.x_coord = v.x_coord and t.y_coord = v.y_coord and t.p_id = 0 and t.file_id = {0}", file_id);
                //int ret_update = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                //int repeat_update = 0;
                //while (repeat_update < 10)
                //{
                //    if (ret_update != 1)
                //    {
                //        repeat_update++;

                //        Thread.Sleep(300);

                //        ret_update = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);
                //    }
                //    else
                //    {
                //        break;
                //    }
                //}

                //if (ret_update != cp_point_item_list.Count)
                //{
                //    throw new Exception("插入CP测试项数据失败2！执行数据库操作返回值错误！");
                //}

                ////5.2 update cp_point_item by cp_item
                //sql = string.Format("update cp_point_item t, cp_item v set t.item_id = v.item_id where t.file_id = v.file_id and t.test_name = v.test_name and t.item_id = 0 and t.file_id = {0}", file_id);
                //ret_update = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                //repeat_update = 0;
                //while (repeat_update < 10)
                //{
                //    if (ret_update != 1)
                //    {
                //        repeat_update++;

                //        Thread.Sleep(300);

                //        ret_update = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);
                //    }
                //    else
                //    {
                //        break;
                //    }
                //}

                //if (ret_update != cp_point_item_list.Count)
                //{
                //    throw new Exception("插入CP测试项数据失败3！执行数据库操作返回值错误！");
                //}

                //5、检查每个 cp_point 的 bin_code 是否正确
                //

                //6、update cp_file 的 cp_item_cnt, cp_point_cnt, cp_point_item_cnt
                sql = string.Format("update cp_file set cp_item_cnt = {1}, cp_point_cnt = {2}, cp_point_item_cnt = {3}, cp_point_bin1_cnt = {4}, updated_datetime = '{5}' where file_id = {0}",
                    file_id,
                    cp_item_list.Count,
                    cp_point_list.Count,
                    cp_point_item_list.Count,
                    cp_point_bin1_cnt,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                int ret_update = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                int repeat_update = 0;
                while (repeat_update < 10)
                {
                    if (ret_update != 1)
                    {
                        repeat_update++;

                        Thread.Sleep(300);

                        ret_update = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);
                    }
                    else
                    {
                        break;
                    }
                }

                if (ret_update != 1)
                {
                    throw new Exception("插入CP测试项数据失败4！执行数据库操作返回值错误！");
                }

                progress_msg = string.Format("更新 cp_file 的 cp_item_cnt, cp_point_cnt, cp_point_item_cnt 完毕！");
                Log4netHelper.Info(this.GetType(), progress_msg);
                this.Invoke(new Action(() =>
                {
                    this.panelProgress.Visible = true;
                    this.txtProgress.Text = string.Format("{0}, {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), progress_msg);
                }));

            }
            catch (Exception ex)
            {
                Log4netHelper.Error(this.GetType(), ex);

                throw ex;
            }
        }

        public void RunDpat(int file_id)
        {
            string sql = string.Empty;

            try
            {
                //1、获取对应的 cp_file
                sql = string.Format("select * from cp_file where file_id = {0}", file_id);
                DataTable dt_file = GlobalVaries.mySqlHelper_dpat.ExecuteDataTable(sql);

                if (dt_file.Rows.Count != 1)
                {
                    throw new Exception("获取CP文件失败！");
                }

                CpFileClass cp_file = new CpFileClass();
                PropertyInfo[] cp_file_props = typeof(CpFileClass).GetProperties();

                for (int i = 0, len = cp_file_props.Length; i < len; i++)
                {
                    if (dt_file.Rows[0][cp_file_props[i].Name].GetType().Name.ToLower().Equals("DateTime".ToLower()))
                    {
                        cp_file_props[i].SetValue(cp_file, dt_file.Rows[0][cp_file_props[i].Name].ToString());
                    }
                    else
                    {
                        if (dt_file.Rows[0][cp_file_props[i].Name] == null || dt_file.Rows[0][cp_file_props[i].Name] == DBNull.Value)
                        {
                            if (cp_file_props[i].PropertyType.Name.ToLower().IndexOf("Int".ToLower()) >= 0)
                            {
                                cp_file_props[i].SetValue(cp_file, 0);
                            }
                            else
                            {
                                cp_file_props[i].SetValue(cp_file, "");
                            }
                        }
                        else
                        {
                            cp_file_props[i].SetValue(cp_file, dt_file.Rows[0][cp_file_props[i].Name]);
                        }

                    }
                }

                //2、删除原 cp_item 中 test_type = 'dpat' 的记录
                //int cp_point_dpat_bin1_cnt = cp_file.cp_point_dpat_bin1_cnt;
                if (cp_file.cp_point_dpat_bin1_cnt != 0)
                {
                    //说明原本已进行过 DPAT 计算
                    sql = string.Format("delete from cp_item where file_id = {0} and test_type = 'dpat'", file_id);
                    int ret = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                    if (ret == 0)
                    {
                        throw new Exception("删除原DPAT测试项失败！");
                    }
                }

                //3.1、获取 cp_item
                sql = string.Format("select * from cp_item where file_id = {0} and test_type = 'cp' order by item_no", file_id);
                DataTable dt_cp_item = GlobalVaries.mySqlHelper_dpat.ExecuteDataTable(sql);
                if (dt_cp_item.Rows.Count == 0)
                {
                    throw new Exception("获取CP测试项数据失败！");
                }

                Dictionary<string, string> cp_item_test_name_dic = new Dictionary<string, string>();

                for (int i = 0, len = dt_cp_item.Rows.Count; i < len; i++)
                {
                    cp_item_test_name_dic.Add(dt_cp_item.Rows[i]["test_name"].ToString().Trim(), dt_cp_item.Rows[i]["unit"].ToString().Trim());
                }

                //3.2、获取 dpat_item
                //sql = string.Format("select * from dpat_item where state = 1 and customer_product_name = '{0}' and version = '{1}' order by item_no desc",
                //    cp_file.customer_product_name,
                //    cp_file.version);

                sql = string.Format("select * from dpat_item where state = 1 and customer_product_name = '{0}' and version = '{1}' order by bin_code",
                    cp_file.customer_product_name,
                    cp_file.version);
                DataTable dt_dpat_item = GlobalVaries.mySqlHelper_dpat.ExecuteDataTable(sql);
                if (dt_dpat_item.Rows.Count == 0)
                {
                    throw new Exception("未找到对应的DPAT测试项！");
                }

                //Dictionary<string, int> dpat_item_item_no_dic = new Dictionary<string, int>();
                Dictionary<string, decimal> dpat_item_sigma_multiplier_dic = new Dictionary<string, decimal>();

                //3.3、判断DPAT测试项在CP测试项中是否存在
                for (int i = 0, len = dt_dpat_item.Rows.Count; i < len; i++)
                {
                    string dpat_item_test_name = dt_dpat_item.Rows[i]["test_name"].ToString().Trim();
                    //int dpat_item_item_no = int.Parse(dt_dpat_item.Rows[i]["item_no"].ToString());
                    decimal dpat_item_sigma_multiplier = decimal.Parse(dt_dpat_item.Rows[i]["sigma_multiplier"].ToString());
                    if (!cp_item_test_name_dic.ContainsKey(dpat_item_test_name))
                    {
                        throw new Exception(string.Format("DPAT测试项 {0} 未对应具体的 CP测试项！", dt_dpat_item.Rows[i]["test_name"].ToString().Trim()));
                    }

                    //dpat_item_item_no_dic.Add(dpat_item_test_name, dpat_item_item_no);
                    dpat_item_sigma_multiplier_dic.Add(dpat_item_test_name, dpat_item_sigma_multiplier);
                }

                //4、开始 DPAT
                DateTime start_dt = DateTime.Now;

                //重置
                //重置  cp_file 的 cp_point_dpat_bin1_cnt = 0
                sql = string.Format("update cp_file set cp_point_dpat_bin1_cnt = 0 where file_id = {0}", file_id);
                GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                //重置 cp_point 的 dpat_bin_code = 0
                sql = string.Format("update cp_point set dpat_bin_code = 0 where file_id = {0} and bin_code = 1", file_id);
                GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                //删除 cp_item 的 test_type = 'dpat'
                sql = string.Format("delete from cp_item where file_id = {0} and test_type = 'dpat'", file_id);
                GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                //重置 cp_point_item 的 dpat_bin_code = 0
                //sql = string.Format("update cp_point_item set dpat_bin_code = 0, dpat_sub_bin_code = 0 where file_id = {0} and bin_code = 1", file_id);
                //GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                Dictionary<long, int> cp_point_item_to_update_dic = new Dictionary<long, int>();
                for (int i = 0, len = dt_dpat_item.Rows.Count; i < len; i++)
                {
                    string dpat_item_test_name = dt_dpat_item.Rows[i]["test_name"].ToString().Trim();
                    //int item_no = int.Parse(dt_dpat_item.Rows[i]["item_no"].ToString());
                    int dpat_bin_code = int.Parse(dt_dpat_item.Rows[i]["bin_code"].ToString());

                    //4.0 获取该 test_name 对应的 cp_point_item 数据，并在获取前，先重置这些数据的 dpat_bin_code = 0 和 dpat_sub_bin_code = 0
                    //sql = string.Format("update cp_point_item set dpat_sub_bin_code = 0 where file_id = {0} and bin_code = 1 and test_name = '{1}'", 
                    //    file_id,
                    //    dpat_item_test_name);
                    //GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                    //List<string> column_list = new List<string>()
                    //{
                    //    "id", "file_id", "number", "x_coord", "y_coord", "site_num", "bin_code",
                    //    "test_no", "test_name", "usl", "lsl", "unit", "item_no", "test_type", "value"
                    //};
                    List<string> column_list = new List<string>()
                    {
                        "id", "file_id", "number", "bin_code", "test_name", "value"
                    };

                    sql = string.Format("select {2} from cp_point_item where file_id = {0} and bin_code = 1 and test_name = '{1}' order by value",
                        file_id,
                        dpat_item_test_name,
                        string.Join(",", column_list));

                    //Log4netHelper.Info(this.GetType(), sql);

                    DataTable dt = GlobalVaries.mySqlHelper_dpat.ExecuteDataTable(sql);

                    int cnt = dt.Rows.Count;

                    int q1_index = 1;
                    int q2_index = 1;
                    int q3_index = 1;

                    decimal q1_value = 0.0M;
                    decimal q2_value = 0.0M;
                    decimal q3_value = 0.0M;

                    //4.1 计算 Q2
                    if ((cnt + 1) % 2 == 0)
                    {
                        //说明 cnt 是奇数
                        q2_index = (cnt + 1) / 2;
                        q2_value = decimal.Parse(dt.Rows[q2_index - 1]["value"].ToString());
                    }
                    else
                    {
                        //说明 cnt 是偶数
                        q2_index = cnt / 2;
                        //q2_index + 1 - 1
                        q2_value = (decimal.Parse(dt.Rows[q2_index - 1]["value"].ToString()) + decimal.Parse(dt.Rows[q2_index + 1 - 1]["value"].ToString())) * 0.5M;
                    }

                    //4.2 计算Q1
                    if ((cnt + 1) % 4 == 0)
                    {
                        q1_index = (cnt + 1) / 4;
                        q1_value = decimal.Parse(dt.Rows[q1_index - 1]["value"].ToString());
                    }
                    else
                    {
                        decimal temp_index = (cnt + 1) * 1.0M / 4;

                        decimal less_index = Math.Floor(temp_index);
                        decimal more_index = less_index + 1.0M;

                        decimal more_multiplier = temp_index - less_index;
                        decimal less_multiplier = 1.0M - more_multiplier;

                        q1_value = decimal.Parse(dt.Rows[Convert.ToInt32(less_index) - 1]["value"].ToString()) * less_multiplier
                            + decimal.Parse(dt.Rows[Convert.ToInt32(more_index) - 1]["value"].ToString()) * more_multiplier;
                    }

                    //4.3 计算Q3
                    if ((cnt + 1) % 4 == 0)
                    {
                        q3_index = (cnt + 1) * 3 / 4;
                        q3_value = decimal.Parse(dt.Rows[q3_index - 1]["value"].ToString());
                    }
                    else
                    {
                        decimal temp_index = (cnt + 1) * 3 * 1.0M / 4;

                        decimal less_index = Math.Floor(temp_index);
                        decimal more_index = less_index + 1.0M;

                        decimal more_multiplier = temp_index - less_index;
                        decimal less_multiplier = 1.0M - more_multiplier;

                        q3_value = decimal.Parse(dt.Rows[Convert.ToInt32(less_index) - 1]["value"].ToString()) * less_multiplier
                            + decimal.Parse(dt.Rows[Convert.ToInt32(more_index) - 1]["value"].ToString()) * more_multiplier;
                    }

                    //4.4 sigma
                    decimal sigma = (q3_value - q1_value) / 1.35M;

                    decimal dpat_usl = q2_value + dpat_item_sigma_multiplier_dic[dpat_item_test_name] * sigma;
                    decimal dpat_lsl = q2_value - dpat_item_sigma_multiplier_dic[dpat_item_test_name] * sigma;

                    //4.5 插入一条 dpat 的 cp_item 记录
                    DateTime now = DateTime.Now;
                    string datetime = now.ToString("yyyy-MM-dd HH:mm:ss");
                    string date = now.ToString("yyyy-MM-dd");
                    int year = now.Year;
                    int month = now.Month;
                    int day = now.Day;

                    List<ColumnToInsertClass> cp_item_column_list = new List<ColumnToInsertClass>();
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "created_datetime", value = datetime, need_quotation = true });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "created_date", value = date, need_quotation = true });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "created_year", value = year.ToString(), need_quotation = false });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "created_month", value = month.ToString(), need_quotation = false });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "created_day", value = day.ToString(), need_quotation = false });

                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "file_id", value = file_id.ToString(), need_quotation = false });

                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "test_name", value = dpat_item_test_name + "_Sigma" + Convert.ToInt32(dpat_item_sigma_multiplier_dic[dpat_item_test_name]).ToString(), need_quotation = true });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "usl", value = dpat_usl.ToString(), need_quotation = false });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "lsl", value = dpat_lsl.ToString(), need_quotation = false });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "unit", value = cp_item_test_name_dic[dpat_item_test_name], need_quotation = true });
                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "item_no", value = (dt_cp_item.Rows.Count + i + 1).ToString(), need_quotation = false });

                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "test_type", value = "dpat", need_quotation = true });// test_type = cp

                    cp_item_column_list.Add(new ColumnToInsertClass() { label = "guid", value = Guid.NewGuid().ToString("N"), need_quotation = true });


                    List<string> col_label_list = new List<string>();
                    List<string> col_value_para_list = new List<string>();
                    for (int j = 0, jlen = cp_item_column_list.Count; j < jlen; j++)
                    {
                        col_label_list.Add(cp_item_column_list[j].label);

                        if (cp_item_column_list[j].need_quotation)
                        {
                            col_value_para_list.Add(string.Format("'{0}'", cp_item_column_list[j].value.Replace("'", "\'")));
                        }
                        else
                        {
                            col_value_para_list.Add(cp_item_column_list[j].value);
                        }
                    }

                    string related_table = "cp_item";
                    sql = string.Format("insert into {0}({1})values({2})", related_table, string.Join(",", col_label_list.ToArray()), string.Join(",", col_value_para_list.ToArray()));
                    int ret_insert = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                    int repeat_insert = 0;
                    while (repeat_insert < 10)
                    {
                        if (ret_insert != 1)
                        {
                            repeat_insert++;

                            Thread.Sleep(300);

                            ret_insert = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (ret_insert != 1)
                    {
                        throw new Exception(string.Format("插入DPAT测试项 {0} 数据失败！执行数据库操作返回值错误！", dpat_item_test_name));
                    }

                    //4.6 用新的 usl 和 lsl 判断 各个 cp_point_item 的 dpat_sub_bin_code
                    for (int j = 0, jlen = dt.Rows.Count; j < jlen; j++)
                    {
                        DataRow dr = dt.Rows[j];

                        decimal value = decimal.Parse(dr["value"].ToString());

                        int dpat_sub_bin_code = 1;
                        if (value < dpat_lsl || value > dpat_usl)
                        {
                            //dpat_sub_bin_code = item_no + 1;
                            dpat_sub_bin_code = dpat_bin_code;
                        }

                        cp_point_item_to_update_dic.Add(long.Parse(dr["id"].ToString()), dpat_sub_bin_code);
                    }
                }

                //5、批量更新 cp_point_item
                Log4netHelper.Info(this.GetType(), "5、批量更新 cp_point_item");

                int ret_update = 0;

                int cnt_loop = 1;
                int batch_update_size = 10000;//一次更新 10000

                List<string> id_list = new List<string>();
                List<string> when_then_list = new List<string>();

                foreach (var item in cp_point_item_to_update_dic)
                {
                    long id = item.Key;
                    int dpat_sub_bin_code = item.Value;

                    id_list.Add(id.ToString());
                    when_then_list.Add(string.Format(" when {0} then {1} ", id, dpat_sub_bin_code));

                    if (cnt_loop % batch_update_size == 0)
                    {
                        sql = string.Format("update cp_point_item set dpat_sub_bin_code = case id {0} end where id in({1})",
                            string.Join("", when_then_list),
                            string.Join(",", id_list.ToArray()));

                        ret_update = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                        if (ret_update != id_list.Count)
                        {
                            throw new Exception("批量更新 cp_point_item 的 dpat_sub_bin_code 值失败！执行数据库操作返回值错误！");
                        }

                        //重置
                        cnt_loop = 1;
                        id_list = new List<string>();
                        when_then_list = new List<string>();
                    }
                    else
                    {
                        cnt_loop++;
                    }
                }

                if (id_list.Count > 0)
                {
                    sql = string.Format("update cp_point_item set dpat_sub_bin_code = case id {0} end where id in({1})",
                            string.Join("", when_then_list),
                            string.Join(",", id_list.ToArray()));

                    ret_update = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                    if (ret_update != id_list.Count)
                    {
                        throw new Exception("批量更新 cp_point_item 的 dpat_sub_bin_code 值失败！执行数据库操作返回值错误！");
                    }
                }


                //6、批量更新 cp_point 的 dpat_bin_code
                sql = string.Format("update cp_point t set t.dpat_bin_code = (select max(dpat_sub_bin_code) from cp_point_item v where v.file_id = {0} and v.bin_code = 1 and v.number = t.number) where file_id = {0} and bin_code = 1", file_id);

                Log4netHelper.Info(this.GetType(), "6、批量更新 cp_point 的 dpat_bin_code, sql :" + sql);

                ret_update = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                if (ret_update != cp_file.cp_point_bin1_cnt)
                {
                    throw new Exception("批量更新 cp_point 的 dpat_bin_code 值失败！执行数据库操作返回值错误！");
                }

                //7、批量更新 cp_point_item 的 dpat_bin_code
                sql = string.Format("update cp_point_item t, cp_point v set t.dpat_bin_code = v.dpat_bin_code where t.file_id = v.file_id and t.number = v.number and t.file_id = {0} and t.bin_code = 1", file_id); ret_update = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                Log4netHelper.Info(this.GetType(), "7、批量更新 cp_point_item 的 dpat_bin_code, sql :" + sql);

                ret_update = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                if (ret_update != cp_file.cp_point_bin1_cnt * dt_cp_item.Rows.Count)
                {
                    throw new Exception("批量更新 cp_point_item 的 dpat_bin_code 值失败！执行数据库操作返回值错误！");
                }

                //8、更新 cp_file 的 cp_point_dpat_bin1_cnt
                DateTime end_dt = DateTime.Now;
                sql = string.Format("update cp_file t set t.cp_point_dpat_bin1_cnt = (select count(p_id) from cp_point v where v.file_id = {0} and v.dpat_bin_code = 1), cp_point_dpat_start_datetime = '{1}', cp_point_dpat_end_datetime = '{2}', cp_point_dpat_time_span = TIMESTAMPDIFF(SECOND, '{1}', '{2}') where t.file_id = {0}",
                    file_id,
                    start_dt.ToString("yyyy-MM-dd HH:mm:ss"),
                    end_dt.ToString("yyyy-MM-dd HH:mm:ss"));

                Log4netHelper.Info(this.GetType(), "8、更新 cp_file 的 cp_point_dpat_bin1_cnt, sql :" + sql);

                ret_update = GlobalVaries.mySqlHelper_dpat.ExecuteNonQuery(sql);

                if (ret_update != 1)
                {
                    throw new Exception("批量更新 cp_file 的 cp_point_dpat_bin1_cnt 值失败！执行数据库操作返回值错误！");
                }
            }
            catch (Exception ex)
            {
                Log4netHelper.Error(this.GetType(), ex);
                throw ex;
            }
        }
    }
}
