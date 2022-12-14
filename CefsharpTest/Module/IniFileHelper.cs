using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CefsharpTest.Module
{
    /// <summary>
    /// INI文件的操作类
    /// </summary>
    public class IniFileHelper
    {
        public string iniFilePath;

        public IniFileHelper(string path)
        {
            this.iniFilePath = path;
        }

        #region 声明读写INI文件的API函数

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, Byte[] retVal, int size, string filePath);

        #endregion

        /// <summary>
        /// 写INI文件
        /// </summary>
        /// <param name="section">段落</param>
        /// <param name="key">键</param>
        /// <param name="iValue">值</param>
        public void IniWriteValue(string section, string key, string iValue)
        {
            WritePrivateProfileString(section, key, iValue, iniFilePath);
        }

        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">段落</param>
        /// <param name="key">键</param>
        /// <returns>返回的键值</returns>
        public string IniReadValue(string section, string key)
        {
            StringBuilder tempValue = new StringBuilder(255);

            int i = GetPrivateProfileString(section, key, "", tempValue, 255, iniFilePath);

            return tempValue.ToString();
        }

        ///// <summary>
        ///// 读取INI文件
        ///// </summary>
        ///// <param name="section">段落</param>
        ///// <param name="key">键</param>
        ///// <returns>返回byte[]类型的section组或键值组</returns>
        //public byte[] IniReadValue(string section, string key)
        //{
        //    byte[] tempValue = new byte[255];

        //    int i = GetPrivateProfileString(section, key, "", tempValue, 255, iniFilePath);

        //    return tempValue;
        //}
    }
}
