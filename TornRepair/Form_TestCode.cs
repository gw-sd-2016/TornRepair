using Emgu.CV;
using Emgu.CV.Structure;
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

namespace TornRepair
{

    // This is the partial class for all my own researches.

        // Future Improvements:
        // Rewrite all the C++ like functions to C# like
        // Create C# class/structs for List<Phi> so that a lot of C++ like functions can be called as member functions in that class


    public partial class Form1 : Form
    {

        private void button4_Click(object sender, EventArgs e)
        {
            List<List<Phi>> DNAseqs = new List<List<Phi>>(); // the exact contour map
            List<List<Phi>> verticiess = new List<List<Phi>>(); // the polygon map
            //List<List<Phi>> DNAs = new List<List<Phi>>(); // the final contour map with arc length and angle of each vertex
            img2 = img1.CopyBlank();
            Image<Gray, Byte> gray1 = img1.Convert<Gray, Byte>();

            gray1 = gray1.SmoothGaussian(3);
            gray1 = gray1.ThresholdBinaryInv(new Gray(245), new Gray(255));
            gray1 = gray1.MorphologyEx(null, CV_MORPH_OP.CV_MOP_CLOSE, 2);
            using (MemStorage storage1 = new MemStorage())
            {
                Image<Gray, Byte> temp = gray1.Clone();
                Contour<Point> contour = temp.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE, RETR_TYPE.CV_RETR_EXTERNAL);
                if (contour == null)
                {
                    // if no contour are found, display the image directly so that user will know the input cannot be recognized
                    pictureBox2.Image = gray1.ToBitmap();
                    return;
                }
                double area = Math.Abs(contour.Area);
                Contour<Point> maxArea = contour;
                // Create a list of valid contours
                List<Contour<Point>> contours = new List<Contour<Point>>();
                List<Point[]> exactPointss = new List<Point[]>();
                if (area >= Util.MIN_AREA)
                {
                    contours.Add(contour);
                }
                contour = contour.HNext;
                for (; contour != null; contour = contour.HNext)
                {
                    double nextArea = Math.Abs(contour.Area);
                    if (nextArea >= Util.MIN_AREA)
                    {
                        contours.Add(contour);
                    }

                }
                foreach (Contour<Point> contr in contours)
                {
                    exactPointss.Add(contr.ToArray());
                }

                int i = 0;
                foreach (Point[] points in exactPointss)
                {
                    List<Phi> DNAseq = new List<Phi>();
                    foreach (Point point in points)
                    {
                        Phi tempPhi = new Phi();
                        tempPhi.x = point.X;
                        tempPhi.y = point.Y;
                        tempPhi.theta = 0;
                        tempPhi.l = i;
                        DNAseq.Add(tempPhi);
                        i++;
                    }
                    DNAseqs.Add(DNAseq);

                }
                foreach (Contour<Point> contr in contours)
                {
                    img2.Draw(contr, new Bgr(255, 0, 0), 2);
                }

                pictureBox2.Image = img2.ToBitmap();
            }

        }

        private void button10_Click(object sender, EventArgs e)
        {

            Image<Gray, byte> img102 = r10.source2;
            Image<Bgr, byte> img101 = imgs_scaled[1].Clone();
            textBox1.AppendText(r10.center2new.ToString() + "\n");
            img101.Draw(new CircleF(r10.center2old, 2), new Bgr(255, 0, 0), 2);
            img101.Draw(new CircleF(r10.center2new, 2), new Bgr(255, 0, 0), 2);
            img102.Draw(new CircleF(r10.center2old, 2), new Gray(127), 2);
            img102.Draw(new CircleF(r10.center2new, 2), new Gray(127), 2);
            img102.Draw(new CircleF(new PointF(img102.Width / 2, img102.Height / 2), 2), new Gray(127), 2);
            img102.Draw(r10.centerLinee, new Gray(127), 2);

            pictureBox1.Image = img101.ToBitmap();
            pictureBox2.Image = img102.ToBitmap();

        }

