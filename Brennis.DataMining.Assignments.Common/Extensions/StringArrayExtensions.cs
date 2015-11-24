using System.Collections.Generic;
using System.Data;
using System.Linq;
using Brennis.DataMining.Assignments.Common.Utils;

namespace Brennis.DataMining.Assignments.Common.Extensions
{
    public static class StringArrayExtensions
    {
        public static DataTable ToDataTable(this string[] lines, string tableName)
        {
            DataTable result = new DataTable(tableName);
            bool[] first = { true };
            bool checkForNumeric = false;
            List<int> numericColumns = new List<int>();

            foreach (object[] values in lines.Select(line => line.Split(',')))
            {
                if (!first[0])
                {
                    if (!checkForNumeric)
                    {
                        for (int i = 0; i < values.Length; i++)
                        {
                            int number;
                            int.TryParse(values[i].ToString(), out number);

                            if (number > 0 || values[i].ToString().Equals("0"))
                                numericColumns.Add(i);
                        }
                        checkForNumeric = true;
                    }

                    DataRow row = result.NewRow();
                    row.ItemArray = values;
                    result.Rows.Add(row);
                    continue;
                }

                foreach (string value in values.Where(value => first[0]))
                    result.Columns.Add(new DataColumn(value));

                first[0] = false;
            }

            foreach (int numericColumn in numericColumns)
            {
                double[] values = result.AsEnumerable().Select(m => double.Parse(m[numericColumn].ToString())).ToArray();
                double[] bins = IntervalCreation.MakeBins(values, 4);
                string[] newValues = values.Select(m => IntervalCreation.InBin(m, bins)).ToArray();

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    DataRow row = result.Rows[i];
                    row[numericColumn] = newValues[i];
                }
            }

            return result;
        }
    }
}
