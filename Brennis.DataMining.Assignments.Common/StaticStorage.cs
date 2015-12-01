using System.Collections.Generic;
using System.Data;

namespace Brennis.DataMining.Assignments.Common
{
    public static class StaticStorage
    {
        public static List<NormalDistributionValueItem> NormalDistributionValueItems { get; set; }
        public static string TargetColum { get; set; }
        public static DataTable DataSet { get; set; }
    }
}
