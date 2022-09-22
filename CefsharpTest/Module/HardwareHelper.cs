using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace CefsharpTest.Module
{
    public abstract class HardwareHelper
    {
        /// <summary>
        /// 获取CPU序列号
        /// 注：不支持CPU序列则返回null
        /// </summary>
        /// <returns></returns>
        public static string GetCpuSerialNumber()
        {
            try
            {
                string cpuSerial = null;
                ManagementClass myCpu = new ManagementClass("win32_Processor");
                ManagementObjectCollection myCpuConnection = myCpu.GetInstances();
                foreach (ManagementObject myObject in myCpuConnection)
                {
                    cpuSerial = myObject.Properties["Processorid"].Value.ToString();
                    break;
                }
                return cpuSerial;
            }
            catch (Exception ex)
            {
                //throw ex;
                return string.Empty;
            }
        }

        public static string GetDiskSerialNumber()
        {
            try
            {
                string strID = string.Empty;
                ManagementClass mc = new ManagementClass("Win32_PhysicalMedia");
                //网上有提到，用Win32_DiskDrive，但是用Win32_DiskDrive获得的硬盘信息中并不包含SerialNumber属性。   
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    strID = mo.Properties["SerialNumber"].Value.ToString().Trim();
                    break;
                }

                return strID;
            }
            catch (Exception ex)
            {
                //throw ex;
                return string.Empty;

            }
        }
    }
}
