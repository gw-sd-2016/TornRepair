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
    // The struct used to describe the contour of a polygon
    public struct Phi
    {
        public double x; // x position
        public double y; // y position
        public double theta; // turning angle
        public int l; // arc length
    }

    public struct Match
    {
        //Match parameters


        public int t11, t12;//starting and ending arc length parameter for piece1
        public int t21, t22;//starting and ending arc length parameter for piece2
        public int x11, y11, x12, y12; //starting and ending coordinates for piece1
        public int x21, y21, x22, y22; //starting and ending coordinates for piece2
        public double confidence;

    }

    // This part is other's code
    public partial class Form1 : Form
    {

        public List<List<Phi>> DNAs = new List<List<Phi>>();
        public List<Contour<Point>> results = new List<Contour<Point>>();
        public List<int> verts = new List<int>();
        public int order = 1;

        Image<Bgr, Byte> img1; // source image
        Image<Bgr, Byte> img2; // result image
        List<Image<Bgr, Byte>> imgs = new List<Image<Bgr, Byte>>(); // all of the loaded image

        List<Image<Bgr, Byte>> imgs_scaled = new List<Image<Bgr, Byte>>();
        List<Image<Gray, Byte>> imgs_gray = new List<Image<Gray, Byte>>(); // all of the grayscale source images
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string strFileName = string.Empty;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //dataGridView1.Rows.Clear();

                img1 = new Image<Bgr, Byte>(ofd.FileName);
                Image<Bgr, Byte>[] images = new Image<Bgr, Byte>[ofd.FileNames.Length];
                imgs.Add(img1);

                for (int i = 0; i < images.Length; i++)
                {
                    images[i] = new Image<Bgr, Byte>(ofd.FileNames[i]);
                    using (Image<Bgr, Byte> thumbnail = images[i].Resize(150, 150, INTER.CV_INTER_CUBIC, true))
                    {
                        DataGridViewRow row = dataGridView1.Rows[dataGridView1.Rows.Add()];
                        row.Cells["Image"].Value = thumbnail.ToBitmap();
                        row.Height = 150;
                    }
                }

                // The image box is 338*335
                double width = img1.Width;
                double height = img1.Height;
                double iWidth = pictureBox1.Width;
                img1 = img1.Resize(pictureBox1.Width, (int)(iWidth / width * height), INTER.CV_INTER_LINEAR);
                imgs_scaled.Add(img1);
                pictureBox1.Image = img1.ToBitmap();
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;


            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            img2 = img1.CopyBlank();

            Image<Gray, Byte> gray1 = img1.Convert<Gray, Byte>().PyrDown().PyrUp();
            Image<Gray, Byte> cannyGray = gray1.Canny(120, 180); // The parameter is subjected to change

            int green = 0;
            int red = 0;
            int count = 0;
            using (MemStorage storage1 = new MemStorage())
                // The contour map works similarly to a linked list in C, cannot traverse this using C# functions, must use provided CV functions
                // This will generate 2 contour maps for a filled image part, one inner contour and one outer contour
                // This only works for image parts filled with single color, not works for newspaper for now

                for (Contour<Point> contours1 = cannyGray.FindContours(); contours1 != null; contours1 = contours1.HNext)
                {
                    // In test case 6, the image parts are placed in four corners of the canvas, so it can check if the contours are 
                    // separated easier
                    // Top left: (0,60),(222,322)
                    // Top right: (180,280),(222,322)
                    // Bottom left:(0,60),(0,60)
                    // Bottom right: (180,280),(0,60)
                    Point[] a = contours1.ToArray();
                    // Only Draw Even Number
                    if (/*ount % 4 == 2*/ true)
                    {
                        img2.Draw(contours1, new Bgr(255, green, red), 5);
                    }
                    green += 50;
                    red += 20;
                    count++;
                }
            label1.Text = "Contour:" + count;
            pictureBox2.Image = img2.ToBitmap();
        }

        private void button3_Click(object sender, EventArgs e)
        {

            List<Phi> DNAseq = new List<Phi>(); // the exact contour map
            List<Phi> verticies = new List<Phi>(); // the polygon map
            List<Phi> DNA = new List<Phi>(); // the final contour map with arc length and angle of each vertex
            /*img2 = imgs[imgs.Count-1].CopyBlank();
            Image<Gray, Byte> gray1 = imgs[imgs.Count-1].Convert<Gray, Byte>();*/
            img2 = img1.CopyBlank();
            Image<Gray, Byte> gray1 = img1.Convert<Gray, Byte>();
            imgs_gray.Add(gray1.Clone());

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
                contour = contour.HNext;
                for (; contour != null; contour = contour.HNext)
                {
                    double nextArea = Math.Abs(contour.Area);
                    if (area < nextArea)
                    {
                        area = nextArea;
                        maxArea = contour;
                    }

                }
                Point[] exactPoints = maxArea.ToArray();
                int i = 0;
                foreach (Point point in exactPoints)
                {
                    Phi tempPhi = new Phi();
                    tempPhi.x = point.X;
                    tempPhi.y = point.Y;
                    tempPhi.theta = 0;
                    tempPhi.l = i;
                    DNAseq.Add(tempPhi);
                    i++;
                }
                if (radioButton1.Checked)
                {
                    img2.Draw(maxArea, new Bgr(255, 0, 0), 2);
                    goto display;
                }
                Contour<Point> result = maxArea.ApproxPoly(1.0, storage1);

                Point[] points = result.ToArray();
                i = 0;
                foreach (Point point in points)
                {
                    Phi tempPhi = new Phi();
                    tempPhi.x = point.X;
                    tempPhi.y = point.Y;
                    tempPhi.theta = 0;
                    tempPhi.l = 0;
                    verticies.Add(tempPhi);
                    i++;
                }
                // interpolate the arc length
                for (int j = 0, t = 0; j < verticies.Count; ++j)
                {
                    while (!(verticies[j].x == DNAseq[t].x && verticies[j].y == DNAseq[t].y))
                    {
                        t++;
                    }

                    Phi vert = verticies[j];
                    vert.l = t;
                    verticies[j] = vert;


                    t = 0;

                }
                // End of Functional codes

                // Start of Experimental

                double angle = 0;
                for (i = 0; i < verticies.Count; ++i)
                {
                    int next = i + 1;
                    if (next == verticies.Count)
                    {
                        next = 0;
                    }//Bounds check

                    //Turning angle computation
                    //If this is starting vertex theta = 0
                    if (i == 0)
                    {
                        angle = 0.0;
                    }
                    else
                    { //Compute turning angle
                        double turn_angle = Util.calcAngle(verticies[next].x, verticies[next].y, verticies[i].x, verticies[i].y, verticies[i - 1].x, verticies[i - 1].y);
                        double vector1_x = verticies[i].x - verticies[i - 1].x;
                        double vector1_y = verticies[i].y - verticies[i - 1].y;
                        double vector2_x = verticies[next].x - verticies[i].x;
                        double vector2_y = verticies[next].y - verticies[i].y;
                        int direction = Util.sign((vector1_y * vector2_x) - (vector1_x * vector2_y));
                        // Cumulate the turning angles
                        angle += (180 - turn_angle) * direction;
                    }

                    //Store this turning function value
                    Phi vert = verticies[i];
                    vert.theta = Math.Round(angle);
                    verticies[i] = vert;
                    //For all points between vertex[i] and vertex[next] theta will be the same
                    // Theta is the total angle turned from start point
                    if (verticies[next].l > verticies[i].l)
                    {
                        for (int j = verticies[i].l; j <= verticies[next].l; ++j)
                        {
                            Phi v = DNAseq[j];
                            v.theta = Math.Round(angle);
                            DNAseq[j] = v;
                        }
                    }
                    else
                    {
                        for (int j = verticies[i].l; j <= verticies[next].l + exactPoints.Length; ++j)
                        {
                            int index = j;
                            if (index >= exactPoints.Length)
                            {
                                index -= exactPoints.Length;
                            }
                            Phi vv = DNAseq[index];
                            vv.theta = Math.Round(angle);
                            DNAseq[index] = vv;
                        }
                    }
                }

                for (i = 0; i < DNAseq.Count; i++)
                {
                    int index = i + verticies[0].l + 1;
                    if (index >= DNAseq.Count)
                    {
                        index -= DNAseq.Count;

                    }
                    DNA.Add(DNAseq[index]);
                    Phi tem = DNA[i];
                    tem.l = i;
                    DNA[i] = tem;
                }
                DNAs.Add(DNA);

                verts.Add(verticies.Count);
                order = DNAs.Count;
                if (order > 1)
                {
                    button5.Enabled = true;
                }
                label1.Text = "Contour:" + order + " of " + DNAs.Count;
                label2.Text = "Edges:" + DNA.Count;
                label3.Text = "Verts:" + verticies.Count;
                img2.Draw(result, new Bgr(255, 0, 0), 2);



                results.Add(result);
            }

            display: //img2=img2.Resize(pictureBox2.Width, pictureBox2.Height, INTER.CV_INTER_LINEAR);
            pictureBox2.Image = img2.ToBitmap();

        }

        

        private void button5_Click(object sender, EventArgs e)
        {
            if (order > 1)
            {
                order--;
                label1.Text = "Contour: " + order + " of " + DNAs.Count;
                img2 = img1.CopyBlank();
                //img2.Draw(results[order - 1], new Bgr(255, 0, 0), 2);
                pictureBox2.Image = imgs_gray[order - 1].ToBitmap();
                label2.Text = "Edge: " + DNAs[order - 1].Count;
                label3.Text = "Verts: " + verts[order - 1];
                button6.Enabled = true;
            }
            else
            {
                Enabled = false;
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (order < DNAs.Count)
            {
                order++;
                label1.Text = "Contour: " + order + " of " + DNAs.Count;
                img2 = img1.CopyBlank();

                //img2.Draw(results[order - 1], new Bgr(255, 0, 0), 2);
                pictureBox2.Image = imgs_gray[order - 1].ToBitmap();
                label2.Text = "Edge: " + DNAs[order - 1].Count;
                label3.Text = "Verts: " + verts[order - 1];
                button5.Enabled = true;
            }
            else
            {
                Enabled = false;
            }

        }
        // Real Matching algorithm
        private void button7_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;

            int times = imgs_gray.Count;
            int count = 1;
            List<Image<Gray, Byte>> imgray = imgs_gray.ToList();
            while (imgray.Count!=1)
            {
                int ind = 1;
                int max_conf = 0; // max confidence
                List<Match> segment = new List<Match>(); // The matching parameters for each segment of a part
                List<int> confidence = new List<int>(); // The confidence level for each segment

                // Match the pieces
                for (int i = 1; i < imgs_gray.Count; ++i)
                {
                    // 0=this part, i=the index for other parts
                    textBox1.AppendText("Searching for best pair \n");
                    segment.Add(Util.partialMatch(DNAs[count], DNAs[i])); // add all the match parameter for other parts compare to this part
                    confidence.Add((int)segment[i - 1].confidence); // add all the confidence level for other parts
                    //Index of max confidence
                    if (max_conf < confidence[i - 1])
                    {
                        max_conf = confidence[i - 1];
                        ind = i;
                    }
                }
                textBox1.AppendText("Found \n");

                //Join the pieces
                textBox1.AppendText("Joining Them \n");
                double rot=0.0;
                Point c1, c2;
                c1 = new Point();
                c2 = new Point();
                bool Joined = true;
                Match m = segment[ind - 1];
                Util.transformation(DNAs[0], DNAs[ind], ref m, ref c1, ref c2, ref rot);




            }
            progressBar1.Value = 100;



        }



    }

}
