﻿using System
    ;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.ID;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.PMAlign;
using CommonModels.BllModel;
using HalconDotNet;
using Vision.Common.EntitisResult;
using Vision.Common.Enums;
using Vision.VisionPro.Common;
using Vision.VisionPro.Common.Entitis;

namespace Vision.VisionPro
{
    public class VisionProService
    {
        private  int ImageWidth = 0;    
        private int ImageHeight = 0;

        /// <summary>
        /// 移动机台
        /// </summary>
        public event Func<int, int, bool> SetMotionMove;
        /// <summary>
        /// 获取当前轴的坐标
        /// </summary>
        public event Func<(int, int)> GetCurrentAxisValue;
        /// <summary>
        /// 
        /// </summary>
        private CogImageConvertTool ImageConvertTool = new CogImageConvertTool();
        /// <summary>
        /// 畸变校准
        /// </summary>
        private CogCalibCheckerboardTool CalibCheckerboardTool;
        /// <summary>
        /// 九点标定
        /// </summary>
        private CogCalibNPointToNPointTool NPointCalib;
        /// <summary>
        /// 模板匹配
        /// </summary>
        private CogPMAlignTool PMAlignTool = new CogPMAlignTool();
        /// <summary>
        /// 读码工具
        /// </summary>
        private CogIDTool IDTool = new CogIDTool(); 
        public BllResult InitVision(int width,int height)
        {
            ImageWidth = width; ImageHeight = height;

            //设置角度值
            PMAlignTool.RunParams.ZoneAngle.High = Math.PI / 4;
            PMAlignTool.RunParams.ZoneAngle.Low = -Math.PI / 4;
            PMAlignTool.RunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.LowHigh;

            return BllResultFactory.Sucess();
        }
        /// <summary>
        /// 单模板训练
        /// </summary>
        /// <param name="templateEntity"></param>
        /// <returns></returns>
        public BllResult MatchingTrain(MatchingTrain templateEntity)
        {
            PMAlignTool.InputImage = new CogImage8Grey(templateEntity.Bitmap);
            PMAlignTool.Pattern.TrainImage = PMAlignTool.InputImage;

            if (templateEntity.EmTargetType ==  Vision.Common.Enums.EmTargetType.Circle)
            {
                CogCircle crl = new CogCircle();

                crl.CenterX = templateEntity.StartX / 2;
                crl.CenterY = templateEntity.StartY / 2;

                crl.Radius = templateEntity.Width / 2;
                PMAlignTool.Pattern.TrainRegion = crl;
                PMAlignTool.Pattern.Origin.TranslationX = crl.CenterX;
                PMAlignTool.Pattern.Origin.TranslationY = crl.CenterY;
            } 
            else
            {
                CogRectangle r = new CogRectangle();

                r.Width = templateEntity.Width;
                r.X = templateEntity.StartX;

                r.Height = templateEntity.Height;
                r.Y = templateEntity.StartY;

                PMAlignTool.Pattern.TrainRegion = r;
                PMAlignTool.Pattern.Origin.TranslationX = r.CenterX;
                PMAlignTool.Pattern.Origin.TranslationY = r.CenterY;
            }

            PMAlignTool.Pattern.Train();
            if (!PMAlignTool.Pattern.Trained)
            {
                return BllResultFactory.Error("模板匹配训练失败");
            }          
            return BllResultFactory.Sucess("");
        }
        /// <summary>
        /// 单模板匹配
        /// </summary>
        /// <param name="mathcingRun"></param>
        /// <returns></returns>
        public BllResult<List<MatchingResult>> Matching(MathcingRun  mathcingRun)
        {
            PMAlignTool.InputImage = new CogImage8Grey(mathcingRun.Bitmap);
            PMAlignTool.RunParams.AcceptThreshold = mathcingRun.Score;

            CogRectangle searchR = new CogRectangle();
            searchR.X = mathcingRun.SearchArea ? mathcingRun.SearchStartX : 0;
            searchR.Y = mathcingRun.SearchArea ? mathcingRun.SearchStartY : 0;
            searchR.Width = mathcingRun.SearchArea ? mathcingRun.SearchWidth : ImageWidth;
            searchR.Height = mathcingRun.SearchArea ? mathcingRun.SearchHeight : ImageHeight;

            PMAlignTool.SearchRegion = searchR;

            PMAlignTool.Run();
            if (PMAlignTool.RunStatus.Result == CogToolResultConstants.Error)
            {
                return BllResultFactory<List<MatchingResult>>.Error(null,"未匹配到模板");
            }
            if (PMAlignTool.Results.Count ==0)            
                return BllResultFactory<List<MatchingResult>>.Error(null, "未匹配到模板");
            List<MatchingResult> matchingResults = new List<MatchingResult>();
            for (int i =0;i< PMAlignTool.Results.Count; i++)
            {              
                CogPMAlignResult result = PMAlignTool.Results[i];
                var resultX = Math.Round(result.GetPose().TranslationX, 4);
                var resultY = Math.Round(result.GetPose().TranslationY, 4);
                var resultAngle = Math.Round(result.GetPose().Rotation, 4);

                matchingResults.Add(new MatchingResult() { X = resultX, Y = resultY, Angle = resultAngle });                
            }
            return BllResultFactory<List<MatchingResult>>.Sucess(matchingResults, "");
        }

