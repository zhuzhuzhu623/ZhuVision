using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vision.Common.Enums;

namespace Vision.Common.EntitisResult
{
    public class ReadCodeResult
    {
        public string BarCode { get; set; } = "";

        public EmCodeType EmCodeType { get; set; }  
    }
}
