using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.BllModel
{
    public class BllResult<T>
    {
        public BllResult(bool success, string msg, T data)
        {
            Success = success;
            Msg = msg;
            Data = data;
        }

        public bool Success { get; set; }
        public string Msg { get; set; }
        public T Data { get; set; }
    }

    public class BllResult
    {
        public BllResult(bool success, string msg, object data)
        {
            Success = success;
            Msg = msg;
            Data = data;
        }

        public bool Success { get; set; }
        public string Msg { get; set; }
        public object Data { get; set; }
    }
}