        /// <summary>
        /// 读码
        /// </summary>
        /// <param name="readCodeRun"></param>
        /// <returns></returns>
        public BllResult<List<ReadCodeResult>> ReadBarCodes(ReadCodeRun readCodeRun)
        {
            ImageConvertTool.InputImage = new CogImage8Grey(readCodeRun.Bitmap);
            ImageConvertTool.Run();
            CogRectangle searchR = new CogRectangle();
            searchR.X = readCodeRun.SearchArea ? readCodeRun.SearchStartX : 0;
            searchR.Y = readCodeRun.SearchArea ? readCodeRun.SearchStartY : 0;
            searchR.Width = readCodeRun.SearchArea ? readCodeRun.SearchWidth : ImageWidth;
            searchR.Height = readCodeRun.SearchArea ? readCodeRun.SearchHeight : ImageHeight;

            if (readCodeRun.EmCodeType != Vision.Common.Enums.EmCodeType.Unknown)
                return ReadAllBarCode(searchR, readCodeRun.EmCodeType);           
            else
            {
                List<ReadCodeResult> readCodeResults = new List<ReadCodeResult>();
                var resultCode = ReadAllBarCode(searchR, EmCodeType.Code128);
                if (resultCode.Success)
                    readCodeResults.AddRange(resultCode.Data);
                resultCode = ReadAllBarCode(searchR, EmCodeType.DataMatrix);
                if (resultCode.Success)
                    readCodeResults.AddRange(resultCode.Data);
                resultCode = ReadAllBarCode(searchR, EmCodeType.QRCode);
                if (resultCode.Success)
                    readCodeResults.AddRange(resultCode.Data);
                resultCode = ReadAllBarCode(searchR, EmCodeType.PDF417);
                if (resultCode.Success)
                    readCodeResults.AddRange(resultCode.Data);
                if (readCodeResults.Count == 0)
                    return BllResultFactory<List<ReadCodeResult>>.Error("未读到条码");
                return BllResultFactory<List<ReadCodeResult>>.Sucess(readCodeResults, "Sucess");
            }
        }


        public BllResult NinePointCalibration(MatchingTrain matchingTrain)
        {
            var resultMatch = MatchingTrain(matchingTrain);
            if (!resultMatch.Success) return BllResultFactory.Error();

            NPointCalib.Calibration.Uncalibrate();
            int n = NPointCalib.Calibration.NumPoints;
            for (int i = 0; i < n; i++)
            {
                NPointCalib.Calibration.DeletePointPair(n - 1 - i);
            }
            var resultAxis = GetCurrentAxisValue();
            int stepX = 4000;
            int stepY = 3000;

            int iX = resultAxis.Item1 - stepX;
            int iY = resultAxis.Item2 - stepY;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int x = iX + stepX * j, y = iY + stepY * i;
                    var resultBool = SetMotionMove(x, y);
                    if(!resultBool)
                        return BllResultFactory.Error();
                    Thread.Sleep(1000);

                    if (NPointCalib.Results.Count == 1)
                    {
                        CogPMAlignResult result = Vision.m_CalibPMA.Results[0];
                        double dx = Math.Round(result.GetPose().TranslationX, 4);
                        double dy = Math.Round(result.GetPose().TranslationY, 4);
                        NPointCalib.Calibration.AddPointPair(dx, dy, stepX * j, stepY * (2 - i));
                        SystemSetting.s_VisionCalibrationPoints[iIndex].dUnCalibratedX = dx;
                        SystemSetting.s_VisionCalibrationPoints[iIndex].dUnCalibratedY = dy;
                        SystemSetting.s_VisionCalibrationPoints[iIndex].dRawCalibratedX = iStepX * j;
                        SystemSetting.s_VisionCalibrationPoints[iIndex].dRawCalibratedY = iStepY * (2 - i);
                    }
                    else
                    {

                        return;
                    }

                    Thread.Sleep(100);
                }
            }
                Vision.m_NPointCalib.Calibration.Calibrate();






