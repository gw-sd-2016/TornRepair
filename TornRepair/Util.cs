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
using System.Numerics;

namespace TornRepair
{
    //  http://stackoverflow.com/questions/5427020/prompt-dialog-in-windows-forms
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form();
            prompt.Width = 500;
            prompt.Height = 150;
            prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
            prompt.Text = caption;
            prompt.StartPosition = FormStartPosition.CenterScreen;
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
    public static class Util
    {
        // some constants used in this program
        
        public const double MIN_AREA = 10;
        public const int STEP = 10;
        public const int DELTA_THETA = 10;
        public const int MULT = 3;
        public const int MIN_TURN = 6;
        public const int THRESHOLD = 5000;

        // Calculate the turning angle

        public static double calcAngle(double p1_x, double p1_y, double p2_x, double p2_y, double p3_x, double p3_y)
        {
            double a_x = p1_x - p2_x;
            double a_y = p1_y - p2_y;

            double b_x = p3_x - p2_x;
            double b_y = p3_y - p2_y;
            double angle = (a_x * b_x + a_y * b_y) / (Math.Sqrt(a_x * a_x + a_y * a_y) * Math.Sqrt(b_x * b_x + b_y * b_y));
            if (angle > 1.0) return 0;
            else
                if (angle < -1.0) return 180.0;
            else
                return Math.Acos(angle) * (180.0 / Math.PI);
        }

        // calculate the sign of the turning angle
        public static int sign(double v)
        {
            return v > 0.0 ? 1 : (v < 0.0 ? -1 : 0);
        }

        public static List<Phi> replicateDNA(List<Phi> input)
        {
            List<Phi> linear = input.ToList();
            for (int i = 0; i < input.Count; ++i)
            {
                Phi temp;
                temp = input[i];
                temp.theta += 360; // might use this to get rid of negative numbers
                // also keeps the previous number if that is not negative
                linear.Add(temp);

            }
            return linear;
        }









        // Partial Match Algorithm
        public static Match partialMatch(List<Phi> DNAseq1, List<Phi> DNAseq2)
        {
            bool flag = true; // ToDo: Compare the control points in contours between two parts
            Match segment; // create an empty match segment
            segment.t11 = 0;
            segment.t12 = 0;
            segment.t21 = 0;
            segment.t22 = 0;

            List<Phi> seq1, seq2; // two empty List of edge maps
            int best = 0, max_match;
            int offset = 0, length = 0;
            if (DNAseq1.Count > DNAseq2.Count) // if the contour in first part has more control points than the second part
            {
                seq1 = replicateDNA(DNAseq1);//replicate the larger DNA

                seq2 = DNAseq2.ToList();//reverse the smaller one
                seq2.Reverse();
            }
            else
            {
                flag = false;
                seq1 = replicateDNA(DNAseq2); // if the first one has less control point, attach all the control points of the second part
                seq2 = DNAseq1.ToList();//reverse the smaller one
                seq2.Reverse();
            }

            for (int shift = 0; shift < seq1.Count - seq2.Count; shift += STEP)
            {
                List<int> diff = new List<int>();

                int start = 0, end = 0;
                for (int i = 0; i < seq2.Count; ++i)
                {
                    int difference = (int)(seq1[i + shift].theta - seq2[i].theta);
                    diff.Add(difference);
                }
                max_match = histogram(diff, seq2, ref start, ref end, DELTA_THETA);
                if (best < max_match)
                {
                    offset = shift;
                    best = max_match;
                    int t_start = start + shift;
                    int t_end = end + shift;
                    if (start + shift >= seq1.Count / 2)
                        t_start = start + shift - seq1.Count / 2;
                    if (end + shift >= seq1.Count / 2)
                        t_end = end + shift - seq1.Count / 2;
                    length = t_start - t_end;

                    if (flag)
                    {
                        segment.t21 = seq2.Count - end - 1;
                        segment.t22 = seq2.Count - start - 1;
                        segment.t11 = t_start;
                        segment.t12 = t_end;
                    }
                    else
                    {
                        segment.t11 = seq2.Count - end - 1;
                        segment.t12 = seq2.Count - start - 1;
                        segment.t21 = t_start;
                        segment.t22 = t_end;
                    }
                }
            }

            segment.x11 = (int)DNAseq1[segment.t11].x;
            segment.y11 = (int)DNAseq1[segment.t11].y;
            segment.x12 = (int)DNAseq1[segment.t12].x;
            segment.y12 = (int)DNAseq1[segment.t12].y;

            segment.x21 = (int)DNAseq2[segment.t21].x;
            segment.y21 = (int)DNAseq2[segment.t21].y;
            segment.x22 = (int)DNAseq2[segment.t22].x;
            segment.y22 = (int)DNAseq2[segment.t22].y;
            if (best == 0)
                segment.confidence = 0;
            else
                segment.confidence = Math.Sqrt((double)(length * length) / best);

            return segment;

        }

