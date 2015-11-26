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
    public class ColorfulContourMap : ContourMap
    {
        new public List<ColorfulPoint>  _points;
        public ColorfulContourMap()
        {

        }

        public ColorfulContourMap(List<ColorfulPoint> p)
        {
            _points = p;
        }

        public void DrawTo(Image<Bgr, byte> input)
        {
            foreach (ColorfulPoint p in _points)
            {
                input.Draw(new CircleF(new PointF(p.X, p.Y), 1), p.color, 2);
            }
        }

    }
}
