using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing.Imaging;
using System.Drawing;

namespace Vision.Halcon
{
    public class HalconHelper
    {

        private static void Bitmap24ToHobject(Bitmap bmp, out HObject ho_img)
        {
            int height = bmp.Height;//图像的高度
            int width = bmp.Width;//图像的宽度

            Rectangle imgRect = new Rectangle(0, 0, width, height);
            BitmapData bitData = bmp.LockBits(imgRect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int stride = bitData.Stride;

            unsafe
            {
                //24位的BitMap每个像素三个字节
                int count = height * width * 3;
                byte[] data = new byte[count];
                byte* bptr = (byte*)bitData.Scan0;
                fixed (byte* pData = data)
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width * 3; j++)
                        {
                            //舍去stride填充的部分
                            data[i * width * 3 + j] = bptr[i * stride + j];
                        }
                    }
                    HOperatorSet.GenImageInterleaved(out ho_img, new IntPtr(pData), "bgr", bmp.Width, bmp.Height, 0, "byte", bmp.Width, bmp.Height, 0, 0, -1, 0);
                }
            }
        }
        /// <summary>
        /// hobject彩色图24位转bitmap
        /// </summary>
        /// <param name="ho_image"></param>
        /// <param name="res24"></param>
        private static void HobjectToBitmap24(HObject ho_image, out Bitmap res24)
        {
            HTuple type, width, height;
            //创建交错格式图像
            HOperatorSet.InterleaveChannels(ho_image, out HObject InterImage, "rgb", "match", 255);
            //获取交错格式图像指针
            HOperatorSet.GetImagePointer1(InterImage, out HTuple Pointer, out type, out width, out height);
            IntPtr ptr = Pointer;
            res24 = new Bitmap(width / 3, height, width, System.Drawing.Imaging.PixelFormat.Format24bppRgb, ptr);
        }
        /// <summary>
        /// hobject彩色图32位转bitmap
        /// </summary>
        /// <param name="ho_image"></param>
        /// <param name="res32"></param>
        private static void HobjectToBitmap32(HObject ho_image, out Bitmap res32)
        {
            HTuple type, width, height;
            //创建交错格式图像
            HOperatorSet.InterleaveChannels(ho_image, out HObject InterImage, "argb", "match", 255);
            //获取交错格式图像指针
            HOperatorSet.GetImagePointer1(InterImage, out HTuple Pointer, out type, out width, out height);
            IntPtr ptr = Pointer;
            res32 = new Bitmap(width / 4, height, width, System.Drawing.Imaging.PixelFormat.Format32bppRgb, ptr);
        }
        /// <summary>
        /// hobject灰度8位转bitmap
        /// </summary>
        /// <param name="ho_image"></param>
        /// <param name="res8"></param>
        private static void HobjectToBitmap8(HObject ho_image, out Bitmap res8)
        {
            HTuple type, width, height;
            HOperatorSet.GetImagePointer1(ho_image, out HTuple Pointer, out type, out width, out height);
            IntPtr ptr = Pointer;
            res8 = new Bitmap(width, height, width, System.Drawing.Imaging.PixelFormat.Format8bppIndexed, ptr);
            //设置灰度调色板
            ColorPalette cp = res8.Palette;
            for (int i = 0; i < 256; i++)
            {
                cp.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
            }
            res8.Palette = cp;
        }
    }
}
