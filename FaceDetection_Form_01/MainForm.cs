using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;
using System.Globalization;

namespace FaceDetection_Form_01
{
    public partial class MainForm : Form
    {

        SystemFunction systemFunction = new SystemFunction();
        Template template = new Template();

        private static BitmapImage extraImage;//存放临时图片
        public static BitmapImage ExtraImage { get => extraImage; set => extraImage = value; }

        public static string filePath;
        public MainForm()
        {
            InitializeComponent();
            template.AddTemplate(Template_Panel);
            foreach (var a in Template_Panel.Controls)
            {
                if (a.GetType() == typeof(PictureBox))
                {
                    PictureBox b;
                    b = (PictureBox)a;
                    b.Click += new EventHandler(this.T_Click);
                }
            }
        }
        //上传按钮
        private void Upload_Click(object sender, EventArgs e)
        {
            string filePath;
            openFileDialog2.Title = "选择你的图片";
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog2.FileName;
                smallImage1.SetSource(systemFunction.Upload(filePath));
                systemFunction.MakeSmall(filePath);
                RenderTargetBitmap render = systemFunction.faceRectangle;
                smallImage1.SetSource(render);
            }
            else
            {
                System.Windows.MessageBox.Show("未选择图片");
            }                    
        }
        //生成按钮
        private void Make_Click(object sender, EventArgs e)
        {
            systemFunction.Make(filePath);
        }
        //一键生成按钮
        private void autoMake_Click(object sender, EventArgs e)
        {
            systemFunction.AutoMake(filePath);
        }

        //更多模板
        private void MoreTemplate_Click(object sender, EventArgs e)
        {
            MoreTemplatesForm moreTemplatesForm = new MoreTemplatesForm(userControl11);
            moreTemplatesForm.ShowDialog();
        }

        //选择模板的回调函数
        private void T_Click(object sender, EventArgs e)
        {
            PictureBox p = (PictureBox)sender;
            string location = p.ImageLocation;
            filePath = location;
            Uri fileUri = new Uri(location);
            ExtraImage = new BitmapImage(fileUri);
            userControl11.SetSource(ExtraImage);
        }

        //保存按钮
        private void SaveButton_Click(object sender, EventArgs e)
        {
            ImageSource SaveImage = userControl11.GetSource();
            string fileName;
            saveFileDialog1.Title = "保存图片";
            saveFileDialog1.Filter = @"jpeg|*.jpg|bmp|*.bmp|gif|*.gif";
            saveFileDialog1.FileName = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() +
                DateTime.Now.Day + DateTime.Now.Hour.ToString() + DateTime.Now.Month.ToString() +
                DateTime.Now.Second.ToString();
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileName = saveFileDialog1.FileName.ToString();
                Bitmap bitmap = AdjustFunction.ImageSourceToBitmap(SaveImage);
                bitmap.Save(fileName);
                System.Windows.MessageBox.Show("保存成功");
            }
            else
            {
                System.Windows.MessageBox.Show("未保存");
            }            
        }

        private void trackBar1_Scroll_1(object sender, EventArgs e)
        {
           userControl11.SetSource( AdjustFunction.SetTrans(systemFunction.temp2, 
               trackBar1.Value,systemFunction.faceList));
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            userControl11.SetSource(AdjustFunction.SetEdgeBlur(systemFunction.temp2, 
                trackBar3.Value, systemFunction.faceList));
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            userControl11.SetSource(AdjustFunction.KiContrast(systemFunction.temp2,
                trackBar4.Value, systemFunction.faceList));
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            userControl11.SetSource(AdjustFunction.BrightnessP(systemFunction.temp2,
               trackBar5.Value, systemFunction.faceList));
        }

        private void withdrew_Click(object sender, EventArgs e)
        {
            trackBar5.Value = (trackBar5.Maximum+ trackBar5.Minimum) / 2;
            trackBar4.Value= (trackBar4.Maximum + trackBar4.Minimum) / 2;
            trackBar3.Value = (trackBar3.Maximum + trackBar3.Minimum) / 2;
            trackBar1.Value = (trackBar1.Maximum + trackBar1.Minimum) / 2;
            systemFunction.withdraw();
            systemFunction.temp2 = systemFunction.original;
            userControl11.SetSource(AdjustFunction.Redraw(systemFunction.faceList,
                AdjustFunction.BitmapToBitmapImage(systemFunction.temp2)));
            imgText.Text = "";
        }

        //定时器刷新处理后的人脸图片
        private int c = 1;
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.smallImage1.SetSource(systemFunction.faceRectangle);
            if (systemFunction.LaterFace != null&&c==1)
            {
                c++;
                this.userControl11.SetSource(systemFunction.LaterFace);
            }
              
        }

        private void imgText_TextChanged(object sender, EventArgs e)
        {
            AdjustFunction.formattedText = new FormattedText(
                imgText.Text,
                CultureInfo.GetCultureInfo("zh-cn"),
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Verdana"),
                32,
                System.Windows.Media.Brushes.Black);
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            MainForm newForm = new MainForm();
            System.Windows.Forms.Application.Restart();
        }
    }
}