        private static int histogram(List<int> diff, List<Phi> seq, ref int t_start, ref int t_end, int delta_theta = 5)
        {
            int max_theta, min_theta;
            extreme(diff, out min_theta, out max_theta);

            int max_points = 0, range = 0, change = 0;
            int startt = 0, endd = 0;
            for (int i = min_theta; i <= max_theta - delta_theta; i += delta_theta)
            {
                int points = 0, _change = 0;
                List<int> thetas = new List<int>();
                bool flag = false;
                for (int j = 0; j < diff.Count; ++j)
                {
                    if (diff[j] >= i && diff[j] < i + delta_theta)
                    {
                        if (!flag)
                        {
                            startt = j;
                            flag = true;
                        }
                        endd = j;
                        points++;//Points that lie in sampling zone
                        thetas.Add((int)seq[j].theta);
                    }
                }

                //apply conditions
                _change = changes(thetas);
                if (max_points < points && _change > 3)
                {
                    max_points = points;
                    change = _change;
                    range = i;
                    t_start = startt;
                    t_end = endd;
                }
            }

            if (t_end - t_start > max_points * MULT)
            {
                int max_count = 0;
                int offset = 0;
                for (int shift = 0; shift < t_end - t_start - max_points * MULT; shift++)
                {
                    int count = 0;
                    for (int i = 0; i < max_points * MULT; ++i)
                    {
                        if (diff[t_start + i + shift] >= range && diff[t_start + i + shift] < range + delta_theta)
                            count++;
                    }
                    if (max_count < count)
                    {
                        max_count = count;
                        offset = shift;
                    }
                }
                t_start += offset;
                t_end = t_start + max_count * MULT;
            }
            return change;

        }

        private static int changes(List<int> X)
        {
            if (X.Count == 0)
                return 0;

            int initial = X[0];
            int count = 0;
            for (int i = 1; i < X.Count; ++i)
            {
                if (Math.Abs(X[i] - initial) > MIN_TURN)
                {
                    count++;
                    initial = X[i];
                }
            }
            return count;
        }

        private static void extreme(List<int> input, out int min, out int max)
        {
            max = input[0];
            min = input[0];
            for (int i = 1; i < input.Count; ++i)
            {
                if (max < input[i])
                    max = input[i];
                if (min > input[i])
                    min = input[i];
            }
        }


        //Compute desired trasformation matrix, rotate the fragment around the centroid for a certain angle
        // Since this works fine, centeroid calculated in this function should be correct
        public static void transformation(List<Phi> DNAseq1, List<Phi> DNAseq2, ref Match segment,
                       ref Point centroid1, ref Point centroid2, ref double angle)
        {
            centroid1.X = 0; centroid1.Y = 0;
            centroid2.X = 0; centroid2.Y = 0;
            for (int i = segment.t11; i <= segment.t12; ++i)
            {
                centroid1.X += (int)DNAseq1[i].x;
                centroid1.Y += (int)DNAseq1[i].y;
            }
            centroid1.X /= (segment.t12 - segment.t11 + 1);
            centroid1.Y /= (segment.t12 - segment.t11 + 1);

            for (int i = segment.t21; i <= segment.t22; ++i)
            {
                centroid2.X += (int)DNAseq2[i].x;
                centroid2.Y += (int)DNAseq2[i].y;
            }
            centroid2.X /= (segment.t22 - segment.t21 + 1);
            centroid2.Y /= (segment.t22 - segment.t21 + 1);

            Complex sum = new Complex(0, 0);
            // assume u=angle 1 on fragment 1, v=angle 2 on fragment 2
            for (int i = segment.t11, j = segment.t21; i <= segment.t12; ++i, ++j)
            {
                Complex u = new Complex(DNAseq1[i].x - centroid1.X, DNAseq1[i].y - centroid1.Y);
                Complex v = new Complex(DNAseq2[j].x - centroid2.X, DNAseq2[j].y - centroid2.Y); 
                sum += u * Complex.Conjugate(v); // this means angle 1+(-angle 2)=angle 1-angle 2
            }
            // get the average of all the angle differences, which is the angle need to rotate
            angle = sum.Phase; // this is in radian
        }

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


