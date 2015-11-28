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

    // There is a subclass that implements colorful contour map, but that is not actually a subclass
    public class ContourMap
    {
        public List<Point> _points { get; set; }
        public List<Point> _polyPoints{get;set;}
        public int Length { get; internal set; }

        // bypass the IDE, no use at all
        public ContourMap()
        {

        }
        public ContourMap(List<Point> p,List<Point> poly)
        {
            _points = p;
            _polyPoints = poly;
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

        public void DrawTo(Image<Bgr, byte> input)
        {
            foreach (Point p in _points)
            {
                input.Draw(new CircleF(new PointF(p.X, p.Y), 1), new Bgr(255,0,0), 2);
            }
            foreach (Point p in _polyPoints)
            {
                input.Draw(new CircleF(new PointF(p.X, p.Y), 1), new Bgr(0, 0, 255), 2);
            }

        }

        public void DrawPolyTo(Image<Bgr,byte> input)
        {
            Point[] points = _polyPoints.ToArray();
            input.DrawPolyline(points, true, new Bgr(255, 0, 0), 2);
            foreach (Point p in _polyPoints)
            {
                input.Draw(new CircleF(new PointF(p.X, p.Y), 1), new Bgr(0, 0, 255), 2);
            }
        }

        // use list of phi for dna just for now, considering create a special class for DNA
        public List<Phi> extractDNA()
        {
            List<Phi> DNA=new List<Phi>(); // DNA for poly 
            List<Phi> DNAseq = new List<Phi>(); // DNA for all
            List<Phi> verticies=new List<Phi>();
            // start of extraction
            int i = 0;
            foreach (Point point in _points)
            {
                Phi tempPhi = new Phi();
                tempPhi.x = point.X;
                tempPhi.y = point.Y;
                tempPhi.theta = 0;
                tempPhi.l = i;
                DNAseq.Add(tempPhi);
                i++;
            }
           
           

          
            i = 0;
            foreach (Point point in _polyPoints)
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
                    for (int j = verticies[i].l; j <= verticies[next].l + Length; ++j)
                    {
                        int index = j;
                        if (index >= Length)
                        {
                            index -= Length;
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
            return DNA;
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
