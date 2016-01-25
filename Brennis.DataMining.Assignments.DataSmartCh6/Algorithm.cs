using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Brennis.DataMining.Assignments.DataSmartCh6.Model;

namespace Brennis.DataMining.Assignments.DataSmartCh6
{
    public class GeneticAlgorithm
    {
        private readonly double _crossoverRate;
        private readonly double _mutationRate;
        private readonly bool _elitism;
        private readonly int _populationSize;
        private readonly int _numIterations;
        private readonly List<Customer> _dataSet;
        private readonly PropertyInfo[] _properties;
        private readonly Random _r = new Random();

        /// <summary>
        /// Set alle parameters die nodig zijn voor het algoritme
        /// </summary>
        /// <param name="crossoverRate"></param>
        /// <param name="mutationRate"></param>
        /// <param name="elitism"></param>
        /// <param name="populationSize"></param>
        /// <param name="numIterations"></param>
        /// <param name="dataSet"></param>
        public GeneticAlgorithm(double crossoverRate, double mutationRate, bool elitism, int populationSize, int numIterations, List<Customer> dataSet)
        {
            _crossoverRate = crossoverRate;
            _mutationRate = mutationRate;
            _elitism = elitism;
            _populationSize = populationSize;
            _numIterations = numIterations;
            _dataSet = dataSet;
            _properties = new Customer().GetType().GetProperties();
        }

        /// <summary>
        /// Voert het algoritme uit met de meegegven functies als parameter
        /// </summary>
        /// <returns></returns>
        public Tuple<Customer, double> Run()
        {
            // Initialiseer de populatie op basis van de populationsize
            var initialPopulation = Enumerable.Range(0, _populationSize).Select(i => CreateIndividual()).ToArray();

            var currentPopulation = initialPopulation;
            
            for (int generation = 0; generation < _numIterations; generation++)
            {
                // Berekenen van de fitnesses van de populatie
                var fitnesses = Enumerable.Range(0, _populationSize).Select(i => ComputeFitness(currentPopulation[i])).ToArray();

                var nextPopulation = new Customer[_populationSize]; 

                // Pas elitsm toe wanneer true
                int startIndex; 
                if(_elitism)
                {
                    startIndex = 1;
                    var populationWithFitness =
                        currentPopulation.Select(
                            (individual, index) => new Tuple<Customer, double>(individual, fitnesses[index]));
                    var populationSorted = populationWithFitness.OrderBy(tuple => tuple.Item2); // item2 is the fitness
                    var bestIndividual = populationSorted.First();
                    nextPopulation[0] = bestIndividual.Item1;
                } else
                {
                    startIndex = 0;
                }

                // Creeër nieuwe individuelen voor de volgende generatie
                for (int newInd = startIndex; newInd < _populationSize; newInd++)
                {
                    // Selecteer de twee parents
                    var parents = SelectTwoParents(currentPopulation, fitnesses);
                    
                    // Voer een crossover uit de gekozen parents om de twee children te genereren op basis van de kans van de crossoverrate
                    var offspring = _r.NextDouble() < _crossoverRate ? Crossover(parents) : parents;

                    // Sla de children op in de volgende populatie na eventuele mutatie.
                    nextPopulation[newInd++] = Mutation(offspring.Item1, _mutationRate);
                    if (newInd < _populationSize) // Wanneer er nog wat ruimte is in de populatie
                        nextPopulation[newInd] = Mutation(offspring.Item2, _mutationRate);
                }

                // De huidige populatie wordt de nieuwe populatie
                currentPopulation = nextPopulation;
            }

            // Herbereken de fitnesses van de nieuwe populatie en selecteer vervolgens de beste.
            var finalFitnesses = Enumerable.Range(0, _populationSize).Select(i => ComputeFitness(currentPopulation[i])).ToArray();
            return
                currentPopulation.Select(
                    (individual, index) => new Tuple<Customer, double>(individual, finalFitnesses[index]))
                    .OrderBy(tuple => tuple.Item2)
                    .First();
        }

        /// <summary>
        /// Maak een nieuw individu met coefficienten tussen -1 en 1.
        /// </summary>
        /// <returns></returns>
        private Customer CreateIndividual()
        {
            Customer result = new Customer();
            foreach (
                PropertyInfo property in
                    _properties.Where(
                        pi => !pi.Name.Equals("SumProduct") && !pi.Name.Equals("Pregnant") && !pi.Name.Equals("SSE")))
            {
                property.SetValue(result, _r.NextDouble()*(1 - (-1)) + (-1));
            }

            return result;
        }

        /// <summary>
        /// Bereken de Squared Error van een klant in de trainingset
        /// </summary>
        /// <param name="customer"></param>
        private static void CalculateSquaredError(Customer customer)
        {
            double squaredError = customer.Pregnant - customer.SumProduct;
            customer.SSE = Math.Pow(squaredError, 2);
        }