        public static void setPicBox(PictureBox p1, Image<Bgr,Byte> img1,PictureBox p2, Image<Bgr,Byte> img2)
        {
            p1.Image = img1.ToBitmap();
            p2.Image = img2.ToBitmap();
        }


        //Transform images according to transform matrix
        public static ReturnImg transform(Image<Gray, Byte> img1, Image<Gray, Byte> mask1, Image<Gray, Byte> img2, Image<Gray, Byte> mask2,
            Image<Gray, Byte> dst, Image<Gray, Byte> dst_mask,
                       Point centroid1, Point centroid2, double angle)
        {
            Image<Gray, Byte> E = img2.Clone();
            Image<Gray, Byte> E_mask = mask2.Clone();//Don't ruin original images


            
            double intersections = 0;
            double x = centroid2.X;
            double y = centroid2.Y;
            double _x, _y,_y2;
            double y2;
            LineSegment2D centerLine = new LineSegment2D(new Point((int)x, (int)y), new Point(img2.Width -(int)x, img2.Height -(int)y));
            //Rectangle r=new Rectangle((int)x,(int)y,2*(img2.Width-(int)x),2*(img2.Height-(int)y));
            Image<Gray, byte> ri = new Image<Gray, byte>(2 * (img2.Width - (int)x), 2 * (img2.Height - (int)y),new Gray(127));
            
            Point oldc = new Point((int)x, (int)y);
            // inverse y axis
           // y2 = -y;
            // rotation of centeroid 

            x -= img2.Width / 2;
            y -= img2.Height / 2;//shift origin to (w/2,h/2)
            _x = x * Math.Cos(angle/(180/Math.PI)) - y * Math.Sin(angle/ (180 / Math.PI));//rotate by theta
            _y = x * Math.Sin(angle/ (180 / Math.PI)) + y * Math.Cos(angle/ (180 / Math.PI));
            
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
            E = E.Rotate(angle, new Gray(255)); // actual rotation happens here
            E_mask = E_mask.Rotate(angle, new Gray(255));


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

            //optimal_h = 1000;
            //optimal_w = 1000;
            dst = new Image<Gray, byte>(optimal_w, optimal_h);
            dst_mask = new Image<Gray, byte>(optimal_w, optimal_h);

            /*if (BKG_WHITE)
                cvSet(dst, cvScalar(255));//make it white
            else
                cvSet(dst, cvScalar(0));//make it black*/
            dst.SetValue(255);
            dst_mask.SetZero();

            //Direct access wrappers
            /*BwImage canvas(dst);
            BwImage canvas_mask(dst_mask);
            BwImage image1(img1);
            BwImage image2(E);
            BwImage image1_msk(mask1);
            BwImage image2_msk(E_mask);*/



            //Apply transformation to image1
            /*t1.X = 0;
            t1.Y = 0;
            t2.X = 0;
            t2.Y = 0;*/
            for (int i = 0; i < img1.Height; ++i)
            {
                for (int j = 0; j < img1.Width; ++j)
                {
                    if (mask1.Data[i, j, 0] != 255)
                    {
                        int i_new = i + t1.Y;
                        int j_new = j + t1.X;
                        dst.Data[i_new, j_new, 0] = img1.Data[i, j, 0];
                        dst_mask.Data[i_new, j_new, 0] = 255;
                    }
                }
            }

            //Apply transformation to image2

            for (int i = 0; i < img2.Height; ++i)
            {
                for (int j = 0; j < img2.Width; ++j)
                {
                    if (E_mask.Data[i, j, 0] != 255)
                    {
                        int i_new = i  +t2.Y;
                        int j_new = j  +t2.X;
                        if (dst_mask.Data[i_new, j_new, 0] !=0)
                            intersections++;
                        else
                        {
                            dst.Data[i_new, j_new, 0] = E.Data[i, j, 0];
                            dst_mask.Data[i_new, j_new, 0] = 255;
                        }
                    }
                }
            }








            /*cvReleaseImage(&E);
            cvReleaseImage(&E_mask);*/ // should not need these two lines because of garbage collection

            // threshold detection is meaningless for 2-piece case, always success

            if (intersections > THRESHOLD)
            {
                /*cvReleaseImage(&dst);//In case of failure in joining
                cvReleaseImage(&dst_mask);//release memory*/
                ReturnImg img = new ReturnImg();
                img.img = dst;
                img.img_mask = dst_mask;
                img.source1 = img1;
                img.source2 = E_mask;
                img.center1 = centroid1;
                img.center2old = oldc;
                img.center2new = centroid2;
                img.centerLinee = centerLine;
                img.returnbool = false;
                return img;
            }
            else
            {
                ReturnImg img = new ReturnImg();
                img.img = dst;
                img.img_mask = dst_mask;
                img.source1 = img1;
                img.source2 = E_mask;
                img.center1 = centroid1;
                img.center2old = oldc;
                img.center2new = centroid2;
                img.centerLinee = centerLine;
                img.returnbool = true;
                return img;
            }
        }

