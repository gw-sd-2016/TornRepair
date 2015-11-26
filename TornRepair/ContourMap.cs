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
    public class ContourMap
    {
        public List<Point> _points { get; set; }
        public int Length { get; internal set; }

        public ContourMap()
        {

        }
        public ContourMap(List<Point> p)
        {
            _points = p;
            Length = p.Count;
        }

        public ContourMap(Point[] p)
        {
            _points = p.ToList();
            Length = p.Length;
        }
        // method for ContourMap[i]
        public Point this[int i]
        {
            get
            {
                return _points[i];
            }
        }

        // transformations, also used in all the subclasses
        /*
        // translate
        public void Translate(int x, int y)
        {
            

        }

        // rotate, angle is in degree
        public void Rotate(double angle)
        {

        }

        // scale
        public void Scale(double x, double y)
        {

        }
        // scale to a size
        public void ScaleTo(int x, int y)
        {

        }*/
        


    }
}
