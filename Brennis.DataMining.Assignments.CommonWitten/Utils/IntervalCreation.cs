namespace Brennis.DataMining.Assignments.CommonWitten.Utils
{
    public static class IntervalCreation
    {
        public static double[] MakeBins(double[] data, int numBins)
        {
            double max = data[0]; 
            double min = data[0];
            foreach (double value in data)
            {
                if (value < min) min = value;
                if (value > max) max = value;
            }
            double width = (max - min) / numBins; 

            double[] intervals = new double[numBins * 2]; 
            intervals[0] = min;
            intervals[1] = min + width;
            for (int i = 2; i < intervals.Length - 1; i += 2)
            {
                intervals[i] = intervals[i - 1];
                intervals[i + 1] = intervals[i] + width;
            }

            return intervals;
        }

        public static string InBin(double x, double[] intervals)
        {
            for (int i = 0; i < intervals.Length - 1; i += 2)
            {
                if (x >= intervals[i] && x <= intervals[i + 1])
                    return intervals[i] + "-" + intervals[i + 1];
            };
            return ""; 
        }
    }
}