        public static ReturnColorImg transformColor(Image<Bgr, Byte> img1, Image<Bgr, Byte> mask1, Image<Bgr, Byte> img2, Image<Bgr, Byte> mask2,
           Image<Bgr, Byte> dst, Image<Bgr, Byte> dst_mask,
                      Point centroid1, Point centroid2, double angle)
        {
            Image<Bgr, Byte> E = img2.Clone();
            Image<Bgr, Byte> E_mask = mask2.Clone();//Don't ruin original images



            double intersections = 0;
            double x = centroid2.X;
            double y = centroid2.Y;
            double _x, _y, _y2;
            double y2;
            
            LineSegment2D centerLine = new LineSegment2D(new Point((int)x, (int)y), new Point(img2.Width - (int)x, img2.Height - (int)y));
            //Rectangle r=new Rectangle((int)x,(int)y,2*(img2.Width-(int)x),2*(img2.Height-(int)y));
            Image<Bgr, byte> ri = new Image<Bgr, byte>(2 * (img2.Width - (int)x), 2 * (img2.Height - (int)y), new Bgr(255,255,255));

            Point oldc = new Point((int)x, (int)y);
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
            E = E.Rotate(angle, new Bgr(255,255,255)); // actual rotation happens here
            E_mask = E_mask.Rotate(angle, new Bgr(255,255,255));


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

            //optimal_h = 1000;
            //optimal_w = 1000;
            dst = new Image<Bgr, byte>(optimal_w, optimal_h);
            dst_mask = new Image<Bgr, byte>(optimal_w, optimal_h);

            /*if (BKG_WHITE)
                cvSet(dst, cvScalar(255));//make it white
            else
                cvSet(dst, cvScalar(0));//make it black*/
            dst.SetValue(255);
            dst_mask.SetZero();

            //Direct access wrappers
            /*BwImage canvas(dst);
            BwImage canvas_mask(dst_mask);
            BwImage image1(img1);
            BwImage image2(E);
            BwImage image1_msk(mask1);
            BwImage image2_msk(E_mask);*/



            //Apply transformation to image1
            /*t1.X = 0;
            t1.Y = 0;
            t2.X = 0;
            t2.Y = 0;*/
            for (int i = 0; i < img1.Height; ++i)
            {
                for (int j = 0; j < img1.Width; ++j)
                {
                    if (mask1.Data[i, j, 0] != 255)
                    {
                        int i_new = i + t1.Y;
                        int j_new = j + t1.X;
                        dst.Data[i_new, j_new, 0] = img1.Data[i, j, 0];
                        dst.Data[i_new, j_new, 1] = img1.Data[i, j, 1];
                        dst.Data[i_new, j_new, 2] = img1.Data[i, j, 2];
                        dst_mask.Data[i_new, j_new, 0] = 255;
                        dst_mask.Data[i_new, j_new, 1] = 255;
                        dst_mask.Data[i_new, j_new, 2] = 255;
                    }
                }
            }

            //Apply transformation to image2

            for (int i = 0; i < img2.Height; ++i)
            {
                for (int j = 0; j < img2.Width; ++j)
                {
                    if (E_mask.Data[i, j, 0] != 255)
                    {
                        int i_new = i + t2.Y;
                        int j_new = j + t2.X;
                        if (dst_mask.Data[i_new, j_new, 0] != 0)
                            intersections++;
                        else
                        {
                            dst.Data[i_new, j_new, 0] = E.Data[i, j, 0];
                            dst.Data[i_new, j_new, 1] = E.Data[i, j, 1];
                            dst.Data[i_new, j_new, 2] = E.Data[i, j, 2];
                            dst_mask.Data[i_new, j_new, 0] = 255;
                            dst_mask.Data[i_new, j_new, 1] = 255;
                            dst_mask.Data[i_new, j_new, 2] = 255;
                        }
                    }
                }
            }








            /*cvReleaseImage(&E);
            cvReleaseImage(&E_mask);*/ // should not need these two lines because of garbage collection

            // threshold detection is meaningless for 2-piece case, always success

            if (intersections > THRESHOLD)
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
                img.returnbool = false;
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
                img.returnbool = true;
                return img;
            }
        }


    }
}

