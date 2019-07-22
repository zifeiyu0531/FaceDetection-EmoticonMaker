using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FaceDetection_Form_01
{
    public class SystemFunction
    {
        private const string subscriptionKey = "19645447fe9f4eb2b182d2c6866e2c44";

        private const string faceEndpoint = "https://westcentralus.api.cognitive.microsoft.com";

        private readonly IFaceClient faceClient = new FaceClient(
            new ApiKeyServiceClientCredentials(subscriptionKey),
            new System.Net.Http.DelegatingHandler[] { });

        static BitmapImage bitmapSource = new BitmapImage();
        static BitmapImage bitmapSource2 = new BitmapImage();
        public Bitmap temp2;
        public BitmapImage bitmapImage;
        public Bitmap original;

        // The list of detected faces.
        public IList<DetectedFace> faceList;
        private IList<DetectedFace> faceListWD;
        // The list of descriptions for the detected faces.
        private string[] faceDescriptions;
        // The resize factor for the displayed image.
        private double resizeFactor;

        public SystemFunction()
        {
            if (Uri.IsWellFormedUriString(faceEndpoint, UriKind.Absolute))
            {
                faceClient.Endpoint = faceEndpoint;
            }
        }       

        // Uploads the image file and calls DetectWithStreamAsync.
        private async Task<IList<DetectedFace>> UploadAndDetectFaces(string imageFilePath)
        {
            // The list of Face attributes to return.
            IList<FaceAttributeType> faceAttributes =
                new FaceAttributeType[]
                {
                    FaceAttributeType.Gender, FaceAttributeType.Age,
                    FaceAttributeType.Smile, FaceAttributeType.Emotion,
                    FaceAttributeType.Glasses, FaceAttributeType.Hair
                };

            // Call the Face API.
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    // The second argument specifies to return the faceId, while
                    // the third argument specifies not to return face landmarks.
                    IList<DetectedFace> faceList =
                        await faceClient.Face.DetectWithStreamAsync(
                            imageFileStream, true, false, faceAttributes);
                    return faceList;
                }
            }
            // Catch and display Face API errors.
            catch (APIErrorException f)
            {
                System.Windows.MessageBox.Show(f.Message);
                return new List<DetectedFace>();
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "Error");
                return new List<DetectedFace>();
            }
        }

        //上传图片
        public BitmapImage Upload(string filePath)
        {
           
            Uri fileUri = new Uri(filePath);

            bitmapSource2.BeginInit();
            bitmapSource2.CacheOption = BitmapCacheOption.None;
            bitmapSource2.UriSource = fileUri;
            bitmapSource2.EndInit();

            return bitmapSource2;
        }

        public RenderTargetBitmap LaterFace { set; get; }
        //生成按钮     
        public async void Make(string filePath)
        {
            
            // Detect any faces in the image.
            faceList = await UploadAndDetectFaces(filePath);          
           
            if (faceList.Count > 0)
            {
                LaterFace = AdjustFunction.Redraw(faceList, bitmapImage);
            }
        }
        //一键生成按钮
        public async void AutoMake(string filePath)
        {

            // Detect any faces in the image.
            faceList = await UploadAndDetectFaces(filePath);

            if (faceList.Count > 0)
            {
                Bitmap bitmap = AdjustFunction.BitmapImageToBitmap(bitmapImage);
                bitmapImage = AdjustFunction.BrightnessP(bitmap, 50);
                bitmapImage = AdjustFunction.SetTrans(bitmap, 50);
                bitmapImage = AdjustFunction.KiContrast(bitmap, 100);
                LaterFace = AdjustFunction.Redraw(faceList, bitmapImage);
            }
        }
        //保存留下来的人脸图
        public RenderTargetBitmap faceRectangle
        {
            set;get;
        }
        //生成小图
        public async void MakeSmall(string filePath)
        {
            // Detect any faces in the image.         
            faceList = await UploadAndDetectFaces(filePath);
            faceListWD = faceList;       
            if (faceList.Count > 0)
            {
                // Prepare to draw rectangles around the faces.
                DrawingVisual visual = new DrawingVisual();
                DrawingContext drawingContext = visual.RenderOpen();
                drawingContext.DrawImage(bitmapSource2,
                    new Rect(0, 0, bitmapSource2.Width, bitmapSource2.Height));
                double dpi = bitmapSource2.DpiX;
                // Some images don't contain dpi info.
                resizeFactor = (dpi == 0) ? 1 : 96 / dpi;
                faceDescriptions = new String[faceList.Count];

                for (int i = 0; i < faceList.Count; ++i)
                {
                    DetectedFace face = faceList[i];


                    //裁剪图片
                    Bitmap temp = AdjustFunction.BitmapImageToBitmap(bitmapSource2);
                    temp2 = img_tailor(temp, new Rectangle(
                            (int)((face.FaceRectangle.Left + 10) * resizeFactor),
                            (int)((face.FaceRectangle.Top + 25) * resizeFactor),
                            (int)((face.FaceRectangle.Width * 0.8) * resizeFactor),
                            (int)((face.FaceRectangle.Height * 0.7) * resizeFactor)
                            ));
                    temp2 = AdjustFunction.BlackAndwhite(temp2);
                    
                    bitmapImage = AdjustFunction.BitmapToBitmapImage(temp2);
                }

                drawingContext.Close();

                // Display the image with the rectangle around the face.
                RenderTargetBitmap faceWithRectBitmap = new RenderTargetBitmap(
                    (int)(bitmapSource2.PixelWidth * resizeFactor),
                    (int)(bitmapSource2.PixelHeight * resizeFactor),
                    96,
                    96,
                    PixelFormats.Pbgra32);

                faceWithRectBitmap.Render(visual);

                faceRectangle = faceWithRectBitmap;
            }
        }

        //还原
        public  void withdraw()
        {
            DetectedFace face = faceListWD[0];
            Bitmap temp = AdjustFunction.BitmapImageToBitmap(bitmapSource2);
            original = img_tailor(temp, new Rectangle(
                            (int)((face.FaceRectangle.Left + 10) * resizeFactor),
                            (int)((face.FaceRectangle.Top + 25) * resizeFactor),
                            (int)((face.FaceRectangle.Width * 0.8) * resizeFactor),
                            (int)((face.FaceRectangle.Height * 0.7) * resizeFactor)
                            ));
            original = AdjustFunction.BlackAndwhite(original);
            AdjustFunction.formattedText = null;
        }

        //裁剪
        public static Bitmap img_tailor(Bitmap src, Rectangle range)
        {
            return src.Clone(range, System.Drawing.Imaging.PixelFormat.DontCare);
        }
   

        //取消
        public void Cancel()
        {

        }
    }
}