            return BllResultFactory.Sucess();
        }


        #region 私有方法
        /// <summary>
        /// 读码
        /// </summary>
        /// <param name="searchR"></param>
        /// <param name="emCodeType"></param>
        /// <returns></returns>
        private BllResult<List<ReadCodeResult>> ReadAllBarCode(CogRectangle searchR,EmCodeType emCodeType)
        {
            IDTool.RunParams.DisableAll1DCodes();
            IDTool.RunParams.QRCode.Enabled = true;
            IDTool.RunParams.QRCode.Model = CogIDQRCodeModelConstants.All;
            if (emCodeType == Vision.Common.Enums.EmCodeType.Code128)
            {
                IDTool.RunParams.DisableAll1DCodes();
                IDTool.RunParams.Code128.Enabled = true;
                IDTool.RunParams.DecodedStringCodePage = CogIDCodePageConstants.ANSILatin1;
            }
            else if (emCodeType == Vision.Common.Enums.EmCodeType.DataMatrix)          
            {
                IDTool.RunParams.DisableAll1DCodes();
                IDTool.RunParams.DataMatrix.Enabled = true;
            }
            else if (emCodeType == Vision.Common.Enums.EmCodeType.PDF417)           
            {
                IDTool.RunParams.DisableAll1DCodes();
                IDTool.RunParams.PDF417.Enabled = true;
                IDTool.RunParams.PDF417.Type = CogIDPDFTypeConstants.All;
            }

            IDTool.InputImage = ImageConvertTool.OutputImage;
            IDTool.Region = searchR;
            IDTool.RunParams.NumToFind = 10;
            IDTool.Run();
            if (IDTool.RunStatus.Result == CogToolResultConstants.Error || IDTool.Results == null)
                return BllResultFactory<List<ReadCodeResult>>.Error("未读到条码");

            if (IDTool.Results.Count == 0)
                return BllResultFactory<List<ReadCodeResult>>.Error("未读到条码");

            List<ReadCodeResult> readCodeResults = new List<ReadCodeResult>();
            for (int i = 0; i < IDTool.Results.Count; i++)
            {
                if (IDTool.Results[i].DecodedData == null) continue;

                ReadCodeResult readCodeResult = new ReadCodeResult();
                readCodeResult.BarCode = IDTool.Results[i].DecodedData.DecodedString;
                readCodeResult.EmCodeType = emCodeType;
                readCodeResults.Add(readCodeResult);

            }
            if (readCodeResults.Count == 0)
                return BllResultFactory<List<ReadCodeResult>>.Error("未读到条码");
            return BllResultFactory<List<ReadCodeResult>>.Sucess(readCodeResults, "Sucess");
        }


        public int CogImgToBytes(CogImage8Grey image, ref byte[] bytes, ref int bmpStride, ref int bmpHeight)
        {
            int iR = 0;
            try
            {
                CogImage8Grey cogImg = new CogImage8Grey(image.ToBitmap());

                int W = cogImg.Width;
                int H = cogImg.Height;
                Cognex.VisionPro.ICogImage8PixelMemory tM = cogImg.Get8GreyPixelMemory(Cognex.VisionPro.CogImageDataModeConstants.Read, 0, 0, W, H);

                Bitmap bmp = new Bitmap(tM.Width, tM.Height, tM.Stride, PixelFormat.Format8bppIndexed, tM.Scan0);
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);// bmp.PixelFormat);
                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;
                // Declare an array to hold the bytes of the bitmap. 
                int iBytesLength = Math.Abs(bmpData.Stride) * bmp.Height;
                bytes = new byte[iBytesLength];
                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, bytes, 0, iBytesLength);
                // Unlock the bits.
                bmp.UnlockBits(bmpData);

                bmpStride = bmpData.Stride;
                bmpHeight = bmp.Height;
                iR = 1;
            }
            catch (Exception ex)
            {
                iR = -1;
               
            }
            return iR;
        }

        public  int BMPToBytes(Bitmap bmp, ref byte[] bytes, ref int bmpStride, ref int bmpHeight)
        {
            int iR = 0;
            try
            {
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);// bmp.PixelFormat);
                                                                                                                  // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap. 
                int iBytesLength = Math.Abs(bmpData.Stride) * bmp.Height;
                bytes = new byte[iBytesLength];

                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, bytes, 0, iBytesLength);

                // Unlock the bits.
                bmp.UnlockBits(bmpData);

                bmpStride = bmpData.Stride;
                bmpHeight = bmp.Height;
                iR = 1;
            }
            catch (Exception ex)
            {
                iR = -1;
            }
            return iR;
        }

        /// <summary>
        /// 按指定压缩质量进行压缩
        /// 调用示例CompressImage("test.png", "tests.png", 50L);
        /// </summary>
        /// <param name="sourcePath">原图路径</param>
        /// <param name="destinationPath">保存路径</param>
        /// <param name="quality">压缩质量</param>
        public void CompressImage(string sourcePath, string destinationPath, long quality)
        {
            // 加载原始图片
            Image sourceImage = Image.FromFile(sourcePath);

            // 创建目标图像的编码参数，通过JPEG质量参数指定压缩质量
            ImageCodecInfo jpegCodec = GetEncoder(ImageFormat.Jpeg);
            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            // 保存压缩后的图片
            sourceImage.Save(destinationPath, jpegCodec, encoderParameters);
            // 释放资源
            sourceImage.Dispose();
        }
        public ImageCodecInfo GetEncoder(ImageFormat format)
        {
            // 获取所有支持的图像编码器
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            // 找到第一个支持指定格式的编码器
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            // 如果没有找到匹配的编码器则抛出异常
            throw new ArgumentException("No appropriate encoder found.", nameof(format));
        }

        #endregion
    }
}
