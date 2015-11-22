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
using System.Threading;

namespace TornRepair
{
    public struct ColorfulPoint
    {
        public int X;
        public int Y;
        public Bgr color; // Color of the edge
    }
    public static class MyUtil
    {
        public static Point[] getMaxContourMap(Image<Gray, byte> input)
        {
            Point[] result = new Point[4];
            input = input.SmoothGaussian(3).ThresholdBinaryInv(new Gray(245), new Gray(255)).MorphologyEx(null, CV_MORPH_OP.CV_MOP_CLOSE, 2);
            using (MemStorage storage1 = new MemStorage())
            {
                Image<Gray, Byte> temp = input.Clone();
                Contour<Point> contour = temp.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE, RETR_TYPE.CV_RETR_EXTERNAL);
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
                result = maxArea.ToArray();
            }
            return result;
        }

        public static List<ColorfulPoint> getColorfulContour(Point[] edge, Image<Bgr, byte> input,int shift=1)
        {
            List<ColorfulPoint> result=new List<ColorfulPoint>();
            foreach(Point p in edge)
            {
                ColorfulPoint cp = new ColorfulPoint();
                cp.X = p.X;
                cp.Y = p.Y;
                // color=(b->0,g->1,r->2)
                
                
                Bgr color = new Bgr(input.Data[p.Y+shift, p.X+shift, 0], input.Data[p.Y+shift, p.X+shift, 1], input.Data[p.Y+shift, p.X+shift, 2]);
                Color ccolor = Color.FromArgb((int)color.Red, (int)color.Green, (int)color.Blue);
                
                

                cp.color = color;
                result.Add(cp);
               

            }
            return result;
        }

        public static void DrawColorfulContour(List<ColorfulPoint> edge,Image<Bgr,byte> input)
        {
            foreach(ColorfulPoint p in edge)
            {
                input.Draw(new CircleF(new PointF(p.X,p.Y),1), p.color, 2);
            }
        }

    }
}
