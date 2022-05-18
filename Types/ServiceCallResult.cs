using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API2.Types
{
    public class ServiceCallResult<T>
    {
        public ResultCodeEnum ResultCode { get; set; }
        public string Remark { get; set; }
        public T data { get; set; }
    }
}