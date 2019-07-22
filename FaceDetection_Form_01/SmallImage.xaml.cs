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

namespace FaceDetection_Form_01
{
    /// <summary>
    /// SmallImage.xaml 的交互逻辑
    /// </summary>
    public partial class SmallImage : UserControl
    {
        public System.Windows.Media.ImageSource Source { get; set; }

        public SmallImage()
        {
            InitializeComponent();
        }
        public void SetSource(BitmapImage bitmapSource)
        {
            SmallFace.Source = bitmapSource;
        }
        public void SetSource(RenderTargetBitmap bitmapSource)
        {
            SmallFace.Source = bitmapSource;
        }
    }
}
