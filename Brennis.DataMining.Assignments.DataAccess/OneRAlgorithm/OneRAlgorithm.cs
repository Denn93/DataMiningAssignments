using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Brennis.DataMining.Assignments.Common;
using Brennis.DataMining.Assignments.Common.Extensions;

namespace Brennis.DataMining.Assignments.DataAccess.OneRAlgorithm
{
    public class OneRAlgorithm : IOneRAlgorithm
    {
        public void Process()
        {
            
            string[] columnNames = StaticStorage.DataSet.Columns.Cast<DataColumn>()
                .Select(x => x.ColumnName)
                .ToArray();

            ResultTables =
                columnNames.Where((v, k) => StaticStorage.NormalDistributionValueItems.All(m => m.ColumnId != k))
                    .Select(CreateTable)
                    .ToList();

            ResultTables.AddRange(
                StaticStorage.NormalDistributionValueItems.GroupBy(m => m.ColumnId).Select(m => CreateTable(m.Key, m.ToList())));
        }

        private DataTable CreateTable(int columnId, List<NormalDistributionValueItem> items)
        {
            string[] targetValues = StaticStorage.DataSet.AsEnumerable().Select(m => m[StaticStorage.TargetColum].ToString().Format()).Where(m => m != "").ToArray();
            string[] targetDistinctValues = targetValues.Distinct().ToArray();

            string predictorColumn = StaticStorage.DataSet.Columns[columnId].ColumnName;
            string[] predictorValues = StaticStorage.DataSet.AsEnumerable().Select(m => m[predictorColumn].ToString().Format()).Where(m => m != "").ToArray();
            string[] predictorDistinctValues = predictorValues.Distinct().ToArray();

            DataTable result = new DataTable($"{StaticStorage.DataSet.Columns[columnId].ColumnName} - {StaticStorage.TargetColum}");
            result.Columns.Add(predictorColumn.Format());
            result.AddColumns(targetDistinctValues);

            foreach (string predictorDistinctValue in predictorDistinctValues)
            {
                DataRow row = result.NewRow();
                foreach (string targetDistinctValue in targetDistinctValues)
                {
                    row[predictorColumn.Format()] = predictorDistinctValue;

                    NormalDistributionValueItem item = items.First(m => m.TargetValue.Equals(targetDistinctValue));

/*                    double exponent =
                         Math.Exp(-(Math.Pow(double.Parse(predictorDistinctValue) - item.Mean, 2)/(2*Math.Pow(item.Std, 2))));
                    double probability = (1/(Math.Sqrt(2*Math.PI)*item.Std))*exponent;*/

                    

                    double zscore = (double.Parse(predictorDistinctValue) - item.Mean) / item.Std;
                    double exponent = -(zscore * zscore) / 2;
                    double probability = (1 / Math.Sqrt(2 * Math.PI)) * Math.Exp(exponent);

                    row[targetDistinctValue] = Math.Round(probability, 6);
                }
                result.Rows.Add(row);
            }

            return result;
        }

        private DataTable CreateTable(string predictorColumn)
        {
            DataTable result = new DataTable($"{StaticStorage.TargetColum} - {predictorColumn}");

            string[] targetValues = StaticStorage.DataSet.AsEnumerable().Select(m => m[StaticStorage.TargetColum].ToString().Format()).Where(m => m != "").ToArray();
            string[] predictorValues = StaticStorage.DataSet.AsEnumerable().Select(m => m[predictorColumn].ToString().Format()).Where(m => m != "").ToArray();

            string[] targetDistinctValues = targetValues.Distinct().ToArray();
            string[] predictorDistinctValues = predictorValues.Distinct().ToArray();

            result.Columns.Add(predictorColumn.Format());
            result.AddColumns(targetDistinctValues);
            result.Columns.Add("probability");

            foreach (string predictorDistinctValue in predictorDistinctValues)
            {
                if (StaticStorage.TargetColum == predictorColumn)
                    continue;

                DataRow row = result.NewRow();
                foreach (string targetDistinctValue in targetDistinctValues)
                {
                    string query =
                    $"{StaticStorage.TargetColum} = '{targetDistinctValue}' AND {predictorColumn} = '{predictorDistinctValue}'";

                    string totalQuery = $"{StaticStorage.TargetColum}='{targetDistinctValue}'";
                    row[targetDistinctValue] = StaticStorage.DataSet.Select(query).Length + "/" + StaticStorage.DataSet.Select(totalQuery).Length;
                }

                row["probability"] = targetDistinctValues.Sum(m => int.Parse(row[m].ToString().Split('/')[0]));

                row[predictorColumn] = predictorDistinctValue; 
                result.Rows.Add(row);
            }

            return result;
        }

        public void Print()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("OneRAlgorithm Result with targetColumn: {0}", StaticStorage.TargetColum);
            Console.WriteLine("-----------------------------------------------------------");
            Console.WriteLine();

            int dataSetCount = StaticStorage.DataSet.Rows.Count;
            foreach (DataTable table in ResultTables)
            {
                Console.WriteLine("Result DataTable: {0}", table.TableName);

                foreach (DataColumn dataColumn in table.Columns)
                    Console.Write("{0,-10} {1,2}", dataColumn.ColumnName, "|");

                Console.WriteLine();

                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < row.ItemArray.Length; i++)
                    {
                        if (i == 0)
                            Console.Write("{0,-10} {1,2}", row.ItemArray[0], "|");
                        else if (i > 0 && i < row.ItemArray.Length - 1)
                        {
                            Console.Write("{0,-10} {1,2}", row.ItemArray[i], "|");
                        }
                        else
                        {
                            if (row.ItemArray.Length > 3)
                                Console.Write("{0,-10} {1,2}",
                                    row.ItemArray[i] + "/" + dataSetCount + "= " +
                                    Math.Round(int.Parse(row.ItemArray[i].ToString())/(double) dataSetCount, 2), "|");
                            else
                                Console.Write("{0,-10} {1,2}", row.ItemArray[i], "|");
                        }
                            
                    }
                    
                    Console.WriteLine();
                }

                Console.WriteLine();
                Console.WriteLine("-----------------------------------------------------------");
                Console.WriteLine();
            }
        }

        public List<DataTable> ResultTables { get; private set; }
    }
}
