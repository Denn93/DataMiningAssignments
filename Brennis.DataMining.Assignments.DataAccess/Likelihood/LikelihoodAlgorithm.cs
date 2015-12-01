using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Brennis.DataMining.Assignments.Common;
using Brennis.DataMining.Assignments.Common.Extensions;
using Expression = NCalc.Expression;

namespace Brennis.DataMining.Assignments.DataAccess.Likelihood
{
    public class LikelihoodAlgorithm : ILikelihoodAlgorithm
    {
        private readonly List<DataTable> _oneRResultSet;
        private readonly string _inputRule;
        private readonly List<KeyValuePair<string, string>> _likelihoods;  

        public LikelihoodAlgorithm(List<DataTable> oneRResultSet,
            string inputRule)
        {
            _oneRResultSet = oneRResultSet;
            _inputRule = inputRule;
            _likelihoods = new List<KeyValuePair<string, string>>();
        }

        public string Process()
        {
            string result = string.Empty;
            List<KeyValuePair<string, string>> input = ProcessInput(_inputRule);

            if (input == null)
                return "An error occurred during the process of your input. Check input values!!!";

            KeyValuePair<string, string> targetColumnPair = input.Find(i => i.Key.Format() == StaticStorage.TargetColum.Format());

            string[] targetValues = StaticStorage.DataSet.AsEnumerable().Select(m => m[StaticStorage.TargetColum].ToString().Format()).ToArray();
            string[] targetDistinctValues = targetValues.Distinct().ToArray();

            if (targetColumnPair.Equals(null))
                return "targetColumn is not specified!!.";

            foreach (string targetDistinct in targetDistinctValues)
                _likelihoods.Add(ProcessProbability(new KeyValuePair<string, string>(StaticStorage.TargetColum, targetDistinct),
                    input));


            string computation =
                $"{_likelihoods.Find(m => m.Key == targetColumnPair.Value).Value} / ( {_likelihoods.Select(m => m.Value).Aggregate((i, j) => i + " + " + j)} )".Replace(',', '.');

            Console.WriteLine();
            Console.WriteLine(
                $"kans({targetColumnPair.Value}) = {computation} = {Math.Round(double.Parse(new Expression(computation).Evaluate().ToString()) * 100, 2)}%");

            return result;
        }

        private KeyValuePair<string, string> ProcessProbability(KeyValuePair<string, string> targetColumnPair, List<KeyValuePair<string, string>> input)
        {
            string result = string.Empty;
            result = input.Aggregate(result,
                (j, valuePair) =>
                    _oneRResultSet.Aggregate(j,
                        (i, dt) => i + dt.GetProbabilityByTargetValue(valuePair, targetColumnPair)))
                .Format()
                .Replace(" ", " * ");

            result += " * " + StaticStorage.DataSet.Select($"{StaticStorage.TargetColum} = '{targetColumnPair.Value}'").Length + "/" +
                      StaticStorage.DataSet.Rows.Count;


            if (result.Contains("0/"))
            {
                string[] values = result.Split('*');
                result = string.Empty;

                int total = values.Length - 1;
                for (int i = 0; i < values.Length - 1; i++)
                {
                    string numerator = values[i].Split('/')[0];
                    string denominator = values[i].Split('/')[1];

                    double numeratorAdd = double.Parse("1")/double.Parse("3");

                    numerator = (double.Parse(numerator) + numeratorAdd).ToString();
                    denominator = (int.Parse(denominator) + 1).ToString();

                    result += (result.Equals(string.Empty))
                        ? $"{numerator}/{denominator}"
                        : $"* {numerator}/{denominator}";
                }

                result += $"* {values[values.Length - 1]}";
            }

            result = result.Replace(',', '.');
            Console.WriteLine($"likelihood({targetColumnPair.Value}) = {result} = {new Expression(result).Evaluate()}");

            return new KeyValuePair<string, string>(targetColumnPair.Value,
                new Expression(result).Evaluate().ToString());
        }

        private List<KeyValuePair<string, string>> ProcessInput(string inputRule)
        {
            try
            {
                return
                    inputRule.Split(',')
                        .Select(m => new KeyValuePair<string, string>(m.Split(':')[0].Format(), m.Split(':')[1].Format()))
                        .ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        } 
    }
}
