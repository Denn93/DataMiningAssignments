using System;
using System.Data;
using Brennis.DataMining.Assignments.DataAccess.CsvReader;
using Brennis.DataMining.Assignments.DataAccess.OneRAlgorithm;
using Brennis.DataMining.Assignments.DataAccess.ZeroRAlgorithm;

namespace Brennis.DataMining.Assignments.ConsoleView
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("DataMining Assignments. Restaurant assignments");
            Console.WriteLine("----------------------------------------------");

            EnterTheMatrix();

            Console.ReadKey();
        }

        private static void EnterTheMatrix()
        {
            DataTable data = new CsvReader().ReadToDataTable("restaurantnumeric.csv", "Restaurant");
            ViewTheMatrixData(data);

            Console.WriteLine("Enter a command:");
            string input = Console.ReadLine();
            bool loop = true;
            while (loop)
            {
                switch ((input ?? string.Empty).ToLower().Trim())
                {
                    case "zeror":
                        ZeroRAlgorithm algorithm = new ZeroRAlgorithm();
                        algorithm.Process(data, "wait");
                        algorithm.Print();

                        loop = false;
                        break;
                    case "oner":
                        OneRAlgorithm oneAlgorithm = new OneRAlgorithm();
                        oneAlgorithm.Process(data, "wait");
                        oneAlgorithm.Print();

                        loop = false;
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

        private static void ViewTheMatrixData(DataTable table)
        {
            Console.WriteLine(table.TableName);
            foreach (DataRow row in table.Rows)
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