        private void button11_Click(object sender, EventArgs e)
        {
            Image<Bgr, Byte> img11 = new Image<Bgr, byte>(new Size(pictureBox1.Width, pictureBox1.Height));
            img11.Draw(new CircleF(r10.center1, 2), new Bgr(255, 0, 0), 2);
            img11.Draw(new CircleF(r10.center2new, 2), new Bgr(0, 255, 0), 2);
            pictureBox1.Image = img11.ToBitmap();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Image<Bgr, Byte> img91 = imgs_scaled[0].Clone();
            Image<Bgr, Byte> img92 = imgs_scaled[1].Clone();

            textBox1.AppendText(cn1.ToString() + "\n");
            textBox1.AppendText(cn2.ToString() + "\n");

            img91.Draw(new CircleF(cn1, 2), new Bgr(255, 0, 0), 2);
            img92.Draw(new CircleF(cn2, 2), new Bgr(255, 0, 0), 2);
            pictureBox1.Image = img91.ToBitmap();
            pictureBox2.Image = img92.ToBitmap();

        }
        // Test method for matching line
        private void button8_Click(object sender, EventArgs e)
        {

            int times = imgs_gray.Count;
            int count = 1;
            while (imgs_gray.Count != 1)
            {
                int ind = 1;
                int max_conf = 0; // max confidence
                List<Match> segment = new List<Match>(); // The matching parameters for each segment of a part
                List<int> confidence = new List<int>(); // The confidence level for each segment

                // Match the pieces
                for (int i = 1; i < imgs_gray.Count; ++i)
                {
                    // 0=this part, i=the index for other parts

                    segment.Add(Util.partialMatch(DNAs[count], DNAs[0])); // add all the match parameter for other parts compare to this part
                    confidence.Add((int)segment[i - 1].confidence); // add all the confidence level for other parts
                    //Index of max confidence
                    if (max_conf < confidence[i - 1])
                    {
                        max_conf = confidence[i - 1];
                        ind = i;
                    }
                }
                img2.CopyBlank();
                img1 = imgs_scaled[0].Clone();
                img2 = imgs_scaled[count].Clone();
                // Test: Draw the possible connecting edges
                // Will delete this if the matching algorithm works correctly
                for (int i = 0; i < segment.Count; i++)
                {
                    int start1 = segment[i].t11;
                    int end1 = segment[i].t12;
                    int start2 = segment[i].t21;
                    int end2 = segment[i].t22;
                    List<Phi> dna = DNAs[count];
                    List<Phi> dna2 = DNAs[0];
                    List<Phi> effective = new List<Phi>();
                    List<Phi> eff2 = new List<Phi>();
                    for (int j = start1; j < end1; j++)
                    {
                        effective.Add(dna[j]);
                    }
                    for (int j = start2; j < end2; j++)
                    {
                        eff2.Add(dna2[j]);
                    }
                    List<Point> points = new List<Point>();
                    foreach (Phi p in effective)
                    {
                        points.Add(new Point((int)p.x, (int)p.y));
                    }


                    img2.DrawPolyline(points.ToArray(), false, new Bgr(0, 255 / (i + 1), 20 * i), 2);
                    //img2 = img2.Resize(pictureBox2.Width, pictureBox2.Height,INTER.CV_INTER_LINEAR);
                    pictureBox2.Image = img2.ToBitmap();

                    List<Point> point2 = new List<Point>();
                    foreach (Phi p in eff2)
                    {
                        point2.Add(new Point((int)p.x, (int)p.y));
                    }


                    img1.DrawPolyline(point2.ToArray(), false, new Bgr(0, 255 / (i + 1), 20 * i), 2);
                    //img2 = img2.Resize(pictureBox2.Width, pictureBox2.Height,INTER.CV_INTER_LINEAR);
                    pictureBox1.Image = img1.ToBitmap();



                }

                textBox1.AppendText("Matched \n");

                count++;
                times--;
                //Join the pieces
                break;
            }
        }

       
    }
}
