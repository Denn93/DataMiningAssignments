using Brennis.DataMining.Assignments.Common;
using Brennis.DataMining.Assignments.DataAccess.CsvReader;
using Brennis.DataMining.Assignments.DataAccess.ID3;
using Brennis.DataMining.Assignments.DataAccess.Likelihood;
using Brennis.DataMining.Assignments.DataAccess.OneRAlgorithm;
using Brennis.DataMining.Assignments.DataAccess.ZeroRAlgorithm;
using System;
using System.Data;

namespace Brennis.DataMining.Assignments.ConsoleView
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("DataMining Assignments. Restaurant assignments");
            Console.WriteLine("----------------------------------------------");

            EnterTheMatrix();

            Console.ReadKey();
        }

        private static void EnterTheMatrix()
        {
            Console.WriteLine("Enter the targetColumn:");
            string targetColumn = Console.ReadLine();

            StaticStorage.TargetColum = targetColumn;
            StaticStorage.DataSet = new CsvReader().ReadToDataTable("restaurant.csv", "Restaurant");

            ViewTheMatrixData();

            Console.WriteLine("Enter a command:");
            string input = Console.ReadLine();
            bool loop = true;
            while (loop)
            {
                switch ((input ?? string.Empty).ToLower().Trim())
                {
                    case "zeror":
                        ZeroRAlgorithm algorithm = new ZeroRAlgorithm();
                        algorithm.Process();
                        algorithm.Print();

                        loop = false;
                        break;

                    case "tree":
                        DecisionTree tree = new DecisionTree();
                        tree.Process();
                        loop = false;

                        break;

                    case "oner":
                        OneRAlgorithm oneAlgorithm = new OneRAlgorithm();
                        oneAlgorithm.Process();
                        oneAlgorithm.Print();

                        loop = false;

                        Console.WriteLine("Enter the probability rule: ");
                        Console.WriteLine("EXAMPLE --> weer:zonnig, temperatuur:koel, vochtigheid:hoog , wind:ja, spelen:ja");
                        string line = Console.ReadLine();

                        Console.WriteLine(new LikelihoodAlgorithm(oneAlgorithm.ResultTables, line).Process());
                        Console.WriteLine();
                        break;

                    case "exit":
                        loop = false;
                        Console.WriteLine("Press Enter to exit.");
                        break;

                    default:
                        Console.WriteLine("Command not found. Try again: ");
                        input = Console.ReadLine();
                        input = input ?? string.Empty;
                        break;
                }
            }
        }

        private static void ViewTheMatrixData()
        {
            Console.WriteLine(StaticStorage.DataSet.TableName);
            foreach (DataRow row in StaticStorage.DataSet.Rows)
            {
                foreach (object value in row.ItemArray)
                    Console.Write("{0,-3} {1,1}", value, "|");
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        private void ShowHelp()
        {
            Console.WriteLine("These are all the possible commands:");
            Console.WriteLine("oneR");
            Console.WriteLine("zeror");
        }
    }
}