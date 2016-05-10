using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV.CvEnum;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using Emgu.CV.ML.Structure;
using System.IO;
using System.Threading;

namespace TornRepair3
{
    public partial class Form1 : Form
    {
        public static List<Mat> blackSourceImages=new List<Mat>();
        public static List<Mat> whiteSourceImages = new List<Mat>();
        public static List<ColorfulContourMap> blackContourMaps = new List<ColorfulContourMap>(); // the contour maps
        public static List<ColorfulContourMap> whiteContourMaps = new List<ColorfulContourMap>(); // the contour maps
        public static List<Mat> blackCroppedImages = new List<Mat>(); // the cropped source image based on the contour map
        public static List<Mat> whiteCroppedImages = new List<Mat>(); // the cropped source image based on the contour map
        

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            String win1 = "Test Window"; //The name of the window
            CvInvoke.NamedWindow(win1); //Create the window using the specific name
            UMat img=new UMat();
            Image<Bgr, byte> old_img = new Image<Bgr, byte>(100, 100);
            Mat img2 = new Mat();
            long start=DateTime.Now.ToFileTime()*10/1000000;
            
            for (int i = 0; i < 200; i++)
            {

                //img = new UMat(100, 100,DepthType.Cv8U, 3); //Create a 3 channel image of 400x200

                //img.SetTo(new Bgr(255, 0, 0).MCvScalar); // set it to Blue color

                old_img = new Image<Bgr, byte>(1200, 1200);
                old_img.SetValue(new Bgr(255, 0, 0));

                //img2 = new Mat(1200, 1200, DepthType.Cv8U, 3);
                //img2.SetTo(new Bgr(255, 0, 0).MCvScalar);
                
                //Draw "Hello, world." on the image using the specific font
                CvInvoke.PutText(
                   old_img,
                   "Hello, world "+i,
                   new System.Drawing.Point(10, 80),
                   FontFace.HersheyComplex,
                   1.0,
                   new Bgr(0, 255, 0).MCvScalar);
                
            }
            Console.WriteLine(DateTime.Now.ToFileTime()*10/1000000 - start);
            start = DateTime.Now.ToFileTime() * 10 / 1000000;
           
            
            CvInvoke.Imshow(win1, old_img); //Show the image
            CvInvoke.WaitKey(0);  //Wait for the key pressing event
            CvInvoke.DestroyWindow(win1); //Destroy the window if key is pressed
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                Image<Bgr, Byte> My_Image = new Image<Bgr, byte>(Openfile.FileName);
                long start = DateTime.Now.ToFileTime() * 10 / 1000000;
                for (int i = 0; i < 1000; i++)
                {
                    pictureBox1.Image = My_Image.ToBitmap();
                    
                }
                Console.WriteLine(DateTime.Now.ToFileTime() * 10 / 1000000 - start);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
           
            
 

 

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form2 f2=new Form2();
            
            f2.Show();
        }

       

        private void pictureBox1_DoubleClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show("222");
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                Mat My_Image =new Mat(Openfile.FileName,LoadImageType.AnyColor);
                long start = DateTime.Now.ToFileTime() * 10 / 1000000;
                for (int i = 0; i < 1000; i++)
                {
                    pictureBox1.Image = My_Image.Bitmap;
                }
                Console.WriteLine(DateTime.Now.ToFileTime() * 10 / 1000000 - start);
            }
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            MessageBox.Show("222");
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                Mat My_Image = new Mat(Openfile.FileName, LoadImageType.AnyColor);
                long start = DateTime.Now.ToFileTime() * 10 / 1000000;
                for (int i = 0; i < 1000; i++)
                {
                    pictureBox1.Image = My_Image.Bitmap;
                    
                }
                Console.WriteLine(DateTime.Now.ToFileTime() * 10 / 1000000 - start);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
