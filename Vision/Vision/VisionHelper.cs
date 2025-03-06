using CommonModels.BllModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vision.Common;

namespace Vision
{
    public class VisionHelper
    {
        public static VisionConfig VisionConfig { get; set; }

        public static BllResult Save()
        {
            try
            {
                string txtPath = AppDomain.CurrentDomain.BaseDirectory + "\\Settings.txt";
                string result = JsonConvert.SerializeObject(VisionConfig);
                result = result.Replace(",", ",\r\n");
                File.WriteAllText(txtPath, result);
                return BllResultFactory.Sucess("");
            }
            catch (Exception ex)
            {
                return BllResultFactory.Error(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public static BllResult Load()
        {
            string txtPath = AppDomain.CurrentDomain.BaseDirectory + "\\Settings.txt";
            //检查路径是否存在该文件，存在则取出来，不存在则赋空值  
            if (File.Exists(txtPath))
            {
                try
                {
                    var resultTxt = File.ReadAllText(txtPath, System.Text.Encoding.UTF8);
                    resultTxt = resultTxt.Replace("\r\n", "");
                    var result = JsonConvert.DeserializeObject<VisionConfig>(resultTxt);
                    if (result == null)
                        return BllResultFactory.Error("配置文件解析失败");
                    VisionConfig = result;
                    return BllResultFactory.Sucess("");
                }
                catch (Exception ex)
                {
                    return BllResultFactory.Error(ex.Message + "\r\n" + ex.StackTrace);
                }
            }
            else
            {
                return BllResultFactory.Error("未找到配置文件");
            }
        }
    }
}
