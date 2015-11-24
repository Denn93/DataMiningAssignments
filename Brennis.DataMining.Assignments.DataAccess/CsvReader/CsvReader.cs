using System.Data;
using System.IO;
using System.Reflection;
using Brennis.DataMining.Assignments.Common.Extensions;

namespace Brennis.DataMining.Assignments.DataAccess.CsvReader
{
    public class CsvReader : ICsvReader
    {
        public DataTable ReadToDataTable(string fileName, string tableName)
        {
            DataTable result = new DataTable(tableName);
            fileName = "Resources\\" + fileName;

            string executableLocation = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
            string fileLocation = null;
            if (executableLocation != null)
                fileLocation = Path.Combine(executableLocation, fileName);

            return (fileLocation != null)
                ? File.ReadAllLines(fileLocation).ToDataTable(tableName)
                : result;
        }

        private static double[] MakeBins(double[] data, int numBins)
        {
            double max = data[0]; // find min & max
            double min = data[0];
            foreach (double value in data)
            {
                if (value < min) min = value;
                if (value > max) max = value;
            }
            double width = (max - min) / numBins; // compute width

            double[] intervals = new double[numBins * 2]; // intervals
            intervals[0] = min;
            intervals[1] = min + width;
            for (int i = 2; i < intervals.Length - 1; i += 2)
            {
                intervals[i] = intervals[i - 1];
                intervals[i + 1] = intervals[i] + width;
            }
            intervals[0] = double.MinValue; // outliers
            intervals[intervals.Length - 1] = double.MaxValue;

            return intervals;
        }
    }
}
