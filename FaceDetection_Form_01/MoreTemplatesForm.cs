using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace FaceDetection_Form_01
{
    public partial class MoreTemplatesForm : Form
    {
        Template Template = new Template();
        private List<PictureBox> pictureBoxes = new List<PictureBox>();//存放pictureBox控件
        private string[] imageFiles;//存放文件夹image下所有照片的地址

        private UserControl1 userControl1;
        public MoreTemplatesForm(UserControl1 userControl1)
        {
            this.userControl1 = userControl1;
            InitializeComponent();
            imageFiles = Directory.GetFiles(@"../../../image/", "*.jpg", SearchOption.AllDirectories);
            Template.AddTemplate(panel1);
            addMouseEvent();
        }

        private void addMouseEvent()
        {
            foreach (var a in panel1.Controls)
            {
                if (a.GetType() == typeof(PictureBox))
                {
                    PictureBox b;
                    b = (PictureBox)a;
                    b.Click += new EventHandler(this.MoreTemplatesForm_Click);
                }

            }

        }

        private void MoreTemplatesForm_Click(object sender, System.EventArgs e)
        {
            PictureBox p = (PictureBox)sender;
            string location = p.ImageLocation;
            MainForm.filePath = location;
            Uri fileUri = new Uri(location);
            MainForm.ExtraImage = new BitmapImage(fileUri);
            this.userControl1.SetSource(MainForm.ExtraImage);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

