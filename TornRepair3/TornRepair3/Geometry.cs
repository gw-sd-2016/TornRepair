using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornRepair3
{
    public static class Geometry
    {
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
    }
}
