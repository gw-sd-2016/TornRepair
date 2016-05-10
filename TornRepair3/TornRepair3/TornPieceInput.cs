using Emgu.CV;
using Emgu.CV.CvEnum;
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
    public partial class TornPieceInput : Form
    {
        public TornPieceInput()
        {
            InitializeComponent();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            refresh();
        }

        private void refresh()
        {

            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            for (int i = 0; i < Form1.blackSourceImages.Count; i++)
            {

                using (Mat thumbnail = generateThumbnail(Form1.blackSourceImages[i]))
                {
                    DataGridViewRow row = dataGridView1.Rows[dataGridView1.Rows.Add()];
                    row.Cells["Image"].Value = thumbnail.Bitmap;
                    row.Height = 150;
                }
            }

            for (int i = 0; i < Form1.whiteSourceImages.Count; i++)
            {

                using (Mat thumbnail = generateThumbnail(Form1.whiteSourceImages[i]))
                {
                    DataGridViewRow row = dataGridView2.Rows[dataGridView2.Rows.Add()];
                    row.Cells["Image2"].Value = thumbnail.Bitmap;
                    row.Height = 150;
                }
            }

        }

        private Mat generateThumbnail(Mat input)
        {
            MatImage m1 = new MatImage(input);
            m1.ResizeTo(150, 150);
            return m1.Out();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            Openfile.Multiselect = true;
            if (Openfile.ShowDialog() == DialogResult.OK)
            {

                DialogResult dia = MessageBox.Show("What is the background color of those images? Yes for white, No for black"
                , "", MessageBoxButtons.YesNo);
                foreach (String fileName in Openfile.FileNames)
                {
                    Mat mimg = new Mat(fileName, LoadImageType.AnyColor);

                    if (dia == DialogResult.Yes)
                    {
                        Form1.whiteSourceImages.Add(mimg);
                    }
                    else
                    {
                        Form1.blackSourceImages.Add(mimg);
                    }
                }


            }
            refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            DataGridViewRow currentRow = dataGridView1.CurrentRow;
            if (currentRow != null)
            {
                Form1.blackSourceImages.RemoveAt(currentRow.Index);

                refresh();
            }

        }
        private void button6_Click(object sender, EventArgs e)
        {
            DataGridViewRow currentRow = dataGridView2.CurrentRow;
            if (currentRow != null)
            {
                Form1.whiteSourceImages.RemoveAt(currentRow.Index);

                refresh();
            }
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
          
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dataGridView1_MouseLeave(object sender, EventArgs e)
        {
           
        }

        private void dataGridView2_MouseLeave(object sender, EventArgs e)
        {
            
        }

        private void TornPieceInput_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            // extract contour map
            if (Form1.blackSourceImages.Count+ Form1.whiteSourceImages.Count != 0)
            {
                //DisplayBestMatch bestMatchView = new DisplayBestMatch();
                QueueView qv = new QueueView();

                // extract the contour maps, send the result into queueview
                int blackIndex = 0;
                int whiteIndex = 0;
                
                List<int> missBlackIndex = new List<int>();
                List<int> missWhiteIndex = new List<int>();
                for (int i= 0; i < Form1.blackSourceImages.Count; i++)
                {
                    List<ColorfulContourMap> cmap;
                    bool inProcess = false;
                    try {
                        cmap = ColorfulContourMap.getAllContourMap(Form1.blackSourceImages[i], blackIndex, 1);
                       
                       
                        for (int j= 0; j < cmap.Count; j++) {
                            inProcess = true;
                            cmap[j]=AddCroppedImages(false, cmap[j]);
                        }
                        Form1.blackContourMaps.AddRange(cmap);
                        blackIndex++;
                    }
                    catch
                    {
                        if (!inProcess)
                        {
                            MessageBox.Show("One of your input images seems to have a white background. That image will be moved into white category");
                            cmap = ColorfulContourMap.getAllContourMap(Form1.blackSourceImages[i], whiteIndex, 0);
                           
                            //missBlackIndex.Add(blackIndex);
                            Form1.whiteSourceImages.Insert(whiteIndex, Form1.blackSourceImages[i]);

                            for (int j= 0; j < cmap.Count; j++)
                            {

                                cmap[j]=AddCroppedImages(true, cmap[j]);
                            }
                            Form1.whiteContourMaps.AddRange(cmap);
                            whiteIndex++;

                            Form1.blackSourceImages.Remove(Form1.blackSourceImages[i]);
                            i--;
                        }
                    }
                    
                   
                   
                   
                }
               
                for(int i=whiteIndex;i<Form1.whiteSourceImages.Count;i++)
                {
                    List<ColorfulContourMap> cmap;
                    bool inProcess = false;
                    try {
                        cmap = ColorfulContourMap.getAllContourMap(Form1.whiteSourceImages[i], whiteIndex, 0);
                       
                        
                        for(int j=0; j<cmap.Count;j++)
                        {
                            inProcess = true;
                            cmap[j]=AddCroppedImages(true, cmap[j]);
                        }
                        Form1.whiteContourMaps.AddRange(cmap);
                        whiteIndex++;
                    }
                    catch
                    {
                        if (!inProcess)
                        {
                            MessageBox.Show("One of your input images seems to have a black background. That image will be moved into black category");
                            cmap = ColorfulContourMap.getAllContourMap(Form1.whiteSourceImages[i], blackIndex, 1);
                           
                            //missWhiteIndex.Add(whiteIndex);

                            for (int j= 0; j < cmap.Count; j++)
                            {

                                cmap[j]=AddCroppedImages(false, cmap[j]);
                            }
                            Form1.blackContourMaps.AddRange(cmap);
                            blackIndex++;

                            Form1.blackSourceImages.Add(Form1.whiteSourceImages[i]);
                            Form1.whiteSourceImages.Remove(Form1.whiteSourceImages[i]);
                            i--;
                        }

                    }


                   
                  
                }
                // move the images into their correct container if users misclassified them



                Hide();

                //bestMatchView.Show();
                qv.Show();
            }
            else
            {
                MessageBox.Show("Please input at least one image.");

            }

            // move to next form
        }

        private ColorfulContourMap AddCroppedImages(bool blackOrWhite, ColorfulContourMap cmap)
        {
            
            // get the image
            Mat img;
            if (blackOrWhite)
            {
                img = Form1.whiteSourceImages[cmap.imageIndex].Clone();
            }
            else
            {
                img = Form1.blackSourceImages[cmap.imageIndex].Clone();
            }
            // get the min max x y
            int minX = cmap.Center.X - cmap.Width/2;
            int minY = cmap.Center.Y - cmap.Height / 2;
            int maxX = cmap.Center.X + cmap.Width / 2;
            int maxY = cmap.Center.Y + cmap.Height / 2;





            // crop the corresponding image
            Mat result = new Mat(img, new Rectangle(new Point(minX, minY), new Size(maxX - minX, maxY - minY)));

            /*Mat result = new Mat(new Size(maxX-minX,maxY-minY),DepthType.Cv8U,3);
            CvInvoke.cvResetImageROI(img);
            CvInvoke.cvSetImageROI(img, new Rectangle(new Point(minX, minY), new Size(maxX - minX, maxY - minY)));
            CvInvoke.cvCopy(img, result,IntPtr.Zero);*/
            if (blackOrWhite)
            {
                CvInvoke.CopyMakeBorder(result, result, 100, 100, 100, 100, BorderType.Constant, new MCvScalar(255, 255, 255));
            }
            else
            {
                CvInvoke.CopyMakeBorder(result, result, 100, 100, 100, 100, BorderType.Constant, new MCvScalar(0, 0, 0));
            }

            // output the image
            //result = img;
            if (blackOrWhite)
            {
                Form1.whiteCroppedImages.Add(result);
                cmap = ColorfulContourMap.getAllContourMap(result,cmap.imageIndex,0)[0]; // update the contour map for the new image
            }
            else
            {
                Form1.blackCroppedImages.Add(result);
                cmap = ColorfulContourMap.getAllContourMap(result, cmap.imageIndex, 1)[0]; // update the contour map for the new image
            }
            return cmap;
        }
    }
}

