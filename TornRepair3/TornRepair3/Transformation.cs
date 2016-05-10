using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// The code in this class  is from a C++ code written by
// Amiya Patanaik, Bibek Behera and Sukadeb Acharya - IIT Kharagpur - India. 
// http://aptnk.in/2008/08/automated-mosaicing-of-torn-paper-documents/ The file name is iConnect.cpp
// Because the original code fails to detect the matching edge for straight edges, I added the color on the edge as another metric
// for detecting matching edge


namespace TornRepair3
{
    // this class provides all the methods used in image transformation
    // most of the code comes from Line 67-240 in their code
    // the only exception is the data structure for returned result
    // I created the data structure because of the language differences between C# and C++
    public static class Transformation
    {
        //Compute desired trasformation matrix, rotate the fragment around the centroid for a certain angle
        // Since this works fine, centeroid calculated in this function should be correct
        // From Line 101-127
        public static void transformation(List<Phi> DNAseq1, List<Phi> DNAseq2, ref Match segment,
                       ref Point centroid1, ref Point centroid2, ref double angle)
        {
            centroid1.X = 0; centroid1.Y = 0;
            centroid2.X = 0; centroid2.Y = 0;
            if (segment.t11 > segment.t12)
            {
                for (int i = segment.t12; i <= segment.t11; ++i)
                {
                    centroid1.X += (int)DNAseq1[i].x;
                    centroid1.Y += (int)DNAseq1[i].y;
                }
            }
            else
            {
                for (int i = segment.t11; i <= segment.t12; ++i)
                {
                    centroid1.X += (int)DNAseq1[i].x;
                    centroid1.Y += (int)DNAseq1[i].y;
                }
            }
            centroid1.X /= Math.Abs(segment.t12 - segment.t11 + 1);
            centroid1.Y /= Math.Abs(segment.t12 - segment.t11 + 1);

            if (segment.t22 < segment.t21)
            {
                for (int i = segment.t22; i <= segment.t21; ++i)
                {
                    centroid2.X += (int)DNAseq2[i].x;
                    centroid2.Y += (int)DNAseq2[i].y;
                }
            }
            else
            {
                for (int i = segment.t21; i <= segment.t22; ++i)
                {
                    centroid2.X += (int)DNAseq2[i].x;
                    centroid2.Y += (int)DNAseq2[i].y;
                }
            }
            centroid2.X /= Math.Abs(segment.t22 - segment.t21 + 1);
            centroid2.Y /= Math.Abs(segment.t22 - segment.t21 + 1);

            Complex sum = new Complex(0, 0);
            // assume u=angle 1 on fragment 1, v=angle 2 on fragment 2
            for (int i = Math.Min(segment.t11, segment.t12), j = Math.Min(segment.t21, segment.t22); i <= Math.Max(segment.t12, segment.t11); ++i, ++j)
            {
                Complex u = new Complex(DNAseq1[i].x - centroid1.X, DNAseq1[i].y - centroid1.Y);
                Complex v = new Complex(DNAseq2[j].x - centroid2.X, DNAseq2[j].y - centroid2.Y);
                sum += u * Complex.Conjugate(v); // this means angle 1+(-angle 2)=angle 1-angle 2
            }
            // get the average of all the angle differences, which is the angle need to rotate
            angle = sum.Phase; // this is in radian
            Console.WriteLine(String.Format("Center1:({0},{1})", centroid1.X, centroid1.Y));
            Console.WriteLine(String.Format("Center2:({0},{1})", centroid2.X, centroid2.Y));
            Console.WriteLine("Turning Angle:" + angle);
        }
        // determine how the image will transform
        // From Line 68-80
        public static int quadrant(Point p)
        {
            if (p.X >= 0 && p.Y >= 0)
                return 1;
            else
                if (p.X < 0 && p.Y >= 0)
                return 2;
            else
                    if (p.X < 0 && p.Y < 0)
                return 3;
            else
                return 4;
        }


        public static void setPicBox(PictureBox p1, Mat img1, PictureBox p2, Mat img2)
        {
            p1.Image = img1.Bitmap;
            p2.Image = img2.Bitmap;
        }


        //Transform images according to transform matrix
        // From Line 130-239
       
