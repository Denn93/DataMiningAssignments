using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Brennis.DataMining.Assignments.Common.Enum;
using Brennis.DataMining.Assignments.Common.Utils;

namespace Brennis.DataMining.Assignments.Common.Extensions
{
    public static class StringArrayExtensions
    {
        public static DataTable ToDataTable(this string[] lines, string tableName, TypeOfNumericProbabilityEnum typeOfNumericProbability = TypeOfNumericProbabilityEnum.Binning)
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

            PerformBinning(typeOfNumericProbability, result, numericColumns);
            
            return result;
        }

        private static void PerformBinning(TypeOfNumericProbabilityEnum probabilityEnum, DataTable result, List<int> numericColumns)
        {
            switch (probabilityEnum)
            {
                    case TypeOfNumericProbabilityEnum.Binning:

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

                    break;

                    case TypeOfNumericProbabilityEnum.NormalDistribution:

                    string[] targetValues = result.AsEnumerable().Select(m => m[StaticStorage.TargetColum].ToString().Format()).ToArray();
                    string[] targetDistinctValues = targetValues.Distinct().ToArray();

                    List<NormalDistributionValueItem> valueItems = new List<NormalDistributionValueItem>();

                    foreach (int numericColumn in numericColumns)
                    {
                        foreach (string targetDistinctValue in targetDistinctValues)
                        {
                            NormalDistributionValueItem normalDistributionValue = new NormalDistributionValueItem();

                            List<int> values =
                                (from DataRow dataRow in result.Rows
                                    where dataRow[StaticStorage.TargetColum].Equals(targetDistinctValue)
                                    select int.Parse(dataRow[numericColumn].ToString()))
                                    .ToList();

                            normalDistributionValue.ColumnId = numericColumn;
                            normalDistributionValue.TargetValue = targetDistinctValue;
                            normalDistributionValue.Mean = values.Average();
                            normalDistributionValue.Variance =
                                values.Sum(m => Math.Pow(m - normalDistributionValue.Mean, 2))/values.Count - 1;

                            normalDistributionValue.Std = Math.Sqrt(normalDistributionValue.Variance);

                            valueItems.Add(normalDistributionValue);
                        }
                    }

                    StaticStorage.NormalDistributionValueItems = valueItems;
                    break;
            }
        }
    }
}
