using CefsharpTest.Model;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefsharpTest.Module
{
    public class DpatHelper
    {
        /// <summary>
        /// dpat 测试项：test_name, sigma_multiplier, bin_code
        /// </summary>
        /// <param name="cp_file_full_name"></param>
        /// <param name="dpat_item_list"></param>
        public static void run(string cp_file_full_name, string customer_product_name, string version, List<DpatItemClass> dpat_item_list)
        {
            Log4netHelper.Info(typeof(DpatHelper), "start dpat helper");

            try
            {
                //0
                if (dpat_item_list.Count == 0)
                {
                    throw new Exception("DPAT测试项不能为空");
                }

                Log4netHelper.Info(typeof(DpatHelper), "start 1 cp_file");

                //1、cp_file
                FileInfo fi = new FileInfo(cp_file_full_name);
                if (!fi.Exists)
                {
                    throw new Exception("CP文件不存在");
                }

                long file_size = fi.Length;

                string file_name = Path.GetFileNameWithoutExtension(cp_file_full_name);
                string file_ext = Path.GetExtension(cp_file_full_name);

                string file_guid = Guid.NewGuid().ToString("N");

                string wafer_no = file_name.Substring(0, file_name.IndexOf("_"));

                CpFileClass cp_file = new CpFileClass()
                {
                    file_size = file_size,
                    customer_product_name = customer_product_name,
                    version = version,
                    wafer_no = wafer_no,
                    file_name = file_name,
                    file_ext = file_ext,
                    file_full_path = cp_file_full_name
                };

                Log4netHelper.Info(typeof(DpatHelper), "start 2 cp item");

                //2、cp_item
                //开始读取 表格
                Workbook workbook = new Workbook();
                workbook.LoadFromFile(cp_file_full_name);

                var sheets = workbook.Worksheets;
                var sheet = sheets[0];//0 = waferCpForm.sheet_index

                int rows_cnt = sheet.Rows.Length;
                int cols_cnt = sheet.Columns.Length;

                string cell_value = string.Empty;

                List<int> test_no_list = new List<int>();           //第一行
                List<string> test_name_list = new List<string>();   //第二行
                List<decimal> usl_list = new List<decimal>();       //第三行
                List<decimal> lsl_list = new List<decimal>();       //第四行
                List<string> unit_list = new List<string>();        //第五行

                // 到cp点数据行为止
                for (int i = 1; i < 6; i++)// 6 = waferCpForm.cp_point_first_row_index
                {
                    int col_index = 5; // 5 = waferCpForm.cp_item_first_col_index

                    while (true)
                    {
                        if (sheet.Range[i, col_index].Value == null || string.IsNullOrEmpty(sheet.Range[i, col_index].Value.Trim()))
                        {
                            //若为null或为空，则说明这一行已经读取完毕，则读取下一行
                            break;
                        }

                        cell_value = sheet.Range[i, col_index].Value.Trim();

                        if (i == 1)//1 = waferCpForm.cp_item_test_no_row_index
                        {
                            //test_no
                            int test_no = 0;
                            if (!int.TryParse(cell_value, out test_no))
                            {
                                throw new Exception(string.Format("第{0}行第{1}列的Test No.值错误！必须为Int类型！", i, col_index));
                            }
                            test_no_list.Add(test_no);
                        }

                        if (i == 2)//2 = waferCpForm.cp_item_test_name_row_index
                        {
                            test_name_list.Add(cell_value);
                        }

                        if (i == 3)//3 = waferCpForm.cp_item_usl_row_index
                        {
                            decimal usl = 0M;
                            if (!decimal.TryParse(cell_value, out usl))
                            {
                                throw new Exception(string.Format("第{0}行第{1}列的USL值错误！必须为Decimal类型！", i, col_index));
                            }
                            usl_list.Add(usl);
                        }

                        if (i == 4)//4 = waferCpForm.cp_item_lsl_row_index
                        {
                            decimal lsl = 0M;
                            if (!decimal.TryParse(cell_value, out lsl))
                            {
                                throw new Exception(string.Format("第{0}行第{1}列的LSL值错误！必须为Decimal类型！", i, col_index));
                            }
                            lsl_list.Add(lsl);
                        }

                        if (i == 5)//5 = waferCpForm.cp_item_unit_row_index
                        {
                            unit_list.Add(cell_value);
                        }

                        col_index++;//注意写上
                    }
                }

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

                        //guid = Guid.NewGuid().ToString("N")
                    };
                    cp_item_list.Add(cp_item);

                    if (cp_item_dic.ContainsKey(cp_item.test_name))
                    {
                        throw new Exception(string.Format("发现重复的 Test Name : {0}", cp_item.test_name));
                    }

                    cp_item_dic.Add(cp_item.test_name, cp_item);
                }

                Log4netHelper.Info(typeof(DpatHelper), "start 3 cp point");

                //3、cp_point
                List<CpPointClass> cp_point_list = new List<CpPointClass>();
                int max_x_coord = 0;
                int max_y_coord = 0;

                int cp_point_bin1_cnt = 0;
                for (int i = 6; i <= rows_cnt; i++)// 6 = waferCpForm.cp_point_first_row_index
                {
                    int number = 1;
                    int x_coord = 0;
                    int y_coord = 0;
                    int site_num = 0;
                    int bin_code = 1;
                    //string guid = Guid.NewGuid().ToString("N");

                    int col_index = 1;
                    while (col_index < 5) // 5 = waferCpForm.cp_item_first_col_index
                    {
                        cell_value = sheet.Range[i, col_index].Value.Trim();

                        if (col_index == 1)// 1 = waferCpForm.cp_point_number_col_index
                        {
                            if (!int.TryParse(cell_value, out number))
                            {
                                throw new Exception(string.Format("第{0}行第{1}列的 Number 值错误！必须为Int类型！", i, col_index));
                            }
                        }

                        if (col_index == 2)// 2 = waferCpForm.cp_point_x_coord_col_index
                        {
                            if (!int.TryParse(cell_value, out x_coord))
                            {
                                throw new Exception(string.Format("第{0}行第{1}列的 X_Coord 值错误！必须为Int类型！", i, col_index));
                            }
                        }

                        if (col_index == 3)// 3 = waferCpForm.cp_point_y_coord_col_index
                        {
                            if (!int.TryParse(cell_value, out y_coord))
                            {
                                throw new Exception(string.Format("第{0}行第{1}列的 Y_Coord 值错误！必须为Int类型！", i, col_index));
                            }
                        }

                        if (col_index == 4)// 4 = waferCpForm.cp_point_site_num_col_index
                        {
                            if (!int.TryParse(cell_value, out site_num))
                            {
                                throw new Exception(string.Format("第{0}行第{1}列的 SiteNum 值错误！必须为Int类型！", i, col_index));
                            }
                        }
                        col_index++;
                    }

                    //bin_code列要单独处理
                    //if (waferCpForm.cp_point_bin_code_col_index == 0)
                    //{
                    cell_value = sheet.Range[i, cols_cnt].Value.Trim();

                    if (!int.TryParse(cell_value, out bin_code))
                    {
                        throw new Exception(string.Format("第{0}行第{1}列的 BinCode 值错误！必须为Int类型！", i, cols_cnt));
                    }
                    //}

                    if (x_coord > max_x_coord)
                    {
                        max_x_coord = x_coord;
                    }

                    if (y_coord > max_y_coord)
                    {
                        max_y_coord = y_coord;
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

                GlobalVaries.dpatForm.ClearMemory();

                Log4netHelper.Info(typeof(DpatHelper), "start 4 cp point item");

                //4、cp_point_item
                Dictionary<string, decimal> dpat_item_sigma_multiplier_dic = new Dictionary<string, decimal>();
                Dictionary<string, List<CpPointItemClass>> dpat_item_cp_point_items_dic = new Dictionary<string, List<CpPointItemClass>>();
                for (int i = 0, len = dpat_item_list.Count; i < len; i++)
                {
                    DpatItemClass dpat_item = dpat_item_list[i];

                    string dpat_item_test_name = dpat_item.test_name;

                    decimal dpat_item_sigma_multiplier = decimal.Parse(dpat_item.sigma_multiplier.ToString());
                    if (!cp_item_dic.ContainsKey(dpat_item_test_name))
                    {
                        throw new Exception(string.Format("DPAT测试项 {0} 未对应具体的 CP测试项！", dpat_item_test_name));
                    }

                    //dpat_item_item_no_dic.Add(dpat_item_test_name, dpat_item_item_no);
                    dpat_item_sigma_multiplier_dic.Add(dpat_item_test_name, dpat_item_sigma_multiplier);

                    dpat_item_cp_point_items_dic.Add(dpat_item_test_name, new List<CpPointItemClass>());
                }

                List<CpPointItemClass> cp_point_item_list = new List<CpPointItemClass>();
                for (int i = 6; i <= rows_cnt; i++)// 6 = waferCpForm.cp_point_first_row_index
                {
                    string guid = Guid.NewGuid().ToString("N");

                    for (int j = 0, jlen = cp_item_list.Count; j < jlen; j++)
                    {
                        int col_index = j + 5;// 5 = waferCpForm.cp_item_first_col_index
                        cell_value = sheet.Range[i, col_index].Value.Trim();

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
                            //cp_point = cp_point_list[i - 6],// 6 = waferCpForm.cp_point_first_row_index
                            cp_item = cp_item_list[j],

                            value = value,
                            sub_bin_code = sub_bin_code
                        });

                        cp_point_list[i - 6].cp_point_item_dic.Add(cp_item_list[j].test_name, new CpPointItemClass()
                        {
                            //cp_point = cp_point_list[i - 6],// 6 = waferCpForm.cp_point_first_row_index
                            //cp_item = cp_item_list[j],
                            test_name = cp_item_list[j].test_name,

                            value = value,
                            sub_bin_code = sub_bin_code
                        });

                        //注意 cp_point_list[i - 6]
                        if (cp_point_list[i - 6].bin_code == 1 && dpat_item_cp_point_items_dic.ContainsKey(cp_item_list[j].test_name))
                        {
                            dpat_item_cp_point_items_dic[cp_item_list[j].test_name].Add(new CpPointItemClass()
                            {
                                //cp_point = cp_point_list[i - 6],// 6 = waferCpForm.cp_point_first_row_index
                                //cp_item = cp_item_list[j],

                                value = value,
                                //sub_bin_code = sub_bin_code
                            });
                        }
                    }
                }

                GlobalVaries.dpatForm.ClearMemory();

                cp_file.cp_item_cnt = cp_item_list.Count;
                cp_file.cp_point_cnt = cp_point_list.Count;
                cp_file.cp_point_item_cnt = cp_point_item_list.Count;
                cp_file.cp_point_bin1_cnt = cp_point_bin1_cnt;

                //5、run dpat
                DateTime start_dpat_dt = DateTime.Now;

                Log4netHelper.Info(typeof(DpatHelper), "start 5 run dpat");

                //Dictionary<long, int> cp_point_item_to_update_dic = new Dictionary<long, int>();

                for (int i = 0, len = dpat_item_list.Count; i < len; i++)
                {
                    DpatItemClass dpat_item = dpat_item_list[i];

                    string test_name = dpat_item.test_name;
                    decimal dpat_item_sigma_multiplier = dpat_item.sigma_multiplier;
                    int dpat_bin_code = dpat_item.bin_code;

                    List<CpPointItemClass> cell_list = dpat_item_cp_point_items_dic[test_name];

                    // 获取 bin_code = 1 的 该 dpat_item_test_name 的 cp_point_item 的数据集
                    //int cnt = dpat_item_cp_point_items_dic[dpat_item_test_name].Count;

                    //先排序
                    cell_list.Sort((a, b) => a.value.CompareTo(b.value));

                    int cnt = cell_list.Count;

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
                        q2_value = cell_list[q2_index - 1].value;
                    }
                    else
                    {
                        //说明 cnt 是偶数
                        q2_index = cnt / 2;
                        //q2_index + 1 - 1
                        q2_value = (cell_list[q2_index - 1].value + cell_list[q2_index + 1 - 1].value) * 0.5M;
                    }

                    //4.2 计算Q1
                    if ((cnt + 1) % 4 == 0)
                    {
                        q1_index = (cnt + 1) / 4;
                        q1_value = cell_list[q1_index - 1].value;
                    }
                    else
                    {
                        decimal temp_index = (cnt + 1) * 1.0M / 4;

                        decimal less_index = Math.Floor(temp_index);
                        decimal more_index = less_index + 1.0M;

                        decimal more_multiplier = temp_index - less_index;
                        decimal less_multiplier = 1.0M - more_multiplier;

                        q1_value = cell_list[Convert.ToInt32(less_index) - 1].value * less_multiplier
                            + cell_list[Convert.ToInt32(more_index) - 1].value * more_multiplier;
                    }

                    //4.3 计算Q3
                    if ((cnt + 1) % 4 == 0)
                    {
                        q3_index = (cnt + 1) * 3 / 4;
                        q3_value = cell_list[q3_index - 1].value;
                    }
                    else
                    {
                        decimal temp_index = (cnt + 1) * 3 * 1.0M / 4;

                        decimal less_index = Math.Floor(temp_index);
                        decimal more_index = less_index + 1.0M;

                        decimal more_multiplier = temp_index - less_index;
                        decimal less_multiplier = 1.0M - more_multiplier;

                        q3_value = cell_list[Convert.ToInt32(less_index) - 1].value * less_multiplier
                            + cell_list[Convert.ToInt32(more_index) - 1].value * more_multiplier;
                    }

                    //4.4 sigma
                    decimal sigma = (q3_value - q1_value) / 1.35M;

                    decimal dpat_usl = q2_value + dpat_item_sigma_multiplier_dic[test_name] * sigma;
                    decimal dpat_lsl = q2_value - dpat_item_sigma_multiplier_dic[test_name] * sigma;

                    string dpat_item_test_name = test_name + "_Sigma" + Convert.ToInt32(dpat_item_sigma_multiplier_dic[test_name]).ToString();
                    CpItemClass cp_item = new CpItemClass()
                    {
                        test_no = cp_item_list.Count + i + 1,
                        test_name = dpat_item_test_name,
                        usl = dpat_usl,
                        lsl = dpat_lsl,
                        unit = cp_item_dic[test_name].unit,
                        item_no = cp_item_list.Count + i + 1,
                        test_type = "dpat"
                    };

                    cp_item_list.Add(cp_item);


                    Log4netHelper.Info(typeof(DpatHelper), "start 5.1");

                    for (int j = 0, jlen = cp_point_list.Count; j < jlen; j++)
                    {
                        decimal value = cp_point_list[j].cp_point_item_dic[test_name].value;
                        int sub_bin_code = cp_point_list[j].cp_point_item_dic[test_name].sub_bin_code;

                        int dpat_sub_bin_code = 1;
                        if (value < dpat_lsl || value > dpat_usl)
                        {
                            //dpat_sub_bin_code = item_no + 1;
                            dpat_sub_bin_code = dpat_bin_code;
                        }

                        cp_point_list[j].cp_point_item_dic.Add(
                            dpat_item_test_name,
                            new CpPointItemClass()
                            {
                                //cp_item = cp_item,
                                test_name = cp_item.test_name,

                                value = value,
                                sub_bin_code = sub_bin_code,

                                dpat_sub_bin_code = dpat_sub_bin_code
                            });
                    }

                    GlobalVaries.dpatForm.ClearMemory();
                }

                Log4netHelper.Info(typeof(DpatHelper), "start 6 set dpat_bin_code");

                //6、判断 各个 cp_point 的 dpat_bin_code
                int cp_point_dpat_bin1_cnt = 0;
                for (int i = 0, len = cp_point_list.Count; i < len; i++)
                {
                    int max_dpat_bin_code = 0;
                    if (cp_point_list[i].bin_code == 1)
                    {
                        foreach (var cp_point_item in cp_point_list[i].cp_point_item_dic)
                        {
                            if (max_dpat_bin_code < cp_point_item.Value.dpat_sub_bin_code)
                            {
                                max_dpat_bin_code = cp_point_item.Value.dpat_sub_bin_code;
                            }
                        }
                    }

                    cp_point_list[i].dpat_bin_code = max_dpat_bin_code;

                    if (max_dpat_bin_code == 1)
                    {
                        cp_point_dpat_bin1_cnt++;
                    }
                }

                GlobalVaries.dpatForm.ClearMemory();

                Log4netHelper.Info(typeof(DpatHelper), "start 7 generate xlsx");

                //7、生成表格
                //4个 sheet
                //CP raw data
                //CP Map
                //DPAT data
                //DPAT Map

                Workbook cp_dpat_workbook = new Workbook();
                //cp_dpat_workbook.CreateEmptySheet("CP Raw Data");
                //Worksheet cp_raw_data_sheet = cp_dpat_workbook.Worksheets[0];


                #region CP Raw Data
                Worksheet cp_raw_data_sheet = cp_dpat_workbook.Worksheets[0];
                cp_raw_data_sheet.Name = "CP Raw Data";

                //CellStyle cell_style = cp_dpat_workbook.Styles.Add("Style");
                //cell_style.Font.Size = 11;
                //cell_style.Font.FontName = "宋体";
                //cp_raw_data_sheet.ApplyStyle(cell_style);


                //CellRange cell_range =  cp_raw_data_sheet.Range[1, 1, 5 + max_y_coord, 4 + max_x_coord + 1];
                //cell_range.Style.Font.Size = 11;
                //cell_range.Style.Font.FontName = "宋体";

                //第一行 test no
                int row_index = 1;
                cp_raw_data_sheet.Range[row_index, 2].Text = "Test No.";

                //5 = waferCpForm.cp_item_first_col_index
                for (int i = 0, len = cp_item_list.Count; i < len; i++)
                {
                    if (cp_item_list[i].test_type.Equals("cp"))
                    {
                        cp_raw_data_sheet.Range[row_index, i + 5].Text = cp_item_list[i].test_no.ToString();
                    }
                }

                //第二行 test name
                row_index = 2;
                cp_raw_data_sheet.Range[row_index, 2].Text = "Test Name";

                //5 = waferCpForm.cp_item_first_col_index
                for (int i = 0, len = cp_item_list.Count; i < len; i++)
                {
                    if (cp_item_list[i].test_type.Equals("cp"))
                    {
                        cp_raw_data_sheet.Range[row_index, i + 5].Text = cp_item_list[i].test_name;
                        cp_raw_data_sheet.Range[row_index, i + 5].Style.WrapText = true;
                    }
                }

                // 第三行 usl
                row_index = 3;
                cp_raw_data_sheet.Range[row_index, 2].Text = "High Limit(USL)";

                //5 = waferCpForm.cp_item_first_col_index
                for (int i = 0, len = cp_item_list.Count; i < len; i++)
                {
                    if (cp_item_list[i].test_type.Equals("cp"))
                    {
                        cp_raw_data_sheet.Range[row_index, i + 5].Text = cp_item_list[i].usl.ToString();
                    }
                }

                // 第四行 lsl
                row_index = 4;
                cp_raw_data_sheet.Range[row_index, 2].Text = "Low Limit(LSL)";

                //5 = waferCpForm.cp_item_first_col_index
                for (int i = 0, len = cp_item_list.Count; i < len; i++)
                {
                    if (cp_item_list[i].test_type.Equals("cp"))
                    {
                        cp_raw_data_sheet.Range[row_index, i + 5].Text = cp_item_list[i].lsl.ToString();
                    }
                }

                //第五行 
                row_index = 5;
                cp_raw_data_sheet.Range[row_index, 1].Text = "Number";
                cp_raw_data_sheet.Range[row_index, 2].Text = "X_Coord";
                cp_raw_data_sheet.Range[row_index, 3].Text = "Y_Coord";
                cp_raw_data_sheet.Range[row_index, 4].Text = "SiteNum";

                //5 = waferCpForm.cp_item_first_col_index
                for (int i = 0, len = cp_item_list.Count; i < len; i++)
                {
                    if (cp_item_list[i].test_type.Equals("cp"))
                    {
                        cp_raw_data_sheet.Range[row_index, i + 5].Text = cp_item_list[i].unit;
                    }
                }

                //cp_raw_data_sheet.Range[row_index, 5 + cp_item_list.Count].Text = "BinCode";
                cp_raw_data_sheet.Range[row_index, cols_cnt].Text = "BinCode";

                //从第6行开始，写入具体数值
                row_index = 6;

                //5 = waferCpForm.cp_item_first_col_index
                for (int i = 0, len = cp_point_list.Count; i < len; i++)
                {
                    CpPointClass cp_point = cp_point_list[i];

                    cp_raw_data_sheet.Range[i + row_index, 1].Text = cp_point.number.ToString();
                    cp_raw_data_sheet.Range[i + row_index, 2].Text = cp_point.x_coord.ToString();
                    cp_raw_data_sheet.Range[i + row_index, 3].Text = cp_point.y_coord.ToString();
                    cp_raw_data_sheet.Range[i + row_index, 4].Text = cp_point.site_num.ToString();

                    for (int j = 0, jlen = cp_item_list.Count; j < jlen; j++)
                    {
                        if (cp_item_list[j].test_type.Equals("cp"))
                        {
                            cp_raw_data_sheet.Range[i + row_index, j + 5].Text = cp_point.cp_point_item_dic[cp_item_list[j].test_name].value.ToString();
                        }
                    }

                    //cp_raw_data_sheet.Range[i + row_index, 5 + cp_item_list.Count].Text = cp_point.bin_code.ToString();
                    cp_raw_data_sheet.Range[i + row_index, cols_cnt].Text = cp_point.bin_code.ToString();
                }

                //设置整体的列宽、行高、字体和字体大小
                double columnWidth = 8.38;//
                ///for (int i = 1, len = 4 + cols_cnt; i <= len; i++)
                for (int i = 0, len = cols_cnt; i < len; i++)
                {
                    cp_raw_data_sheet.Columns[i].ColumnWidth = columnWidth;
                }

                double rowHeight = 13.5;//
                for (int i = 0, len = 5 + max_y_coord; i < len; i++)
                {
                    cp_raw_data_sheet.Rows[i].RowHeight = rowHeight;
                }

                cp_raw_data_sheet.Range[1, 1, 5 + cp_point_list.Count, cols_cnt].ConvertToNumber();
                //cp_raw_data_sheet.Range[1, 1, 5 + max_y_coord, 4 + max_x_coord + 1].Style.HorizontalAlignment = HorizontalAlignType.Center;
                //cp_raw_data_sheet.Range[1, 1, 5 + max_y_coord, 4 + max_x_coord + 1].Style.VerticalAlignment = VerticalAlignType.Center;

                #endregion

                #region CP Map

                Worksheet cp_map_sheet = cp_dpat_workbook.Worksheets[1];
                cp_map_sheet.Name = "CP Map";

                //第1,2,3,4行
                //row_index = 1;
                for (int i = 1; i <= max_x_coord; i++)
                {
                    cp_map_sheet.Range[1, i + 4].Text = "+";

                    string str = string.Format("{0:d3}", i);

                    cp_map_sheet.Range[2, i + 4].Text = str.Substring(0, 1);
                    cp_map_sheet.Range[3, i + 4].Text = str.Substring(1, 1);
                    cp_map_sheet.Range[4, i + 4].Text = str.Substring(2, 1);
                }

                //第1,2,3,4列，从第5行开始
                //row_index = 5;

                for (int i = 1; i <= max_y_coord; i++)
                {
                    cp_map_sheet.Range[i + 4, 1].Text = "+";

                    string str = string.Format("{0:d3}", i);

                    cp_map_sheet.Range[i + 4, 2].Text = str.Substring(0, 1);
                    cp_map_sheet.Range[i + 4, 3].Text = str.Substring(1, 1);
                    cp_map_sheet.Range[i + 4, 4].Text = str.Substring(2, 1);
                }

                //开始填充
                for (int i = 0, len = cp_point_list.Count; i < len; i++)
                {
                    CellRange cell_range = cp_map_sheet.Range[cp_point_list[i].y_coord + 4, cp_point_list[i].x_coord + 4];
                    cell_range.Text = cp_point_list[i].bin_code.ToString();
                    if (cp_point_list[i].bin_code == 1)
                    {
                        cell_range.Style.KnownColor = ExcelColors.Green;
                        cell_range.ConvertToNumber();
                    }
                    else
                    {
                        cell_range.Style.KnownColor = ExcelColors.Red;
                    }
                }

                columnWidth = 2;
                for (int i = 0, len = 4 + max_x_coord; i < len; i++)
                {
                    cp_map_sheet.Columns[i].ColumnWidth = columnWidth;
                }

                cp_map_sheet.Range[1, 1, 4, 4 + max_x_coord].ConvertToNumber();
                cp_map_sheet.Range[1, 1, 4 + max_y_coord, 4].ConvertToNumber();

                cp_map_sheet.Range[1, 1, 4 + max_y_coord, 4 + max_x_coord].Style.HorizontalAlignment = HorizontalAlignType.Center;
                cp_map_sheet.Range[1, 1, 4 + max_y_coord, 4 + max_x_coord].Style.VerticalAlignment = VerticalAlignType.Center;


                #endregion

                #region DPAT Data
                Worksheet dpat_data_sheet = cp_dpat_workbook.Worksheets[2];
                dpat_data_sheet.Name = "DPAT Data";

                //第一行 test no
                row_index = 1;
                dpat_data_sheet.Range[row_index, 2].Text = "Test No.";

                //5 = waferCpForm.cp_item_first_col_index
                for (int i = 0, len = cp_item_list.Count; i < len; i++)
                {
                    if (cp_item_list[i].test_type.Equals("cp"))
                    {
                        dpat_data_sheet.Range[row_index, i + 5].Text = cp_item_list[i].test_no.ToString();
                    }
                }

                //第二行 test name
                row_index = 2;
                dpat_data_sheet.Range[row_index, 2].Text = "Test Name";

                //5 = waferCpForm.cp_item_first_col_index
                for (int i = 0, len = cp_item_list.Count; i < len; i++)
                {
                    //if (cp_item_list[i].test_type.Equals("cp"))
                    //{
                    dpat_data_sheet.Range[row_index, i + 5].Text = cp_item_list[i].test_name;
                    dpat_data_sheet.Range[row_index, i + 5].Style.WrapText = true;
                    //}
                }

                // 第三行 usl
                row_index = 3;
                dpat_data_sheet.Range[row_index, 2].Text = "High Limit(USL)";

                //5 = waferCpForm.cp_item_first_col_index
                for (int i = 0, len = cp_item_list.Count; i < len; i++)
                {
                    //if (cp_item_list[i].test_type.Equals("cp"))
                    //{
                    dpat_data_sheet.Range[row_index, i + 5].Text = cp_item_list[i].usl.ToString();
                    //}
                }

                // 第四行 lsl
                row_index = 4;
                dpat_data_sheet.Range[row_index, 2].Text = "Low Limit(LSL)";

                //5 = waferCpForm.cp_item_first_col_index
                for (int i = 0, len = cp_item_list.Count; i < len; i++)
                {
                    //if (cp_item_list[i].test_type.Equals("cp"))
                    //{
                    dpat_data_sheet.Range[row_index, i + 5].Text = cp_item_list[i].lsl.ToString();
                    //}
                }

                //第五行 
                row_index = 5;
                dpat_data_sheet.Range[row_index, 1].Text = "Number";
                dpat_data_sheet.Range[row_index, 2].Text = "X_Coord";
                dpat_data_sheet.Range[row_index, 3].Text = "Y_Coord";
                dpat_data_sheet.Range[row_index, 4].Text = "SiteNum";

                //5 = waferCpForm.cp_item_first_col_index
                for (int i = 0, len = cp_item_list.Count; i < len; i++)
                {
                    //if (cp_item_list[i].test_type.Equals("cp"))
                    //{
                    dpat_data_sheet.Range[row_index, i + 5].Text = cp_item_list[i].unit;
                    //}
                }

                //dpat_data_sheet.Range[row_index, 5 + cp_item_list.Count].Text = "BinCode";
                dpat_data_sheet.Range[row_index, 4 + cp_item_list.Count + 1].Text = "BinCode";

                //从第6行开始，写入具体数值
                row_index = 6;

                //5 = waferCpForm.cp_item_first_col_index
                for (int i = 0, len = cp_point_list.Count; i < len; i++)
                {
                    CpPointClass cp_point = cp_point_list[i];

                    dpat_data_sheet.Range[i + row_index, 1].Text = cp_point.number.ToString();
                    dpat_data_sheet.Range[i + row_index, 2].Text = cp_point.x_coord.ToString();
                    dpat_data_sheet.Range[i + row_index, 3].Text = cp_point.y_coord.ToString();
                    dpat_data_sheet.Range[i + row_index, 4].Text = cp_point.site_num.ToString();

                    for (int j = 0, jlen = cp_item_list.Count; j < jlen; j++)
                    {
                        //if (cp_item_list[j].test_type.Equals("cp"))
                        //{
                        dpat_data_sheet.Range[i + row_index, j + 5].Text = cp_point.cp_point_item_dic[cp_item_list[j].test_name].value.ToString();
                        //}
                    }

                    //dpat_data_sheet.Range[i + row_index, 5 + cp_item_list.Count].Text = cp_point.bin_code.ToString();
                    if(cp_point.dpat_bin_code == 0)
                    {
                        //说明没有参与 DPAT 计算，即原本就是 bin_code 不为 1
                        dpat_data_sheet.Range[i + row_index, 4 + cp_item_list.Count + 1].Text = cp_point.bin_code.ToString();
                    }
                    else
                    {
                        dpat_data_sheet.Range[i + row_index, 4 + cp_item_list.Count + 1].Text = cp_point.dpat_bin_code.ToString();
                    }
                }

                //设置整体的列宽、行高、字体和字体大小
                columnWidth = 8.38;//
                ///for (int i = 1, len = 4 + cols_cnt; i <= len; i++)
                for (int i = 0, len = cols_cnt; i < len; i++)
                {
                    dpat_data_sheet.Columns[i].ColumnWidth = columnWidth;
                }

                rowHeight = 13.5;//
                for (int i = 0, len = 5 + max_y_coord; i < len; i++)
                {
                    dpat_data_sheet.Rows[i].RowHeight = rowHeight;
                }

                dpat_data_sheet.Range[1, 1, 5 + cp_point_list.Count, 4 + cp_item_list.Count + 1].ConvertToNumber();

                #endregion

                #region DPAT Map

                cp_dpat_workbook.CreateEmptySheet("DPAT Map");
                Worksheet dpat_map_sheet = cp_dpat_workbook.Worksheets[cp_dpat_workbook.Worksheets.Count - 1];

                //第1,2,3,4行
                //row_index = 1;
                for (int i = 1; i <= max_x_coord; i++)
                {
                    dpat_map_sheet.Range[1, i + 4].Text = "+";

                    string str = string.Format("{0:d3}", i);

                    dpat_map_sheet.Range[2, i + 4].Text = str.Substring(0, 1);
                    dpat_map_sheet.Range[3, i + 4].Text = str.Substring(1, 1);
                    dpat_map_sheet.Range[4, i + 4].Text = str.Substring(2, 1);
                }

                //第1,2,3,4列，从第5行开始
                //row_index = 5;

                for (int i = 1; i <= max_y_coord; i++)
                {
                    dpat_map_sheet.Range[i + 4, 1].Text = "+";

                    string str = string.Format("{0:d3}", i);

                    dpat_map_sheet.Range[i + 4, 2].Text = str.Substring(0, 1);
                    dpat_map_sheet.Range[i + 4, 3].Text = str.Substring(1, 1);
                    dpat_map_sheet.Range[i + 4, 4].Text = str.Substring(2, 1);
                }

                //开始填充
                for (int i = 0, len = cp_point_list.Count; i < len; i++)
                {
                    CpPointClass cp_point = cp_point_list[i];

                    CellRange cell_range = dpat_map_sheet.Range[cp_point.y_coord + 4, cp_point.x_coord + 4];

                    if (cp_point.dpat_bin_code == 0)
                    {
                        //说明没有参与 DPAT 计算，即原本就是 bin_code 不为 1
                        cell_range.Text = cp_point.bin_code.ToString();
                        cell_range.Style.KnownColor = ExcelColors.Red;
                    }
                    else if(cp_point.dpat_bin_code == 1)
                    {
                        cell_range.Text = "1";
                        cell_range.Style.KnownColor = ExcelColors.Green;
                        cell_range.ConvertToNumber();
                    }
                    else
                    {
                        //dpat 即不为 0， 也不为 1，那么就是新增的 bin_code
                        cell_range.Text = cp_point.dpat_bin_code.ToString();
                        cell_range.Style.KnownColor = ExcelColors.Yellow;
                    }
                }

                columnWidth = 2;
                for (int i = 0, len = 4 + max_x_coord; i < len; i++)
                {
                    dpat_map_sheet.Columns[i].ColumnWidth = columnWidth;
                }

                dpat_map_sheet.Range[1, 1, 4, 4 + max_x_coord].ConvertToNumber();
                dpat_map_sheet.Range[1, 1, 4 + max_y_coord, 4].ConvertToNumber();

                dpat_map_sheet.Range[1, 1, 4 + max_y_coord, 4 + max_x_coord].Style.HorizontalAlignment = HorizontalAlignType.Center;
                dpat_map_sheet.Range[1, 1, 4 + max_y_coord, 4 + max_x_coord].Style.VerticalAlignment = VerticalAlignType.Center;

                #endregion

                cp_dpat_workbook.SaveToFile(string.Format(@"D:\DPAT\20220614\cp_dpat_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss")), FileFormat.Version2007);

                Log4netHelper.Info(typeof(DpatHelper), "start end");
            }
            catch (Exception ex)
            {
                Log4netHelper.Error(typeof(DpatHelper), ex);
            }
        }
    }
}
