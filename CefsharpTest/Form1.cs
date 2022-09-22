using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public enum ImagePosition
        {
            system,
            control,
            form
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ////先生成数据集合
            //var chiSquare = new ChiSquared(5);
            //Console.WriteLine(@"2. Generate 1000 samples of the ChiSquare(5) distribution");
            //var data = new double[1000];
            //for (var i = 0; i < data.Length; i++)
            //{
            //    data[i] = chiSquare.Sample();
            //}

            ////使用扩展方法进行相关计算
            //Console.WriteLine(@"3.使用扩展方法获取生成数据的基本统计结果");
            //Console.WriteLine(@"{0} - 最大值", data.Maximum().ToString(" #0.00000;-#0.00000"));
            //Console.WriteLine(@"{0} - 最小值", data.Minimum().ToString(" #0.00000;-#0.00000"));
            //Console.WriteLine(@"{0} - 均值", data.Mean().ToString(" #0.00000;-#0.00000"));
            //Console.WriteLine(@"{0} - 中间值", data.Median().ToString(" #0.00000;-#0.00000"));
            //Console.WriteLine(@"{0} - 有偏方差", data.PopulationVariance().ToString(" #0.00000;-#0.00000"));
            //Console.WriteLine(@"{0} - 无偏方差", data.Variance().ToString(" #0.00000;-#0.00000"));
            //Console.WriteLine(@"{0} - 标准偏差", data.StandardDeviation().ToString(" #0.00000;-#0.00000"));
            //Console.WriteLine(@"{0} - 标准有偏偏差", data.PopulationStandardDeviation().ToString(" #0.00000;-#0.00000"));
            //Console.WriteLine();

            //List<string> al = new List<string>() { "11", "22", "33" };

            //List<string> bl = new List<string>();

            //bl = bl.Concat(al).ToList();

            //bl.Add("44");

            //this.textBox1.Text = string.Join(",", bl.ToArray());

            ImagePosition position = ImagePosition.system;

            this.textBox1.Text = position.ToString();
        }
    }
}
