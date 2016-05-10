using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornRepair3
{
    public static class Constants
    {
        public const double MIN_AREA = 5000;
        public const int STEP = 10;
        public const int DELTA_THETA = 10;
        public const int MULT = 3;
        public const int MIN_TURN = 6;
        public const int THRESHOLD = 8000;
        public const int MIN_COLOR_SHIFT = 2; // as the API said
        public const int COLOR_TOLERANCE = 3;
        public const int AREA_SHIFT = 3;
        public const int EDGE_COLOR_DIFFERENCE = 40;
        public const int MIN_CONFIDENCE = 20;
    }
}
