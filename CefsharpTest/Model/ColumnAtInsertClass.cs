using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefsharpTest.Model
{
    public class ColumnToInsertClass
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string label { get; set; }

        /// <summary>
        /// 列值
        /// </summary>
        public string value { get; set; }

        /// <summary>
        /// 是否需要单引号
        /// 若为数值数据类型，则设置为false
        /// 因使用 MySQL 数据库，故即使数值数据类型，也可以使用单引号包裹进行insert操作，故默认使用 true
        /// </summary>
        public bool need_quotation = true;
    }
}
