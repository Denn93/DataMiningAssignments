using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Brennis.DataMining.Assignments.DataSmartCh6.DataAccess;
using Brennis.DataMining.Assignments.DataSmartCh6.Model;

namespace Brennis.DataMining.Assignments.DataSmartCh6.Repository
{
    /// <summary>
    /// De repository waarin het genetic algorithm wordt aangeroepen met de specifieke waarde. 
    /// En daarna wordt beste model geprint in de Console.
    /// </summary>
    internal class PregnancyRepository
    {
        private readonly List<Customer> _customers; 

        public PregnancyRepository(string file)
        {
            _customers = new CsvReader().Get(file);
        }

        /// <summary>
        /// Hier wordt de Total Sum of Squares berekend.
        /// </summary>
        /// <returns></returns>
        public double CalculcateTotalSumOfSquares()
        {
            return _customers.Sum(m => Math.Pow(m.Pregnant - _customers.Average(n => n.Pregnant), 2));
        }

        public Customer ExcecuteAlgorithm()
        {
            // Hier wordt het genetic algorithm aangeroepen met (op volgorde): crossoverrate, mutationrate, elitism, populationsize, 
            // iterationsize, trainingdata
            GeneticAlgorithm pregnancy = new GeneticAlgorithm(0.75, 0.05, true, 30, 300, _customers);
            var solution = pregnancy.Run();

            // Vervolgens wordt hier de beste model geprint inclusief de SSE, Total Sum of Squares, 
            // Explained Sum of Squares en R Squared
            Console.WriteLine("Solution: ");

            Customer result = solution.Item1;
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(result))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(result);
                Console.WriteLine("{0}={1}", name, value);
            }

            double totalSumOfSquares = CalculcateTotalSumOfSquares();
            double explainedSumOfSquares = totalSumOfSquares - solution.Item2;
            double rSquared = explainedSumOfSquares/totalSumOfSquares;

            Console.WriteLine("\nSSE: {0}: ", solution.Item2);
            Console.WriteLine("\nTotal Sum of Squares: {0}: ", totalSumOfSquares);
            Console.WriteLine("\nExplained Sum of Squares: {0}: ", explainedSumOfSquares);
            Console.WriteLine("\nR Squared: {0}: ", rSquared);

            Console.ReadLine();

            return result;
        }
    }
}