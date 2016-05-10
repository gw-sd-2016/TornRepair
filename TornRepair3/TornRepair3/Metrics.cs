using ColorMine.ColorSpaces.Comparisons;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornRepair3
{
    public static class Metrics
    {
        public static double Whiteness(Bgr color)
        {
            // convert CV color format into C# color format
            Color c = Color.FromArgb((int)color.Red, (int)color.Green, (int)color.Blue);
            // get saturation and brightness
            float s = c.GetSaturation();
            float b = c.GetBrightness();

            // get whiteness
            return Math.Sqrt(s * s + (1 - b) * (1 - b));
        }
        // compare two Emgu CV colors using ColorMine, an API for C#
        public static int colorDifference(Bgr c1, Bgr c2)
        {
            var myRGB1 = new ColorMine.ColorSpaces.Rgb { R = c1.Red, G = c1.Green, B = c1.Blue };
            var myRGB2 = new ColorMine.ColorSpaces.Rgb { R = c2.Red, G = c2.Green, B = c2.Blue };

            return (int)myRGB1.Compare(myRGB2, new Cie1976Comparison());
        }
    }
}
