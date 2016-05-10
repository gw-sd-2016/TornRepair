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

namespace TornRepair3
{
    public partial class QueueView : Form
    {
        private int num = 0;
        private int ind1 = -1; // the index of the first contour map
        private int ind2 = -1; // the index of the second contour map
        private ColorfulContourMap ctmap; // the current contour map
        public double confidence = 0;
        public double overlap = 0;
        private bool blackOWhite = false;
        public QueueView()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void refresh()
        {
            dataGridView1.Rows.Clear();
            for (int i = 0; i < Form1.blackSourceImages.Count; i++)
            {
                if (/*Form1.matched[i] == false*/true)
                {
                    using (Mat thumbnail = generateThumbnail(Form1.blackSourceImages[i]))
                    {
                        DataGridViewRow row = dataGridView1.Rows[dataGridView1.Rows.Add()];
                        
                        row.Cells["SourceImage"].Value = thumbnail.Bitmap;
                        row.Height = 150;
                    }
                }
            }
            for (int i = 0; i < Form1.whiteSourceImages.Count; i++)
            {
                if (/*Form1.matched[i] == false*/true)
                {
                    using (Mat thumbnail = generateThumbnail(Form1.whiteSourceImages[i]))
                    {
                        DataGridViewRow row = dataGridView1.Rows[dataGridView1.Rows.Add()];
                        row.Cells["SourceImage"].Value = thumbnail.Bitmap;
                        row.Height = 150;
                    }
                }
            }
            ConfidenceView.Text = confidence.ToString();
            OverlapView.Text = overlap.ToString();
        }
        private Mat generateThumbnail(Mat input)
        {
            MatImage m1 = new MatImage(input);
            m1.ResizeTo(150, 150);
            return m1.Out();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            refresh();
        }

        private void QueueView_Activated(object sender, EventArgs e)
        {
            refresh();
        }

        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            listBox1.Items.Clear();
            num = e.RowIndex;

            int index = 1;
            foreach (ColorfulContourMap cmap in Form1.blackContourMaps)
            {
                if (cmap.imageIndex == num)
                {

                    listBox1.Items.Add("Contour " + index + cmap.matched.ToString());
                    index++;
                }
            }
            foreach (ColorfulContourMap cmap in Form1.whiteContourMaps)
            {
                if (cmap.imageIndex+Form1.blackSourceImages.Count == num)
                {

                    listBox1.Items.Add("Contour " + index + cmap.matched.ToString());
                    index++;
                }
            }

