using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// The code for some of the data structure below is from a C++ code written by
// Amiya Patanaik, Bibek Behera and Sukadeb Acharya - IIT Kharagpur - India.
// http://aptnk.in/2008/08/automated-mosaicing-of-torn-paper-documents/
// Because the original code fails to detect the matching edge for straight edges, I added the color on the edge as another metric
// for detecting matching edge

namespace TornRepair3
{

    // used to represent the color on a point
    public struct ColorfulPoint
    {
        public int X;
        public int Y;
        public Bgr color; // Color of the edge


    }
    // from their phi data structure (Line 53-57), used to represent the turning angle on a point of a matching edge
    // but the edge color is not from their code, it is used in my own color matching algorithm
    public struct Phi
    {
        public double x; // x position
        public double y; // y position
        public double theta; // turning angle
        public int l; // arc length
        public Bgr color; // edge color
    }
    // from their match data structure (Line 59-65), used to represent the attributes of a matching edge
    // the ToString() method is not in their code, it is used for my own debugging purpose
    public struct Match
    {
        //Match parameters


        public int t11, t12;//starting and ending arc length parameter for piece1
        public int t21, t22;//starting and ending arc length parameter for piece2
        public int x11, y11, x12, y12; //starting and ending coordinates for piece1
        public int x21, y21, x22, y22; //starting and ending coordinates for piece2
        public double confidence;

        new public String ToString()
        {
            return String.Format("Piece 1:({0},{1})->({2},{3})", x11, y11, x12, y12) + "\n" +
                String.Format("Piece 2:({0},{1})->({2},{3})", x21, y21, x22, y22);
        }

    }

   
    // The data structure for the result of a 2-piece match
    public struct ReturnColorImg
    {
        public Mat img;
        public Mat img_mask;
        public Mat source1;
        public Mat source2;
        public Point center1;
        public Point center2old;
        public Point center2new;
        public LineSegment2D centerLinee;
        public Mat rimg;
        public bool returnbool;
        public Point translate1; // t1
        public Point translate2; // t2
        public double overlap;
        public bool success;
    }
    // used for matching history
    public struct MatchHistoryData
    {
        public Image<Bgr, byte> img1;
        public Image<Bgr, byte> img2;
        public double confident;
        public double overlap;
    }
    // used for the metrics for matching
    public struct MatchMetricData
    {
        public ColorfulContourMap map1;
        public ColorfulContourMap map2;
        public double confident;
        public double overlap;
        public List<Phi> dna1;
        public List<Phi> dna2;
        public Match match;
    }

}
