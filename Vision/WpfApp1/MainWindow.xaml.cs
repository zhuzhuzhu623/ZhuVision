using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Vision.VisionPro;
using System.Drawing;
using Vision.VisionPro.Common.Entitis;
using System.Threading;
namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public event Func<int, int,bool> On;
        public event Func<(int, int)> On1;
        /// <summary>
        /// 获取当前轴的坐标
        /// </summary>
        public event Func<(int, int)> GetCurrentAxisValue;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            On += MainWindow_On2;


            GetCurrentAxisValue += MainWindow_GetCurrentAxisValue;


            GetOn();
            //VisionProService visionProService = new VisionProService();

            //Bitmap bitmap = new Bitmap("D:\\HalocnImage\\123.jpg");

            //visionProService.InitVision(bitmap.Width,bitmap.Height);
            //ReadCodeRun readCodeRun = new ReadCodeRun();    
            //readCodeRun.Bitmap = bitmap;
            //readCodeRun.SearchArea = false;
            //visionProService.ReadBarCodes(readCodeRun);
        }

        private (int, int) MainWindow_GetCurrentAxisValue()
        {
            return (10, 10);
        }

        private bool MainWindow_On2(int arg1, int arg2)
        {
            return true;
        }

        private int MainWindow_On1(int arg)
        {
            throw new NotImplementedException();
        }

        private void MainWindow_On(int arg1, int arg2)
        {
            
        }


        public (int, int) GetD()
        {
            return (10, 10);
        }
        public void GetOn()
        {
            GetCurrentAxisValue();
        }

        public void Get(Action<int,int> action)
        {
            Thread.Sleep(1000); 
            action.Invoke(10,20);
        }

        public void Set(int x, int y)
        {

        }
    }
}
