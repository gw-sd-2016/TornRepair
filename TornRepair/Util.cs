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
    public static class Util
    {
        // some constants used in this program
        public const double MIN_AREA = 10;
        public const int STEP = 10;
        public const int DELTA_THETA = 10;
        public const int MULT = 3;
        public const int MIN_TURN = 6;

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


        //Compute desired trasformation matrix, angle is the return value
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
            for (int i = segment.t11, j = segment.t21; i <= segment.t12; ++i, ++j)
            {
                Complex u = new Complex(DNAseq1[i].x - centroid1.X, DNAseq1[i].y - centroid1.Y);
                Complex v = new Complex(DNAseq2[j].x - centroid2.X, DNAseq2[j].y - centroid2.Y);
                sum += u * Complex.Conjugate(v);
            }
            angle = sum.Phase;
        }



        //Transform images according to transform matrix
        public static bool transform(Image<Gray,Byte> img1, Image<Gray, Byte> mask1, Image<Gray, Byte> img2, Image<Gray, Byte> mask2,
            Image<Gray, Byte> dst, Image<Gray, Byte> dst_mask,
                       Point centroid1, Point centroid2, double angle)
        {
            IplImage* E = cvCloneImage(img2);
            IplImage* E_mask = cvCloneImage(mask2);//Don't ruin original images
            double intersections = 0;
            double x = centroid2.x;
            double y = centroid2.y;
            double _x, _y;
            x -= img2->width / 2;
            y -= img2->height / 2;//shift origin to (w/2,h/2)
            _x = x * cos(-angle) - y * sin(-angle);//rotate by theta
            _y = x * sin(-angle) + y * cos(-angle);
            _x += img2->width / 2;
            _y += img2->height / 2;//back to origin
            centroid2.x = (int)_x;
            centroid2.y = (int)_y;
            CvPoint shift;
            shift.x = centroid1.x - centroid2.x;
            shift.y = centroid1.y - centroid2.y;
            rotate(E, (float)angle);
            rotate(E_mask, (float)angle);
            //Find optimal size of canvas to hold both images and appropriate transformations
            CvPoint t1, t2;//transformation 1 and 2
            int optimal_h, optimal_w;//of canvas(IplImage* dst)
            switch (quadrant(shift))
            {
                case 1:
                    t1.x = 0;
                    t1.y = 0;
                    t2 = shift;
                    optimal_h = max(img1->height, img2->height + shift.y);
                    optimal_w = max(img1->width, img2->width + shift.x);
                    break;
                case 2:
                    t1.x = -shift.x;
                    t1.y = 0;
                    t2.x = 0;
                    t2.y = shift.y;
                    optimal_h = max(img1->height, img2->height + shift.y);
                    optimal_w = max(img2->width, img1->width - shift.x);
                    break;
                case 3:
                    t1.x = -shift.x;
                    t1.y = -shift.y;
                    t2.x = 0;
                    t2.y = 0;
                    optimal_h = max(img1->height - shift.y, img2->height);
                    optimal_w = max(img1->width - shift.x, img2->width);
                    break;
                case 4:
                    t1.x = 0;
                    t1.y = -shift.y;
                    t2.x = shift.x;
                    t2.y = 0;
                    optimal_h = max(img1->height - shift.y, img2->height);
                    optimal_w = max(img2->width + shift.x, img1->width);
                    break;
            }
            dst = cvCreateImage(cvSize(optimal_w, optimal_h), IPL_DEPTH_8U, 1);
            dst_mask = cvCreateImage(cvSize(optimal_w, optimal_h), IPL_DEPTH_8U, 1);
            if (BKG_WHITE)
                cvSet(dst, cvScalar(255));//make it white
            else
                cvSet(dst, cvScalar(0));//make it black
            cvSet(dst_mask, cvScalar(0));//make it all zeros

            //Direct access wrappers
            BwImage canvas(dst);
            BwImage canvas_mask(dst_mask);
            BwImage image1(img1);
            BwImage image2(E);
            BwImage image1_msk(mask1);
            BwImage image2_msk(E_mask);

            //Apply transformation to image1
            for (int i = 0; i < img1->height; ++i)
                for (int j = 0; j < img1->width; ++j)
                    if (image1_msk[i][j])
                    {
                        int i_new = i + t1.y;
                        int j_new = j + t1.x;
                        canvas[i_new][j_new] = image1[i][j];
                        canvas_mask[i_new][j_new] = 255;
                    }

            //Apply transformation to image2
            for (int i = 0; i < img2->height; ++i)
                for (int j = 0; j < img2->width; ++j)
                    if (image2_msk[i][j])
                    {
                        int i_new = i + t2.y;
                        int j_new = j + t2.x;
                        if (canvas_mask[i_new][j_new])
                            intersections++;
                        else
                        {
                            canvas[i_new][j_new] = image2[i][j];
                            canvas_mask[i_new][j_new] = 255;
                        }
                    }


            cvReleaseImage(&E);
            cvReleaseImage(&E_mask);

            if (intersections > THRESHOLD)
            {
                cvReleaseImage(&dst);//In case of failure in joining
                cvReleaseImage(&dst_mask);//release memory
                return false;
            }
            else
                return true;
        }


    }
}

