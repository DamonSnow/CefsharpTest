using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CefsharpTest.Controllers
{
    public class AppController
    {
        public void startPro(string folder = "system", string fileName = "SalaryQuery", string guid = "")
        {
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            //string str = string.Format(@"{0}\exe\{1}\{2}.exe {3} 00908 王章华 1 172.18.67.19", Application.StartupPath, folder, fileName, guid);

            //System.Diagnostics.Process.Start(str);

            using (Process p = new Process())
            {
                p.StartInfo.FileName = string.Format(@"{0}\exe\{1}\{2}.exe", Application.StartupPath, folder, fileName);//可执行程序路径
                p.StartInfo.Arguments = string.Format("{0} 00908 王章华 0 172.16.67.93", guid);//参数以空格分隔，如果某个参数为空，可以传入""
                //p.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
                //p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                //p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                //p.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                //p.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
                //p.Start();
                //p.WaitForExit();
                ////正常运行结束放回代码为0
                //if (p.ExitCode != 0)
                //{
                //    output = p.StandardError.ReadToEnd();
                //    output = output.ToString().Replace(System.Environment.NewLine, string.Empty);
                //    output = output.ToString().Replace("\n", string.Empty);
                //    throw new Exception(output.ToString());
                //}
                //else
                //{
                //    output = p.StandardOutput.ReadToEnd();
                //}

                try
                {
                    p.Start();
                    p.WaitForExit();
                    p.Close();
                }
                catch
                {
                }
            }
        }
    }
}
