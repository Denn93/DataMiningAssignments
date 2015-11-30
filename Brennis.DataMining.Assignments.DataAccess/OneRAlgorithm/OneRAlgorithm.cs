using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Brennis.DataMining.Assignments.Common;
using Brennis.DataMining.Assignments.Common.Enum;
using Brennis.DataMining.Assignments.Common.Extensions;

namespace Brennis.DataMining.Assignments.DataAccess.OneRAlgorithm
{
    public class OneRAlgorithm : IOneRAlgorithm
    {
        public void Process(DataTable data, string targetColumn)
        {
            TargetColumn = targetColumn;
            DataSet = data;

            string[] columnNames = data.Columns.Cast<DataColumn>()
                .Select(x => x.ColumnName)
                .ToArray();

            ResultTables =
                columnNames.Where((v, k) => StaticStorage.NormalDistributionValueItems.All(m => m.ColumnId != k))
                    .Select(columnName => CreateTable(data, targetColumn, columnName))
                    .ToList();

            ResultTables.AddRange(StaticStorage.NormalDistributionValueItems.Select(m=>CreateTable(m, data)));
        }

        private DataTable CreateTable(NormalDistributionValueItem item, DataTable data)
        {
            string predictorColumn = data.Columns[item.ColumnId].ColumnName;
            string[] predictorValues = data.AsEnumerable().Select(m => m[predictorColumn].ToString().Format()).Where(m => m != "").ToArray();
            string[] predictorDistinctValues = predictorValues.Distinct().ToArray();

            DataTable result = new DataTable($"{data.Columns[item.ColumnId].ColumnName} - {TargetColumn}");
            result.Columns.Add(predictorColumn.Format());
            result.Columns.Add("probability (normal distribution)");

            foreach (string predictorDistinctValue in predictorDistinctValues)
            {
                DataRow row = result.NewRow();
                row[predictorColumn.Format()] = predictorDistinctValue;

               /* double exponent =
                    Math.Exp(-(Math.Pow(double.Parse(predictorDistinctValue) - item.Mean, 2)/(2*Math.Pow(item.Std, 2))));
                double probability = (1/(Math.Sqrt(2*Math.PI)*item.Std))*exponent;*/

                double zscore = (double.Parse(predictorDistinctValue) - item.Mean)/item.Std;
                double exponent = -(zscore*zscore)/2;
                double probability = (1/Math.Sqrt(2*Math.PI))*Math.Exp(exponent);

                row[1] = Math.Round(probability,6);
                result.Rows.Add(row);
            }

            return result;
        }

        private DataTable CreateTable(DataTable data, string targetColumn, string predictorColumn)
        {
            DataTable result = new DataTable($"{targetColumn} - {predictorColumn}");

            string[] targetValues = data.AsEnumerable().Select(m => m[targetColumn].ToString().Format()).Where(m => m != "").ToArray();
            string[] predictorValues = data.AsEnumerable().Select(m => m[predictorColumn].ToString().Format()).Where(m => m != "").ToArray();

            string[] targetDistinctValues = targetValues.Distinct().ToArray();
            string[] predictorDistinctValues = predictorValues.Distinct().ToArray();

            result.Columns.Add(predictorColumn.Format());
            result.AddColumns(targetDistinctValues);
            result.Columns.Add("probability");

            foreach (string predictorDistinctValue in predictorDistinctValues)
            {
                if (targetColumn == predictorColumn)
                    continue;

                DataRow row = result.NewRow();
                foreach (string targetDistinctValue in targetDistinctValues)
                {
                    string query =
                    $"{targetColumn} = '{targetDistinctValue}' AND {predictorColumn} = '{predictorDistinctValue}'";

                    string totalQuery = $"{TargetColumn}='{targetDistinctValue}'";
                    row[targetDistinctValue] = data.Select(query).Length + "/" + data.Select(totalQuery).Length;
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
            Console.WriteLine("OneRAlgorithm Result with targetColumn: {0}", TargetColumn);
            Console.WriteLine("-----------------------------------------------------------");
            Console.WriteLine();

            int dataSetCount = DataSet.Rows.Count;
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
                            if (row.ItemArray.Length > 2)
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

        public string TargetColumn { get; set; }

        public List<DataTable> ResultTables { get; private set; }

        public DataTable DataSet { get; set; }
    }
}
