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
    public static class Metrics
    {
        public static double Whiteness(Bgr color)
        {
            // convert CV color format into C# color format
            Color c = Color.FromArgb((int)color.Red, (int)color.Green, (int)color.Blue);
            // get saturation and brightness
            float s = c.GetSaturation();
            float b = c.GetBrightness();
            // get whiteness
            return Math.Sqrt(s*s+(1- b)*(1- b));
        }
    }
    public static class MyUtil
    {
        public static ContourMap getMaxContourMap(Image<Gray, byte> input)
        {
            ContourMap result = new ContourMap();
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
                result = new ContourMap(maxArea.ToArray());
            }
            return result;
        }
        // get colorful contour using cross sampling
        public static ColorfulContourMap getColorfulContour(ContourMap edge, Image<Bgr, byte> input,int shift=0)
        {
            ColorfulContourMap cmap;
            List<ColorfulPoint> result=new List<ColorfulPoint>();
            foreach(Point p in edge._points)
            {
                ColorfulPoint cp = new ColorfulPoint();
                cp.X = p.X;
                cp.Y = p.Y;
                // color=(b->0,g->1,r->2)
               // Bgr color = new Bgr(input.Data[p.Y + shift, p.X + shift, 0], input.Data[p.Y + shift, p.X + shift, 1], input.Data[p.Y + shift, p.X + shift, 2]);
                // Add all colors of nearby pixels
                HashSet<Bgr> nearbyColors = new HashSet<Bgr>();
                
                for(int i=p.X-shift; i<=p.X+shift; i++)
                {
                    if (i >= 0 && i < input.Width)
                    {
                        nearbyColors.Add(new Bgr(input.Data[p.Y, i, 0], input.Data[p.Y,i, 1], input.Data[p.Y, i, 2]));
                    }
                }
                for(int i=p.Y-shift; i<=p.Y+shift; i++)
                {
                    if (i >= 0 && i < input.Height)
                    {
                        nearbyColors.Add(new Bgr(input.Data[i, p.X, 0], input.Data[i, p.X, 1], input.Data[i, p.X, 2]));
                    }

                }
                // check the whiteness of nearby pixels, use the less whiteness pixel as edge color
                double maxWhiteness = 0; // check max
                int index = 0;
                int maxIndex = 0;
                foreach (Bgr c in nearbyColors)
                {
                    double whiteness = Metrics.Whiteness(c);
                    if (whiteness>maxWhiteness) {
                        maxWhiteness = whiteness;
                        maxIndex = index;
                    }
                    index++;
                }




                //Color ccolor = Color.FromArgb((int)color.Red, (int)color.Green, (int)color.Blue);
                
                

                cp.color = nearbyColors.ElementAt(maxIndex);
                result.Add(cp);
               
               

            }
            cmap = new ColorfulContourMap(result);
            return cmap;
        }
        // get colorful contour use area sampling
        public static ColorfulContourMap getColorfulContourAreaSample(ContourMap edge, Image<Bgr, byte> input, int shift = 0)
        {
            ColorfulContourMap cmap;
            List<ColorfulPoint> result = new List<ColorfulPoint>();
            foreach (Point p in edge._points)
            {
                ColorfulPoint cp = new ColorfulPoint();
                cp.X = p.X;
                cp.Y = p.Y;
                // color=(b->0,g->1,r->2)
                // Bgr color = new Bgr(input.Data[p.Y + shift, p.X + shift, 0], input.Data[p.Y + shift, p.X + shift, 1], input.Data[p.Y + shift, p.X + shift, 2]);
                // Add all colors of nearby pixels
                HashSet<Bgr> nearbyColors = new HashSet<Bgr>();

                for (int i = p.X - shift; i <= p.X + shift; i++)
                {
                    for (int j = p.Y - shift; j <= p.Y + shift; j++)
                    {
                        if (i >= 0 && i < input.Width)
                        {
                            if (j >= 0 && j < input.Height)
                            {
                                nearbyColors.Add(new Bgr(input.Data[j, i, 0], input.Data[j, i, 1], input.Data[j, i, 2]));
                            }
                        }
                    }
                }
               
                // check the whiteness of nearby pixels, use the less whiteness pixel as edge color
                double maxWhiteness = 0; // check max
                int index = 0;
                int maxIndex = 0;
                foreach (Bgr c in nearbyColors)
                {
                    double whiteness = Metrics.Whiteness(c);
                    if (whiteness > maxWhiteness)
                    {
                        maxWhiteness = whiteness;
                        maxIndex = index;
                    }
                    index++;
                }




                //Color ccolor = Color.FromArgb((int)color.Red, (int)color.Green, (int)color.Blue);



                cp.color = nearbyColors.ElementAt(maxIndex);
                result.Add(cp);


            }
            cmap = new ColorfulContourMap(result);
            return cmap;
        }

        // get color using circle sampling
        public static ColorfulContourMap getColorfulContourCircleSample(ContourMap edge, Image<Bgr, byte> input, int shift = 0)
        {
            ColorfulContourMap cmap;
            List<ColorfulPoint> result = new List<ColorfulPoint>();
            foreach (Point p in edge._points)
            {
                ColorfulPoint cp = new ColorfulPoint();
                cp.X = p.X;
                cp.Y = p.Y;
                // color=(b->0,g->1,r->2)
                // Bgr color = new Bgr(input.Data[p.Y + shift, p.X + shift, 0], input.Data[p.Y + shift, p.X + shift, 1], input.Data[p.Y + shift, p.X + shift, 2]);
                // Add all colors of nearby pixels
                HashSet<Bgr> nearbyColors = new HashSet<Bgr>();

                for (int i = p.X - shift; i <= p.X + shift; i++)
                {
                    for (int j = p.Y - shift; j <= p.Y + shift; j++)
                    {
                        if (i >= 0 && i < input.Width)
                        {
                            if (j >= 0 && j < input.Height)
                            {
                                if ((i-p.X)*(i-p.X)+(j-p.Y)*(j-p.Y)<=shift*shift) {
                                    nearbyColors.Add(new Bgr(input.Data[j, i, 0], input.Data[j, i, 1], input.Data[j, i, 2]));
                                }
                            }
                        }
                    }
                }

                // check the whiteness of nearby pixels, use the less whiteness pixel as edge color
                double maxWhiteness = 0; // check max
                int index = 0;
                int maxIndex = 0;
                foreach (Bgr c in nearbyColors)
                {
                    double whiteness = Metrics.Whiteness(c);
                    if (whiteness > maxWhiteness)
                    {
                        maxWhiteness = whiteness;
                        maxIndex = index;
                    }
                    index++;
                }




                //Color ccolor = Color.FromArgb((int)color.Red, (int)color.Green, (int)color.Blue);



                cp.color = nearbyColors.ElementAt(maxIndex);
                result.Add(cp);

            }
            cmap = new ColorfulContourMap(result);
            return cmap;
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
