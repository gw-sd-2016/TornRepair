using Emgu.CV;
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
    public partial class DisplayImage : Form
    {
        public DisplayImage()
        {
            InitializeComponent();
        }
        public DisplayImage(Mat img, Point tweak1, Point tweak2, int overlap)
        {
            InitializeComponent();
            pictureBox1.Image = img.Bitmap;
            Text = String.Format("Tweak 1:({0},{1})  Tweak 2:({2},{3})  Overlap: {4}", tweak1.X, tweak1.Y, tweak2.X, tweak2.Y, overlap);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
