﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.BllModel
{
    public class BllResultFactory
    {
        public static BllResult Sucess(Object data, String msg)
        {
            return new BllResult(true, msg, data);
        }

        public static BllResult Sucess(Object data)
        {
            return new BllResult(true, "", data);
        }

        public static BllResult Sucess(String msg)
        {
            return new BllResult(true, msg, null);
        }

        public static BllResult Sucess()
        {
            return new BllResult(true, "", null);
        }

        public static BllResult Error(object data, String msg)
        {
            return new BllResult(false, msg, data);
        }

        public static BllResult Error(object data)
        {
            return new BllResult(false, "", data);
        }

        public static BllResult Error(String msg)
        {
            return new BllResult(false, msg, null);
        }

        public static BllResult Error()
        {
            return new BllResult(false, "", null);
        }
    }

    public class BllResultFactory<T>
    {
        #region 泛型
        public static BllResult<T> Sucess(T data, String msg)
        {
            return new BllResult<T>(true, msg, data);
        }

        public static BllResult<T> Sucess(T data)
        {
            return new BllResult<T>(true, "", data);
        }

        public static BllResult<T> Sucess(String msg)
        {
            return new BllResult<T>(true, msg, default(T));
        }

        public static BllResult<T> Sucess()
        {
            return new BllResult<T>(true, "", default(T));
        }

        public static BllResult<T> Error(T data, String msg)
        {
            return new BllResult<T>(false, msg, data);
        }

        public static BllResult<T> Error(T data)
        {
            return new BllResult<T>(false, "", data);
        }

        public static BllResult<T> Error(String msg)
        {
            return new BllResult<T>(false, msg, default(T));
        }

        public static BllResult<T> Error()
        {
            return new BllResult<T>(false, "", default(T));
        }
        #endregion
    }
}
