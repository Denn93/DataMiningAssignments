using System.Data;

namespace Brennis.DataMining.Assignments.Common.Extensions
{
    public static class DataTableExtensions
    {
        public static void AddColumns(this DataTable table, string[] values)
        {
            foreach (string value in values)
                table.Columns.Add(value);
        }
    }
}
