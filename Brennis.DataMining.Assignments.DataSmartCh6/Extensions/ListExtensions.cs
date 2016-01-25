using System.Collections.Generic;

namespace Brennis.DataMining.Assignments.DataSmartCh6.Extensions
{
    public static class ListExtensions
    {
        public static List<double> Sequence(this List<double> list, double start, double end, double steps)
        {
            for (double i = start; i <= (end - steps); i += steps)
                list.Add(i);

            list.Add(end);

            return list;
        }
    }
}