        public static ReturnColorImg transformColor(Mat img1, Mat mask1, Mat img2, Mat mask2,
           Mat dst, Mat dst_mask,
                      Point centroid1, Point centroid2, double angle, Point tweak1, Point tweak2, bool mode = true)
        {
            Mat E = img2.Clone();
            Mat E_mask = mask2.Clone();//Don't ruin original images



            double intersections = 0;
            double x = centroid2.X;
            double y = centroid2.Y;
            double _x, _y, _y2;
            double y2;

            LineSegment2D centerLine = new LineSegment2D(new Point((int)x, (int)y), new Point(img2.Width - (int)x, img2.Height - (int)y));
            //Rectangle r=new Rectangle((int)x,(int)y,2*(img2.Width-(int)x),2*(img2.Height-(int)y));
            
            Mat ri = new Mat(2 * (img2.Width - (int)x), 2 * (img2.Height - (int)y),DepthType.Cv8U, 3);
            ri.SetTo(new MCvScalar(255, 255, 255));
            Point oldc = new Point((int)x, (int)y);
            bool success = false; // if the tweaking is not successful, return false
            // inverse y axis
            // y2 = -y;
            // rotation of centeroid 

            x -= img2.Width / 2;
            y -= img2.Height / 2;//shift origin to (w/2,h/2)
            _x = x * Math.Cos(angle / (180 / Math.PI)) - y * Math.Sin(angle / (180 / Math.PI));//rotate by theta
            _y = x * Math.Sin(angle / (180 / Math.PI)) + y * Math.Cos(angle / (180 / Math.PI));

            _x += img2.Width / 2;
            _y += img2.Height / 2;//back to origin

            //_x = x+img2.Width/2;
            //_y = y+img2.Height/2;

            // inverse y axis
            //_y = -_y2;


            centroid2.X = (int)_x;
            centroid2.Y = (int)_y;
            Point shift = new Point();
            shift.X = centroid1.X - centroid2.X;
            shift.Y = centroid1.Y - centroid2.Y;
            MatImage m1 = new MatImage(E);
            m1.Rotate(angle, new Bgr(255, 255, 255));
            E = m1.Out();

            MatImage m2 = new MatImage(E_mask);
            m1.Rotate(angle, new Bgr(255, 255, 255));
            E_mask = m2.Out();

            //Find optimal size of canvas to hold both images and appropriate transformations
            Point t1, t2;//transformation 1 and 2
            t1 = new Point();
            t2 = new Point();
            int optimal_h = 0, optimal_w = 0;//of canvas(IplImage* dst)
            switch (quadrant(shift))
            {
                case 1:
                    t1.X = 0;
                    t1.Y = 0;
                    t2 = shift;
                    optimal_h = Math.Max(img1.Height, img2.Height + shift.Y);
                    optimal_w = Math.Max(img1.Width, img2.Width + shift.X);
                    break;
                case 2:
                    t1.X = -shift.X;
                    t1.Y = 0;
                    t2.X = 0;
                    t2.Y = shift.Y;
                    optimal_h = Math.Max(img1.Height, img2.Height + shift.Y);
                    optimal_w = Math.Max(img2.Width, img1.Width - shift.X);
                    break;
                case 3:
                    t1.X = -shift.X;
                    t1.Y = -shift.Y;
                    t2.X = 0;
                    t2.Y = 0;
                    optimal_h = Math.Max(img1.Height - shift.Y, img2.Height);
                    optimal_w = Math.Max(img1.Width - shift.X, img2.Width);
                    break;
                case 4:
                    t1.X = 0;
                    t1.Y = -shift.Y;
                    t2.X = shift.X;
                    t2.Y = 0;
                    optimal_h = Math.Max(img1.Height - shift.Y, img2.Height);
                    optimal_w = Math.Max(img2.Width + shift.X, img1.Width);
                    break;
            }

            // add tweak factor
            t1.X += tweak1.X;
            t1.Y += tweak1.Y;
            t2.X += tweak2.X;
            t2.Y += tweak2.Y;


            //optimal_h = 1000;
            //optimal_w = 1000;
            dst = new Mat(optimal_h, optimal_w,DepthType.Cv8U,3);
            dst_mask = new Mat(optimal_h, optimal_w,DepthType.Cv8U,3);
           
            if (mode)
            {
                dst.SetTo(new MCvScalar(255, 255, 255)); // white background=255, black background=0
               
            }
            else
            {
                dst.SetTo(new MCvScalar(0, 0, 0)); // white background=255, black background=0
               
            }

            dst_mask.SetTo(new MCvScalar(0, 0, 0));

            
            

           


            /*if (BKG_WHITE)
                cvSet(dst, cvScalar(255));//make it white
            else
                cvSet(dst, cvScalar(0));//make it black*/
          


           
            for (int i = 0; i < img1.Height; ++i)
            {
                for (int j = 0; j < img1.Width; ++j)
                {
                    // if black background
                    if (mode)
                    {
                        if (mask1.GetData(i, j)[0] != 255)
                        {
                            int i_new = i + t1.Y;
                            int j_new = j + t1.X;
                            try
                            {
                                dst.SetValue(i_new, j_new, img1.GetData(i, j));
                                int[] vals = { 255, 255, 255 };
                                dst_mask.SetValue(i_new, j_new, vals);

                              

                                
                            }
                            catch(IndexOutOfRangeException e)
                            {
                                //MessageBox.Show("You cannot tweak in that direction further");
                                success = false;
                                goto ret;

                            }

                        }
                    }
                    // if white background
                    else
                    {
                        if (mask1.GetData(i, j) [0] != 0)
                        {
                            int i_new = i + t1.Y;
                            int j_new = j + t1.X;
                            try
                            {
                                dst.SetValue(i_new, j_new, img1.GetData(i, j));
                                int[] vals = { 0, 0, 0 };
                                dst_mask.SetValue(i_new, j_new, vals);

                               

                                
                            }
                            catch(IndexOutOfRangeException e)
                            {
                                //MessageBox.Show("You cannot tweak in that direction further");
                                success = false;
                                goto ret;

                            }

                        }
                    }

                }
            }
            
            //Apply transformation to image2

            for (int i = 0; i < img2.Height; ++i)
            {
                for (int j = 0; j < img2.Width; ++j)
                {
                    // if black background
                    if (mode)
                    {
                        if (E_mask.GetData(i, j) [0] != 255)
                        {
                            int i_new = i + t2.Y;
                            int j_new = j + t2.X;
                            try
                            {
                                if (dst_mask.GetData(i_new,j_new)[0] != 0)
                                {
                                    intersections++;
                                }
                                else
                                {
                                    dst.SetValue(i_new, j_new, E.GetData(i, j));
                                    int[] vals = { 255, 255, 255 };
                                    dst_mask.SetValue(i_new, j_new, vals);

                                   
                                   
                                }
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                //MessageBox.Show("You cannot tweak in that direction further");
                                success = false;
                                goto ret;
                            }


                        }
                    }
                    // else if white background
                    else
                    {
                        if (E_mask.GetData(i, j)[0] != 0)
                        {
                            int i_new = i + t2.Y;
                            int j_new = j + t2.X;
                            try
                            {
                                if (dst_mask.GetData(i_new, j_new)[0] != 0)
                                {
                                    intersections++;
                                }
                                else
                                {
                                    dst.SetValue(i_new, j_new, E.GetData(i, j));
                                    int[] vals = { 0, 0, 0 };
                                    dst_mask.SetValue(i_new, j_new, vals);

                                  

                                }
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                //MessageBox.Show("You cannot tweak in that direction further");
                                success = false;
                                goto ret;
                            }


                        }
                    }
                }
            }

            /*for (int i = 0; i < Adst.Length; i++)
            {
                for(int j=0; j < Adst[0].Length; j++)
                {
                    dst.SetValue(i, j, Adst[i][j]);
                }
            }
            for (int i = 0; i < Adst_mask.Length; i++)
            {
                for (int j = 0; j < Adst_mask[0].Length; j++)
                {
                    dst_mask.SetValue(i, j, Adst_mask[i][j]);
                }
            }*/
            // dst.SetTo(Adst);
            //dst_mask.SetTo(Adst_mask);


            success = true;

            /*cvReleaseImage(&E);
            cvReleaseImage(&E_mask);*/ // should not need these two lines because of garbage collection

            // threshold detection is meaningless for 2-piece case, always success

            ret:
            if (intersections > Constants.THRESHOLD)
            {
                /*cvReleaseImage(&dst);//In case of failure in joining
                cvReleaseImage(&dst_mask);//release memory*/
                ReturnColorImg img = new ReturnColorImg();
                img.img = dst;
                img.img_mask = dst_mask;
                img.source1 = img1;
                img.source2 = E_mask;
                img.center1 = centroid1;
                img.center2old = oldc;
                img.center2new = centroid2;
                img.centerLinee = centerLine;
                img.returnbool = false; // for determining if the image is matched or not
                img.translate1 = t1;
                img.translate2 = t2;
                img.overlap = intersections;
                img.success = success; // for tweak only
                return img;
            }
            else
            {
                ReturnColorImg img = new ReturnColorImg();
                img.img = dst;
                img.img_mask = dst_mask;
                img.source1 = img1;
                img.source2 = E_mask;
                img.center1 = centroid1;
                img.center2old = oldc;
                img.center2new = centroid2;
                img.centerLinee = centerLine;
                img.returnbool = true; // for determining if the image is matched or not
                img.translate1 = t1;
                img.translate2 = t2;
                img.overlap = intersections;
                img.success = success; // for tweak only
                return img;
            }
        }


    }
}




