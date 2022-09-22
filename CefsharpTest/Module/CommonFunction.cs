using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CefsharpTest.Module
{
    public static class CommonFunction
    {
        public enum ImagePosition
        {
            system,
            control,
            form
        }

        public static Image GetImage(string imageName, ImagePosition position = ImagePosition.system)
        {
            string imageFileAbsolutePath = Application.StartupPath + @"\images\" + position.ToString() + @"\" + imageName.Trim();
            if (File.Exists(imageFileAbsolutePath))
            {
                return Image.FromFile(imageFileAbsolutePath);
            }
            return Image.FromFile(Application.StartupPath + @"\images\" + ImagePosition.system.ToString() + @"\None.png");
        }

        /// <summary>
        /// 方法：判断是否可以是vista/win7以上的系统
        /// </summary>
        /// <returns>如果是则返回true，不是则返回false</returns>
        public static bool IsLaterThanVista()
        {
            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT && os.Version.Major >= 6)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        
    }
}
