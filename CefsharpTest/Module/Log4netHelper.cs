using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefsharpTest.Module
{
    /// <summary>
    /// 日志帮助类
    /// 包含：log4net写入日志文件
    /// </summary>
    public abstract class Log4netHelper
    {
        /// <summary>
        /// 写入错误日志
        /// </summary>
        /// <param name="t"></param>
        /// <param name="msg"></param>
        public static void Error(Type t, string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Error(msg);
        }

        /// <summary>
        /// 写入错误日志
        /// </summary>
        /// <param name="t"></param>
        /// <param name="ex"></param>
        public static void Error(Type t, Exception ex)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            //log.Error("异常，具体信息：", ex);
            string exMsg = GetExceptionContent(ex);
            log.Error(exMsg);
        }

        /// <summary>
        /// 错误消息日志
        /// </summary>
        /// <param name="t"></param>
        /// <param name="msg"></param>
        public static void Info(Type t, string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Info(msg);
        }

        /// <summary>
        /// 写入警告日志
        /// </summary>
        /// <param name="t"></param>
        /// <param name="msg"></param>
        public static void Warm(Type t, string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Warn(msg);
        }

        private static string GetExceptionContent(Exception ex)
        {
            string result = string.Empty;
            if (ex.InnerException != null)
            {
                result += GetExceptionContent(ex.InnerException);
            }

            result += string.Format("异常信息：{0}\r\n异常栈：{1}\r\n", ex.Message, ex.StackTrace);

            return result;
        }
    }
}