            listBox1.SelectedIndex = 0;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int firstAppear = 0;
            bool blackOrWhite = false; // false=black
            foreach (ColorfulContourMap cmap in Form1.blackContourMaps)
            {
                if (cmap.imageIndex == num)
                {
                    blackOrWhite = false;
                    goto black;
                }
                firstAppear++;
                
            }
            firstAppear=0;
            foreach (ColorfulContourMap cmap in Form1.whiteContourMaps)
            {
                if (cmap.imageIndex == num-Form1.blackSourceImages.Count)
                {
                    blackOrWhite = true;
                    break;
                }
                firstAppear++;
                
            }
            black:  if (!blackOrWhite) // black
            {
                Mat img1 = Form1.blackSourceImages[num].Clone();
                Mat img2 = Form1.blackSourceImages[num].Clone();
                img2.SetTo(new MCvScalar(255,255,255));
                //Form1.blackContourMaps[firstAppear + listBox1.SelectedIndex].DrawTo(img1);
                //Form1.blackContourMaps[firstAppear + listBox1.SelectedIndex].DrawColorTo(img2);
                {
                   
                    MatImage m1 = new MatImage(img1);
                   
                    m1.ResizeTo(pictureBox1.Width,pictureBox1.Height);
                   
                    img1 = m1.Out();
                }
                pictureBox1.Image = img1.Bitmap;
                {

                    MatImage m2 = new MatImage(img2);

                    m2.ResizeTo(pictureBox2.Width, pictureBox2.Height);

                    img2 = m2.Out();
                }
                pictureBox2.Image = img2.Bitmap;
            }
            else // white
            {
                Mat img1 = Form1.whiteSourceImages[num - Form1.blackSourceImages.Count].Clone();
                Mat img2 = Form1.whiteSourceImages[num - Form1.blackSourceImages.Count].Clone();
                img2.SetTo(new MCvScalar(0));
                //Form1.whiteContourMaps[firstAppear + listBox1.SelectedIndex].DrawTo(img1);
                //Form1.whiteContourMaps[firstAppear + listBox1.SelectedIndex].DrawColorTo(img2);
                {

                    MatImage m1 = new MatImage(img1);

                    m1.ResizeTo(pictureBox1.Width, pictureBox1.Height);

                    img1 = m1.Out();
                }
                pictureBox1.Image = img1.Bitmap;
                {

                    MatImage m2 = new MatImage(img2);

                    m2.ResizeTo(pictureBox2.Width, pictureBox2.Height);

                    img2 = m2.Out();
                }
                pictureBox2.Image = img2.Bitmap;
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void displayFragments(PictureBox pb)
        {
            // determine the index of the first contour map for a image
            int firstAppear = 0;
            bool blackOrWhite = false;
            foreach (ColorfulContourMap cmap in Form1.blackContourMaps)
            {
                if (cmap.imageIndex == num)
                {
                    blackOrWhite = false;
                    goto black;
                }
                firstAppear++;
            }
            firstAppear = 0;
            foreach (ColorfulContourMap cmap in Form1.whiteContourMaps)
            {
                if (cmap.imageIndex == num - Form1.blackSourceImages.Count)
                {
                    blackOrWhite = true;
                    break;
                }
                firstAppear++;
            }
            black:
            if (!blackOrWhite)
            {
                Mat img1 = new Mat();
                if (pb == pictureBox3)
                {
                    ind1 = firstAppear + listBox1.SelectedIndex;
                    img1 = Form1.blackCroppedImages[ind1].Clone();

                    Form1.blackContourMaps[ind1].DrawTo(img1);
                }
                else
                {
                    ind2 = firstAppear + listBox1.SelectedIndex;
                    img1 = Form1.blackCroppedImages[ind2].Clone();

                    Form1.blackContourMaps[ind2].DrawTo(img1);
                }
                {

                    MatImage m2 = new MatImage(img1);

                    m2.ResizeTo(pb.Width, pb.Height);

                    img1 = m2.Out();
                }
                pb.Image = img1.Bitmap;
                blackOWhite = false;
               
            }
            else
            {
                Mat img1 = new Mat();
                if (pb == pictureBox3)
                {
                    ind1 = firstAppear + listBox1.SelectedIndex;
                    img1 = Form1.whiteCroppedImages[ind1].Clone();

                    Form1.whiteContourMaps[ind1].DrawTo(img1);
                }
                else
                {
                    ind2 = firstAppear + listBox1.SelectedIndex;
                    img1 = Form1.whiteCroppedImages[ind2].Clone();

                    Form1.whiteContourMaps[ind2].DrawTo(img1);
                }
                {

                    MatImage m2 = new MatImage(img1);

                    m2.ResizeTo(pb.Width, pb.Height);

                    img1 = m2.Out();
                }
                pb.Image = img1.Bitmap;
                blackOWhite = true;
               
            }
           
        }
        private void button3_Click(object sender, EventArgs e)
        {
            displayFragments(pictureBox3);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            displayFragments(pictureBox4);
        }

        private void button2_Click(object sender, EventArgs e)
        {
           
            if (Form1.blackCroppedImages.Count+Form1.whiteCroppedImages.Count != 1)
            {
                if (ind1 < 0 || ind2 < 0)
                {
                    //TwoPieceMatchAnalysis tpma = new TwoPieceMatchAnalysis { map1 = Form1.blackContourMaps[0], map2 = Form1.contourMaps[1] };
                    //tpma.Show();
                    MessageBox.Show("Please provide two input images for matching");
                }
                else
                {
                    if (!blackOWhite)
                    {
                        TwoPieceMatchAnalysis tpma = new TwoPieceMatchAnalysis { map1 = Form1.blackContourMaps[ind1], map2 = Form1.blackContourMaps[ind2],
                        pic1=Form1.blackCroppedImages[ind1],pic2=Form1.blackCroppedImages[ind2]};
                        tpma.blackOrWhite = blackOWhite;
                        tpma.Show();
                    }
                    else
                    {
                        TwoPieceMatchAnalysis tpma = new TwoPieceMatchAnalysis { map1 = Form1.whiteContourMaps[ind1], map2 = Form1.whiteContourMaps[ind2],
                            pic1 = Form1.whiteCroppedImages[ind1].Clone(),
                            pic2 = Form1.whiteCroppedImages[ind2].Clone()
                        };
                        tpma.blackOrWhite = blackOWhite;
                        tpma.Show();
                    }
                }
            }
        }
    }
}

