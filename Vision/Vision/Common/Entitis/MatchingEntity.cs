using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vision.Common.Enums;

namespace Vision.VisionPro.Common.Entitis
{
    public class BaseEntity
    {
        public Bitmap Bitmap { get; set; }

        /// <summary>
        /// 是否定义搜索区域
        /// </summary>
        public bool SearchArea = true;
        /// <summary>
        /// 搜索范围起始点X
        /// </summary>
        public int SearchStartX = 0;

        public int SearchStartY = 0;

        public int SearchWidth = 0;

        public int SearchHeight = 0;

    
    }

    public class MatchingTrain : BaseEntity
    {   
        /// <summary>
         /// 靶标类型
         /// </summary>
        public EmTargetType EmTargetType = EmTargetType.Circle;
        /// <summary>
        /// 模板起始点X
        /// </summary>
        public int StartX = 0;  

        public int StartY = 0;  

        public int Width = 0;

        public int Height = 0;
    }

    public class MathcingRun:BaseEntity 
    {
        /// <summary>
        /// 靶标类型
        /// </summary>
        public EmTargetType EmTargetType = EmTargetType.Circle;

        public double Score = 0.8;
        
    }



    public class ReadCodeRun  :BaseEntity
    {
      public   EmCodeType EmCodeType { get; set; }   = EmCodeType.Unknown;  
    }

}
