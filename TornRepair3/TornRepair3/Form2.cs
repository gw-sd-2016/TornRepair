using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TornRepair3
{
    public partial class Form2 : Form
    {
        public Image<Bgr, byte> img;
        public Mat mimg;
        public Mat mimgInGray;
        public Form2()
        {
            InitializeComponent();

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                long start = DateTime.Now.ToFileTime() * 10 / 1000000;
                for (int i = 0; i < 1; i++)
                {
                    img = new Image<Bgr, byte>(Openfile.FileName).Resize(0.5, Inter.Cubic);






                    pictureBox1.Image = img.ToBitmap();


                    
                }
                Console.WriteLine(DateTime.Now.ToFileTime() * 10 / 1000000 - start);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            long start = DateTime.Now.ToFileTime() * 10 / 1000000;
            for (int i = 0; i<1; i++)
            {
                MatImage m1 = new MatImage(mimg);
                m1.Rotate(45,new Bgr(255,255,255));

                // engineering code
                //Mat mim = m1.Out();
                //pictureBox2.Image = mim.Bitmap;

                // artistic code
                mimg = m1.Out();
                pictureBox2.Image = mimg.Bitmap;
            }
            Console.WriteLine(DateTime.Now.ToFileTime() * 10 / 1000000 - start);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            long start = DateTime.Now.ToFileTime() * 10 / 1000000;
            for (int i = 0; i < 1000; i++)
            {

                // uncomment below for engineering code
                //Image<Bgr,byte> imgf = img.Rotate(45, new Bgr(0, 0, 0));
                //pictureBox1.Image = imgf.Bitmap;

                // uncomment below for artistic code
                img = img.Rotate(45, new Bgr(0, 0, 0));
               
                pictureBox1.Image = img.Bitmap;


            }
            Console.WriteLine(DateTime.Now.ToFileTime() * 10 / 1000000 - start);
            MessageBox.Show("Congractulations!! You discovered one of the most brilliant artifacts in my chamber. Hope you can figure out how does that work!", "Acheivement unlocked: Experienced explorer!!");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                long start = DateTime.Now.ToFileTime() * 10 / 1000000;
                for (int i = 0; i < 1; i++)
                {
                    mimg = new Mat(Openfile.FileName, LoadImageType.AnyColor);
                    // block for using my helper class for Mat
                    {
                        // pipeline for using my helper class
                        // load
                        MatImage m1 = new MatImage(mimg);
                        // operations
                        m1.Resize(0.5);
                        // other operations

                        // output
                        mimg = m1.Out();
                    }
                    pictureBox2.Image = mimg.Bitmap;
                }
                Console.WriteLine(DateTime.Now.ToFileTime() * 10 / 1000000 - start);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
           
        }

        private void button4_Click(object sender, EventArgs e)
        {
            long start = DateTime.Now.ToFileTime() * 10 / 1000000;
            for (int i = 0; i < 1000; i++)
            {
                MatImage m1 = new MatImage(mimg);
                m1.Translate(-1 + 2*i % 2, -1 + 2*i % 2); // don't let the picture move out of the box

                // engineering code
                Mat mim = m1.Out();
                pictureBox2.Image = mim.Bitmap;

                
            }
            Console.WriteLine(DateTime.Now.ToFileTime() * 10 / 1000000 - start);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            MatImage m1 = new MatImage(mimg);
            m1.Convert();
           
            mimgInGray = m1.Out();
            MatImage m2 = new MatImage(mimgInGray);
            m2.SmoothGaussian(3);
            m2.ThresholdBinaryInv(245, 255);
            // this causes an exception, and I don't know how to fix that yet. 
            //The good news is that this really doesn't matter for small images
            //m2.MorphologyEx(2); 
            mimgInGray = m2.Out();
            MatImage m3 = new MatImage(mimgInGray);
            VectorOfVectorOfPoint contours = m3.FindContours();
            List<ColorfulContourMap> cmaps = ColorfulContourMap.getAllContourMap(mimg, 0);




            //CvInvoke.DrawContours(mimg, contours,-1, new Bgr(255,0,0).MCvScalar,2);
            Mat mimg2 =new Mat(new Size(mimg.Width,mimg.Height),DepthType.Cv8U,3);
            foreach(ColorfulContourMap cmap in cmaps)
            {
                cmap.DrawColorTo(mimg2);
            }
            pictureBox1.Image = mimg.Bitmap;

            pictureBox2.Image = mimg2.Bitmap;
        }
    }
}
