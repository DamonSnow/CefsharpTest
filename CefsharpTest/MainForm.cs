using CefSharp;
using CefSharp.WinForms;
using CefsharpTest.Controllers;
using CefsharpTest.Module;
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
    public partial class MainForm : Form
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

        public MainForm()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            this.initializeChromium();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.lblCopyrightYear.Text = "© " + DateTime.Now.Year.ToString();

            this.lblNow.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss");
        }

        public void initializeChromium()
        {
            CefSharpSettings.LegacyJavascriptBindingEnabled = true;

            CefSettings settings = new CefSettings();
            settings.CefCommandLineArgs.Add("disable-gpu", "1"); // 禁用gpu
            settings.SetOffScreenRenderingBestPerformanceArgs();

            string page = string.Format(@"{0}\www\html\index.html", Application.StartupPath);

            if(!File.Exists(page))
            {
                MessageBox.Show("页面不存在");
            }

            Cef.Initialize(settings);

            chromeBrowser = new ChromiumWebBrowser(page);

            this.panelBody.Controls.Add(chromeBrowser);
            chromeBrowser.Dock = DockStyle.Fill;

            //BrowserSettings browserSettings = new BrowserSettings();
            //browserSettings.FileAccessFromFileUrls = CefState.Enabled;
            //browserSettings.UniversalAccessFromFileUrls = CefState.Enabled;

            chromeBrowser.Load(page);

            chromeBrowser.RegisterJsObject("AppController", new AppController(), new CefSharp.BindingOptions { CamelCaseJavascriptNames = false });

            chromeBrowser.KeyboardHandler = new CEFKeyBoardHander();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cef.Shutdown();
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

        private void timerInterval_Tick(object sender, EventArgs e)
        {
            this.lblNow.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss");
        }
    }
}
