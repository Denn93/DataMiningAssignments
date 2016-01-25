using System;
using System.IO;
using Brennis.DataMining.Assignments.DataSmartCh6.Model;
using Brennis.DataMining.Assignments.DataSmartCh6.Repository;

namespace Brennis.DataMining.Assignments.DataSmartCh6
{
    class Program
    {
        static void Main(string[] args)
        {
            // Hier wordt de repository aangemaakt met de trainingset.
            PregnancyRepository repo =
                new PregnancyRepository(Directory.GetCurrentDirectory() + Configuration.File);

            //Hier wordt het genetic algorithm aangeroepen waaruit de beste coefficienten komen.
            Customer coefficients = repo.ExcecuteAlgorithm();

            // Hier wordt de prediction uitgevoerd op de testset met de coefficients die hierboven zijn gevonden in de trainingset.
            PredictionAlgorithm predictionAlgorithm = new PredictionAlgorithm(coefficients);
            predictionAlgorithm.Execute();

            Console.ReadLine();
        }
    }
}
