using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenCvSharp;

namespace FindCircles
{
    public partial class Form1 : Form
    {
        Mat src = new Mat();

        public Form1()
        {
            InitializeComponent();
            ReadImg();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                ReadImg();
            }
        }

        private void ReadImg() 
        {
            try
            {
                // 读取图像
                src = Cv2.ImRead(textBox1.Text, ImreadModes.Color);
                //显示原图
                pictureBox1.Image = (Bitmap)ToImage(src);
            }
            catch
            {
                MessageBox.Show("当前图片文件不存在！");
                return;
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            int blur = int.Parse(textBox4.Text);
            if (src.Empty())
            {
                return;
            }
            Mat colorImage = src.Clone();
            Mat grayImage = new Mat();

            Cv2.CvtColor(colorImage, grayImage, ColorConversionCodes.BGR2GRAY);

            try
            {
                // 对图像应用高斯模糊以减少噪声
                Cv2.GaussianBlur(grayImage, grayImage, new OpenCvSharp.Size(blur, blur), 0);
            }
            catch
            {
                MessageBox.Show("高斯模糊不能为偶数！");
                return;
            }
            // 使用霍夫变换检测圆
            CircleSegment[] circleSegment = Cv2.HoughCircles(grayImage, HoughModes.Gradient, 2, int.Parse(textBox6.Text), 100, 100, int.Parse(textBox2.Text), int.Parse(textBox3.Text));

            //找到的圆个数
            label1.Text = circleSegment.Length.ToString();

            //最大最小半径初始化
            int maxRadius = 0;
            int minRadius = 0;

            // 输出或显示检测到的圆的信息
            if (circleSegment.Length > 0)
            {
                minRadius = (int)grayImage.Rows / 2;

                for (int i = 0; i < circleSegment.Length; i++)
                {
                    double centerX = circleSegment[i].Center.X;
                    double centerY = circleSegment[i].Center.Y;
                    OpenCvSharp.Point center = new OpenCvSharp.Point((int)centerX, (int)centerY);
                    int radius = (int)circleSegment[i].Radius;

                    //保存最大圆半径
                    maxRadius = maxRadius > radius ? maxRadius : radius;
                    //保存最小圆半径
                    minRadius = minRadius > radius ? radius :  minRadius;

                    // 绘制圆圈并输出中心坐标和半径
                    Cv2.Circle(colorImage, center, 3, Scalar.Red, 3);
                    Cv2.Circle(colorImage, center, radius, Scalar.Red, 3);
                    Cv2.PutText(colorImage, $"C{i + 1} ({center.X}, {center.Y}) R={radius}", center, HersheyFonts.HersheyComplex, 1.0, Scalar.Red, 1, LineTypes.AntiAlias);
                }
            }

            //显示圆最大/最小半径
            label9.Text = maxRadius.ToString();
            label8.Text = minRadius.ToString();

            //显示结果图片
            pictureBox1.Image = (Bitmap)ToImage(colorImage);

            grayImage.Dispose();
            colorImage.Dispose();
        }

        //将OPenCV格式图片转为C#格式图片
        public Image ToImage(Mat src)
        {
            // 将cv::Mat转换为byte数组（假设图像颜色模式为BGR）
            byte[] imgData = new byte[src.Rows * src.Cols * src.Channels()];
            Marshal.Copy(src.Data, imgData, 0, imgData.Length);

            // 创建Bitmap并设置像素格式为Bgr24
            Bitmap bitmap = new Bitmap(src.Width, src.Height, PixelFormat.Format24bppRgb);

            // 指定锁定区域
            Rectangle rect = new Rectangle(0, 0, src.Width, src.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);

            // 复制数据到Bitmap
            int bytesPerPixel = Image.GetPixelFormatSize(bmpData.PixelFormat) / 8;
            IntPtr ptr = bmpData.Scan0;
            Marshal.Copy(imgData, 0, ptr, imgData.Length);

            // 解锁Bitmap
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            src?.Dispose();
        }
    }
}
