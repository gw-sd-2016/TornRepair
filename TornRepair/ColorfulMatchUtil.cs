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
using ColorMine.ColorSpaces.Comparisons;

namespace TornRepair
{
    public static class ColorfulMatchUtil
    {
        // Partial Match Algorithm
        public static Match partialColorMatch(List<Phi> DNAseq1, List<Phi> DNAseq2)
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
                seq1 = Util.replicateDNA(DNAseq1);//replicate the larger DNA

                seq2 = DNAseq2.ToList();//reverse the smaller one
                seq2.Reverse();
            }
            else
            {
                flag = false;
                seq1 = Util.replicateDNA(DNAseq2); // if the first one has less control point, attach all the control points of the second part
                seq2 = DNAseq1.ToList();//reverse the smaller one
                seq2.Reverse();
            }
            List<int> zc=new List<int>();
            List<int> starts = new List<int>();
            for (int shift = 0; shift < seq1.Count - seq2.Count; shift += Util.STEP)
            {
                List<int> diff = new List<int>();
                bool flag1 = false;
                int start = 0, end = 0;
                // TODO: change the differences into color difference (done)
                int zeroCount = 0;
                for (int i = 0; i < seq2.Count; ++i)
                {
                    int difference = colorDifference(seq1[i + shift].color,seq2[i].color);
                    if (difference == 0)
                    {
                        if (!flag1)
                        {
                            flag1 = true;
                            start = i;
                            starts.Add(start);
                        }
                        zeroCount++;
                    }
                    diff.Add(difference);
                }
                zc.Add(zeroCount);
                if (zeroCount==0)
                {
                    starts.Add(-1);
                }
                // TTODO: implement a histogram algorithm for color match
                //max_match = colorHistogram(diff, seq2, ref start, ref end, Util.DELTA_THETA);
                max_match = 0;
                /*if (end < start)
                {
                    Console.WriteLine("22");
                }*/
                
               /* if (best < max_match)
                {
                    offset = shift;
                    best = max_match;
                    int t_start = start + shift;
                    int t_end = end + shift;
                    if (start + shift >= seq1.Count / 2)
                        t_start = start + shift - seq1.Count / 2;
                    if (end + shift >= seq1.Count / 2)
                        t_end = end + shift - seq1.Count / 2;
                    length = t_start - t_end; // problematic
                   

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
                } */
            }

            Console.WriteLine("Max:" + zc.Max());
            int t_shift=0;
            int s_start = 0;
            for(int i=0; i < zc.Count; i++)
            {
                if (zc[i] == zc.Max())
                {
                    t_shift = Util.STEP * i;
                    s_start = starts[i];
                }
            }
            int startPos1 = t_shift + s_start;
            int endPos1 = startPos1 + zc.Max();
            int startPos2 = s_start;
            int endPos2 = startPos2 + zc.Max();
            // check if the algorithm get the correct position of the matching color
            Console.WriteLine("Flag:" + flag);
            Console.WriteLine("Shiftreq:" +startPos1);
            Console.WriteLine("Count:" + DNAseq1.Count);

            Console.WriteLine("P1_start_x"+seq1[startPos1].x);
            Console.WriteLine("P1_start_y"+seq1[startPos1].y);
            Console.WriteLine("P1_end_x"+seq1[endPos1].x);
            Console.WriteLine("P1_end_y" + seq1[endPos1].y);

            Console.WriteLine("P2_start_x" + seq2[startPos2].x);
            Console.WriteLine("P2_start_y" + seq2[startPos2].y);
            Console.WriteLine("P2_end_x" + seq2[endPos2].x);
            Console.WriteLine("P2_end_y" + seq2[endPos2].y);


            int t_start = t_shift;
            int t_end = t_shift;
            if (t_shift >= seq1.Count / 2)
                t_start = t_shift - seq1.Count / 2;
            if (t_shift >= seq1.Count / 2)
                t_end = t_shift - seq1.Count / 2;
            length = t_start - t_end; // problematic


            if (flag)
            {
                segment.t21 = seq2.Count -  - 1;
                segment.t22 = seq2.Count - t_start - 1;
                segment.t11 = t_start;
                segment.t12 = t_end;
            }
            else
            {
                segment.t11 = seq2.Count - t_end - 1;
                segment.t12 = seq2.Count - t_start - 1;
                segment.t21 = t_start;
                segment.t22 = t_end;
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
        // compare two Emgu CV colors using ColorMine, an API for C#
        private static int colorDifference(Bgr c1, Bgr c2)
        {
            var myRGB1 = new ColorMine.ColorSpaces.Rgb { R = c1.Red, G = c1.Green, B = c1.Blue };
            var myRGB2 = new ColorMine.ColorSpaces.Rgb { R = c2.Red, G = c2.Green, B = c2.Blue };

            return (int)myRGB1.Compare(myRGB2, new Cie1976Comparison()); 
        }
        // Theta means the color difference in this case, the delta theta is 2 because the API said a difference of 2 is indifferent
        // for human perception
        private static int colorHistogram(List<int> diff, List<Phi> seq, ref int t_start, ref int t_end, int delta_theta = 2)
        {
            int max_theta, min_theta;
            extreme(diff, out min_theta, out max_theta); // checked, works

            int max_points = 0, range = 0, change = 0;
            int startt = 0, endd = 0;
            for (int i = min_theta; i <= max_theta - delta_theta; i += delta_theta)
            {
                int points = 0, _change = 0;
                List<Bgr> colors = new List<Bgr>();
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
                        colors.Add(seq[j].color);
                    }
                }

                //apply conditions
                _change = changes(colors);
                if (max_points < points && _change > 3)
                {
                    max_points = points;
                    change = _change;
                    range = i;
                    t_start = startt;
                    t_end = endd;
                }
            }

            if (t_end - t_start > max_points * Util.MULT)
            {
                int max_count = 0;
                int offset = 0;
                for (int shift = 0; shift < t_end - t_start - max_points * Util.MULT; shift++)
                {
                    int count = 0;
                    for (int i = 0; i < max_points * Util.MULT; ++i)
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
                t_end = t_start + max_count * Util.MULT;
            }
            return change;

        }
        // calculate significant changes for color value
        private static int changes(List<Bgr> X)
        {
            if (X.Count == 0)
                return 0;

            Bgr initial = X[0];
            int count = 0;
            for (int i = 1; i < X.Count; ++i)
            {
                if (colorDifference(X[i],initial) > Util.MIN_COLOR_SHIFT)
                {
                    count++;
                    initial = X[i];
                }
            }
            return count;
        }
        // this should work for color matching without changing the code
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

    }
}
