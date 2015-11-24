using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
                columnNames.Select(columnName => CreateTable(data, targetColumn, columnName)).ToList();

            
        }

        private static DataTable CreateTable(DataTable data, string targetColumn, string predictorColumn)
        {
            DataTable result = new DataTable($"{targetColumn} - {predictorColumn}");

            string[] targetValues = data.AsEnumerable().Select(m => m[targetColumn].ToString()).ToArray();
            string[] predictorValues = data.AsEnumerable().Select(m => m[predictorColumn].ToString()).ToArray();

            string[] targetDistinctValues = targetValues.Distinct().ToArray();
            string[] predictorDistinctValues = predictorValues.Distinct().ToArray();

            result.Columns.Add(predictorColumn);
            result.AddColumns(targetDistinctValues);
            result.Columns.Add("Probability");

            foreach (string predictorDistinctValue in predictorDistinctValues)
            {
                if (targetColumn == predictorColumn)
                    continue;

                DataRow row = result.NewRow();
                foreach (string targetDistinctValue in targetDistinctValues)
                {
                        string query =
                        $"{targetColumn} = '{targetDistinctValue}' AND {predictorColumn} = '{predictorDistinctValue}'";

                    row[targetDistinctValue] = data.Select(query).Length;
                }

                row["Probability"] = targetDistinctValues.Sum(m => int.Parse(row[m].ToString()));

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
                            string query = $"{TargetColumn}='{table.Columns[i]}'";
                            Console.Write("{0,-10} {1,2}", row.ItemArray[i] + "/" + DataSet.Select(query).Length, "|");
                        }
                        else
                            Console.Write("{0,-10} {1,2}",
                                row.ItemArray[i] + "/" + dataSetCount + "= " +
                                Math.Round(int.Parse(row.ItemArray[i].ToString())/(double) dataSetCount, 2), "|");
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
