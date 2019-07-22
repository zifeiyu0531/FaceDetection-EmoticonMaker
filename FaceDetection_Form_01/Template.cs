using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace FaceDetection_Form_01
{
    public class Template
    {
        public Template()
        {
            this.imageFiles = Directory.GetFiles(@"../../../image/", "*.jpg", SearchOption.AllDirectories);
        }

        private List<PictureBox> pictureBoxes = new List<PictureBox>();//存放pictureBox控件
        private string[] imageFiles;                       //存放文件夹image下所有照片的地址


        //主界面获取模板
        public void GetTemplate()
        {

        }

        //更多页面添加模板,把模板照片添加到框里picturebox
        public void AddTemplate(Panel panel)
        {
            foreach (var a in panel.Controls)
            {
                if (a.GetType() == typeof(PictureBox))
                {
                    pictureBoxes.Add((PictureBox)a);
                }

            }
            int num = -1;
            foreach (PictureBox a in pictureBoxes)
            {
                a.SizeMode = PictureBoxSizeMode.StretchImage;
                num++;
                if (imageFiles.Length > num)
                {
                    a.Image = System.Drawing.Image.FromFile(imageFiles[num]);
                    a.ImageLocation = Path.GetFullPath(imageFiles[num]);
                }
            }
        }

        //更多页面删除模板
        public void DeleteTemplate()
        {

        }
    }
}
