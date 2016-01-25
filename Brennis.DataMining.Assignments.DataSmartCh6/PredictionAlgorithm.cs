using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Brennis.DataMining.Assignments.DataSmartCh6.DataAccess;
using Brennis.DataMining.Assignments.DataSmartCh6.Model;

namespace Brennis.DataMining.Assignments.DataSmartCh6
{
    public class PredictionAlgorithm
    {
        private readonly Customer _coefficients;
        private List<Customer> _testSetCustomers;

        /// <summary>
        /// Set de coefficiënten die gekozen zijn in het genetic algorithm en de testset data.
        /// </summary>
        /// <param name="coefficients"></param>
        public PredictionAlgorithm(Customer coefficients)
        {
            _coefficients = coefficients;
            _testSetCustomers = new CsvReader().Get(Directory.GetCurrentDirectory() + Configuration.TestSetFile);
        }

        /// <summary>
        /// Bereken de SumProduct van een klant met de coefficienten
        /// </summary>
        /// <param name="testCustomer"></param>
        /// <param name="coefficients"></param>
        /// <returns></returns>
        private double CalculateSumProduct(Customer testCustomer, Customer coefficients)
        {
            return (from PropertyDescriptor descriptor in TypeDescriptor.GetProperties(testCustomer)
                where
                    !descriptor.Name.Equals("SumProduct") && !descriptor.Name.Equals("Pregnant") &&
                    !descriptor.Name.Equals("SSE") && !descriptor.Name.Equals("Intercept")
                let value1 = double.Parse(descriptor.GetValue(testCustomer).ToString())
                let value2 = double.Parse(descriptor.GetValue(coefficients).ToString())
                select value1*value2).Sum();
        }

        /// <summary>
        /// Voer de prediction agoritme uit op basis van de model coefficienten en de testset data.
        /// </summary>
        public void Execute()
        {
            Dictionary<Customer, double> values = new Dictionary<Customer, double>();
            foreach (Customer customer in _testSetCustomers)
            {
                double sumProduct = CalculateSumProduct(customer, _coefficients);
                values.Add(customer, sumProduct);
            }

            /* 
            Wij hebben gekozen om de berkekende waardes als cutoff te gebruiken 
            (DataSmart Chapter 8, p. 233) ipv increments van 0.05 tussen
            de minimum en maximum van de predictions, omdat wij geen 'mooie' waarde
            als min en max krijgen, waardoor we met incrementele cutoffs
            aan het einde een deel zoude missen. */

            double value;
            double threshold;
            double precision;

            CalculatePerformanceMetrics(values, out value, out threshold, out precision);

            Console.WriteLine($"Dit model kan {value}% van de zwangerschappen voorspellen bij de cutoff van " +
                              $"{threshold} met 0 false positives en een precision van {precision}.");

            //WriteToCsv(values);

            Console.ReadKey();
        }

        /// <summary>
        /// Calculate probability cutoff, True Negative Rate, False Positive Rate en True Positive Rate
        /// om de treshold en de bijbehorende True Positive Rate en precision te bepalen.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="value"></param>
        /// <param name="threshold"></param>
        /// <param name="precision"></param>
        private static void CalculatePerformanceMetrics(Dictionary<Customer, double> values, out double value,
            out double threshold, out double precision)
        {
            List<double> probabilityCutoff = values.Select(m => m.Value).Distinct().OrderBy(m => m).ToList();

            List<double> precisions =
                probabilityCutoff.Select(
                    cutoff =>
                        values.Where(m => (int) m.Key.Pregnant == 1).Count(m => m.Value >= cutoff)/
                        (double) values.Count(m => m.Value >= cutoff)).ToList();

            List<double> trueNegativeRate =
                probabilityCutoff.Select(
                    cutoff =>
                        values.Where(m => (int) m.Key.Pregnant == 0).Count(m => m.Value < cutoff)/
                        (double) values.Count(m => (int) m.Key.Pregnant == 0)).ToList();

            List<double> falsePositiveRate = trueNegativeRate.Select(m => 1 - m).ToList();

            List<double> truePositiveRate =
                probabilityCutoff.Select(
                    cutoff =>
                        values.Where(m => (int) m.Key.Pregnant == 1).Count(m => m.Value >= cutoff)/
                        (double) values.Count(m => (int) m.Key.Pregnant == 1)).ToList();

            int index =
                trueNegativeRate.Select((v, k) => new {Key = k, Value = v})
                    .Where(m => (int) m.Value == 1)
                    .Select(m => m.Key)
                    .First();

            value = truePositiveRate[index]*100;
            threshold = probabilityCutoff.Where((k, v) => trueNegativeRate[v] == 1 && falsePositiveRate[v] == 0).First();
            precision = precisions.Where((k, v) => trueNegativeRate[v] == 1 && falsePositiveRate[v] == 0).First();
        }

        /// <summary>
        /// Deze methode wordt alleen gebruikt voor het wegschrijven van de predictions. Deze wordt op dit moment niet gebruikt
        /// ,omdat dit alleen het inleveren van de resultaten is gebruikt.
        /// </summary>
        /// <param name="values"></param>
        public void WriteToCsv(Dictionary<Customer, double> values) 
        {
            StringBuilder bob = new StringBuilder();

            var prop = TypeDescriptor.GetProperties(values.First().Key);
            List<string> headers = (from PropertyDescriptor descriptor in prop select descriptor.Name).ToList();
            headers.Add("Linear Prediction");
            bob.AppendLine(string.Join(";", headers.Where(m => !m.Equals("SumProduct") && !m.Equals("SSE") && !m.Equals("Intercept"))));

            
            foreach (KeyValuePair<Customer, double> testCustomer in values)
            {
                List<string> itemArray =
                    (from PropertyDescriptor descriptor in TypeDescriptor.GetProperties(testCustomer.Key)
                        where
                            !descriptor.Name.Equals("SumProduct") && !descriptor.Name.Equals("SSE") &&
                            !descriptor.Name.Equals("Intercept")
                        select descriptor.GetValue(testCustomer.Key).ToString()).ToList();

                itemArray.Add(testCustomer.Value.ToString());
                bob.AppendLine(string.Join(";", itemArray));
            }
           
            File.WriteAllText(
                @"C:\Users\denni\Dropbox\HRO Dropbox\Jaar 4\MIINBOD02\CMIBOD023T\BRENNIS\LinearPredictionDataSet.csv",
                bob.ToString());
        }
    }
}
