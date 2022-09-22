using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CefsharpTest
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //this.webView21.Source = new Uri(Application.StartupPath + "\\html\\在线地图.html");
            this.webView21.Source = new Uri("http://www.baidu.com");
        }
    }
}
