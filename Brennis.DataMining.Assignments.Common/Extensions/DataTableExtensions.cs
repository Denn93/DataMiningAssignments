using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Brennis.DataMining.Assignments.Common.Extensions
{
    public static class DataTableExtensions
    {
        public static void AddColumns(this DataTable table, string[] values)
        {
            foreach (string value in values)
                table.Columns.Add(value);
        }

        public static string GetProbabilityByTargetValue(this DataTable dt, KeyValuePair<string, string> predictorPair,
            KeyValuePair<string, string> targetPair)
        {
            if (dt.Columns[0].ColumnName == targetPair.Key || dt.Columns[0].ColumnName != predictorPair.Key)
                return string.Empty;
            
            foreach (DataRow row in dt.Rows.Cast<DataRow>().Where(row => row.ItemArray[0].Equals(predictorPair.Value)))
            {
                int columnIndex = dt.Columns.IndexOf(dt.Columns[targetPair.Value]);
                return " " + row.ItemArray[columnIndex];
            }

            return string.Empty;
        }
    }
}