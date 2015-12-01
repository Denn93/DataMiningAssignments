using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Brennis.DataMining.Assignments.Common;
using Brennis.DataMining.Assignments.DataAccess.Models;

namespace Brennis.DataMining.Assignments.DataAccess.ZeroRAlgorithm
{
    public class ZeroRAlgorithm : IZeroRAlgorithm
    {
        
        public void Process()
        {
            TargetColumn = StaticStorage.TargetColum;
            List<string> targetValues = StaticStorage.DataSet.AsEnumerable().Select(m => m[TargetColumn].ToString()).ToList();

            ResultSet =
                targetValues.GroupBy(m => m)
                    .Select(m => new ZeroRResultRow
                    {
                        Key = m.Key,
                        Count = m.Count(),
                        Total = targetValues.Count,
                        Probability = m.Count()/(double) targetValues.Count
                    }).ToList();
        }

        public void Print()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("ZeroRAlgorithm Result with targetColumn: {0}", TargetColumn);
            Console.WriteLine("-----------------------------------------------------------");

            Console.WriteLine("\tKey|Count|Total|Probability");

            for (int i = 0; i < ResultSet.Count; i++)
                Console.WriteLine("\t{0,-5}|{1,-5}|{2,-5}|{3,-5}|{4,-5}", i,
                    ResultSet[i].Key, ResultSet[i].Count, ResultSet[i].Total, ResultSet[i].Probability);
        }

        public string TargetColumn { get; private set; }

        public List<ZeroRResultRow> ResultSet { get; private set; }
    }
}
