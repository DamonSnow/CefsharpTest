using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CefsharpTest
{
    public partial class CanvasTestForm : Form
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

        public CanvasTestForm()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            this.initializeChromium();
        }

        private void CanvasTestForm_Load(object sender, EventArgs e)
        {

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
    }
}