        /// <summary>
        /// Bereken de SumProduct van een klant met de coefficienten
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="coefficient"></param>
        private static void CalculateSumProduct(Customer customer, Customer coefficient)
        {
            customer.SumProduct = (from PropertyDescriptor descriptor in TypeDescriptor.GetProperties(customer)
                    where
                        !descriptor.Name.Equals("SumProduct") && !descriptor.Name.Equals("Pregnant") &&
                        !descriptor.Name.Equals("SSE")
                    let value1 = double.Parse(descriptor.GetValue(customer).ToString())
                    let value2 = double.Parse(descriptor.GetValue(coefficient).ToString())
                    select value1 * value2).Sum();
        }

        /// <summary>
        /// Bij elke iteratie worden alle SumProducts en SSE's gecorrigeerd naar 0.0
        /// </summary>
        private void ReturnDataSetToDefault()
        {
            foreach (Customer customer in _dataSet)
            {
                customer.SumProduct = 0.0;
                customer.SSE = 0.0;
            }
        }

        /// <summary>
        /// Geeft het totaal aantal kolommen terug.
        /// </summary>
        /// <returns></returns>
        private int IndividualSize()
        {
            return _properties.Count(pi => !pi.Name.Equals("SumProduct") && !pi.Name.Equals("Pregnant") && !pi.Name.Equals("SSE"));
        }

        /// <summary>
        /// Bereken de fitness van een individu met de SumProduct en SquaredError
        /// </summary>
        /// <param name="individual"></param>
        /// <returns></returns>
        private double ComputeFitness(Customer individual)
        {
            ReturnDataSetToDefault();

            foreach (Customer customer in _dataSet)
            {
                CalculateSumProduct(customer, individual);
                CalculateSquaredError(customer);
            }

            return _dataSet.Sum(m => m.SSE);
        }

        /// <summary>
        /// Op basis van de ranking methode twee parents selecteren.
        /// </summary>
        /// <param name="population"></param>
        /// <param name="fitness"></param>
        /// <returns></returns>
        private Tuple<Customer, Customer> SelectTwoParents(Customer[] population, double[] fitness)
        {
            var ranking = population.Select((x, i) => new Tuple<Customer, double>(x, fitness[i]))
                .ToList()
                .OrderByDescending(m => m.Item2)
                .Select((x, i) => new Tuple<Customer, double, int>(x.Item1, x.Item2, i + 1)).ToList();

            Customer parentOne = new Customer();
            Customer parentTwo = new Customer();

            // Ranking methode op basis van probability. Hoogste fitness heeft de hoogste probability om gekozen te worden.
            for (int i = 0; i < 2; i++)
            {
                double probabilityUpperBoundary = 0;
                double probabilityLowerBoundary = 0;

                int rndSelect = _r.Next(0, ranking.Sum(m=>m.Item3));
                foreach (Tuple<Customer, double, int> rank in ranking)
                {
                    probabilityUpperBoundary += rank.Item3;
                    if (rndSelect >= probabilityLowerBoundary && rndSelect <= probabilityUpperBoundary)
                    {
                        if (i == 0)
                            parentOne = rank.Item1;
                        else
                            parentTwo = rank.Item1;

                        break;
                    }
                    probabilityLowerBoundary = probabilityUpperBoundary; 
                }
            }
            
            return new Tuple<Customer, Customer>(parentOne, parentTwo);
        }

        /// <summary>
        /// Crossover tussen de geselcteerde parents. Two point crossover methode wordt gebruikt.
        /// </summary>
        /// <param name="parents"></param>
        /// <returns></returns>
        private Tuple<Customer, Customer> Crossover(Tuple<Customer, Customer> parents)
        {
            int indSize = IndividualSize();
            int crossOverPointOne = _r.Next(1, indSize-1);
            int crossOverPointTwo = _r.Next(crossOverPointOne + 1, indSize);

            PropertyInfo[] prop = _properties.Where(
                pi => !pi.Name.Equals("SumProduct") && !pi.Name.Equals("Pregnant") && !pi.Name.Equals("SSE"))
                .Where((x, i) => i > crossOverPointOne && i < crossOverPointTwo)
                .ToArray();

            Customer indOne = Customer.Copy(parents.Item1);
            Customer indTwo = Customer.Copy(parents.Item2);
            
            foreach (var property in prop)
            {
                property.SetValue(indOne, property.GetValue(parents.Item2));
                property.SetValue(indTwo, property.GetValue(parents.Item1));
            }

            return new Tuple<Customer, Customer>(indOne, indTwo);
        }

        /// <summary>
        /// Mutatie methode voor de gecreërde children op basis van de mutation rate. 
        /// </summary>
        /// <param name="individual"></param>
        /// <param name="mutRate"></param>
        /// <returns></returns>
        private Customer Mutation(Customer individual, double mutRate)
        {
            double rnd = _r.NextDouble();

            if (!(rnd < mutRate)) return individual;

            int randomMut = _r.Next(1, (IndividualSize()));

            PropertyInfo[] prop = _properties
                .Where(pi => !pi.Name.Equals("SumProduct") && !pi.Name.Equals("Pregnant") && !pi.Name.Equals("SSE"))
                .ToArray();

            //Set een random waarde voor de gemuteerde coefficient
            for (int i = 0; i < randomMut; i++)
            {
                int index = _r.Next(1, (IndividualSize()));
                prop[index].SetValue(individual, _r.NextDouble() * (1 - (-1)) + (-1));
            }

            return individual;
        }
    }
}
