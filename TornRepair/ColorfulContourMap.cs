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
    // I just put the inheritance here, for now there is no actual inheritance
    public class ColorfulContourMap : ContourMap
    {
        new public List<ColorfulPoint>  _points;
        new public List<ColorfulPoint> _polyPoints;
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public Point Center { get; internal set; }

        // no use at all, just to bypass the IDE error checking
        public ColorfulContourMap()
        {

        }

        public ColorfulContourMap(List<ColorfulPoint> p)
        {
            _points = p;
            int minX = 99999;
            int maxX = -99999;
            int minY = 99999;
            int maxY = -99999;
            foreach(ColorfulPoint pp in p)
            {
                if (pp.X > maxX)
                {
                    maxX = pp.X;
                }
                if (pp.Y > maxY)
                {
                    maxY = pp.Y;
                }
                if (pp.X < minX)
                {
                    minX = pp.X;
                }
                if (pp.Y < minY)
                {
                    minY = pp.X;
                }
            }
            Height = maxY - minY;
            Width = maxX - minX;
            Center = new Point((maxX + minX) / 2, (maxY + minY) / 2);
            

        }

        public void DrawTo(Image<Bgr, byte> input)
        {
            foreach (ColorfulPoint p in _points)
            {
                input.Draw(new CircleF(new PointF(p.X, p.Y), 1), p.color, 2);
            }
        }

        // transformations, plan to inherit in base class but have some technical difficulties for now
        // translate
        public void Translate(int x, int y)
        {
           for(int i=0; i < _points.Count; i++)
            {
                ColorfulPoint p = _points[i];
                p.X += x;
                p.Y += y;
                _points[i] = p;

            }

        }

        // rotate, angle is in degree
        public void Rotate(double angle)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                ColorfulPoint p = _points[i];
                p.X -= Center.X;
                p.Y -= Center.Y;
                int _px, _py;
                _px = (int)(p.X * Math.Cos(angle / (180 / Math.PI)) - p.Y * Math.Sin(angle / (180 / Math.PI)));//rotate by theta
                _py = (int)(p.X * Math.Sin(angle / (180 / Math.PI)) + p.Y * Math.Cos(angle / (180 / Math.PI)));
                p.X = _px + Center.X;
                p.Y = _py + Center.Y;
                _points[i] = p;

            }

        }

        public void RotateAboutImage(double angle,Image<Bgr,byte> input)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                ColorfulPoint p = _points[i];
                p.X -= input.Width/2;
                p.Y -= input.Height/2;
                int _px, _py;
                _px = (int)(p.X * Math.Cos(angle / (180 / Math.PI)) - p.Y * Math.Sin(angle / (180 / Math.PI)));//rotate by theta
                _py = (int)(p.X * Math.Sin(angle / (180 / Math.PI)) + p.Y * Math.Cos(angle / (180 / Math.PI)));
                p.X = _px + input.Width/2;
                p.Y = _py + input.Height/2;
                _points[i] = p;

            }

        }

        // scale, although this will compromise the accuracy of the contour map
        public void Scale(double x, double y=-1)
        {
            if (y<0)
            {
                y = x;
            }
            for (int i = 0; i < _points.Count; i++)
            {
                ColorfulPoint p = _points[i];
                p.X = (int)(p.X * x);
                p.Y = (int)(p.Y * y);
                _points[i] = p;

            }

        }
        // scale to a size
        public void ScaleTo(int x, int y)
        {
           

        }

        // extract this contour map into DNA, the feature map
        List<Phi> extractDNA()
        {
            return null;
        }

        public ColorfulContourMap Clone()
        {
            ColorfulContourMap cmap=new ColorfulContourMap();
            cmap._points=_points;
            cmap. _polyPoints=_polyPoints;
            cmap.Width = Width;
            cmap.Height = Height;
            cmap.Center = Center;
            return cmap;
    }


    }
}
