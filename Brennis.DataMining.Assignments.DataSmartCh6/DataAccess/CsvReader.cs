using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brennis.DataMining.Assignments.DataSmartCh6.Model;

namespace Brennis.DataMining.Assignments.DataSmartCh6.DataAccess
{
    internal class CsvReader
    {
        public List<Customer> Get(string file)
        {
            var lines = File.ReadAllLines(file);
            
            return
                lines.Where((x, i) => i > 0)
                    .Select(line => line.Split(';').Select(double.Parse).ToArray())
                    .Select(ToCustomer)
                    .ToList();
        }

        private static Customer ToCustomer(double[] values)
        {
            return new Customer
            {
                Male = values[0],
                Female = values[1],
                Home = values[2],
                Apt = values[3],
                PregnancyTest = values[4],
                BirthControl = values[5],
                FeminineHygiene = values[6],
                FolicAcid = values[7],
                PrenatalVitamins = values[8],
                PrenatalYoga = values[9],
                BodyPillow = values[10],
                GingerAle = values[11],
                SeaBands = values[12],
                StoppedBuyingCiggies = values[13],
                Cigarettes = values[14],
                SmokingCessation = values[15],
                StoppedBuyingWhine = values[16],
                Wine = values[17],
                MaternityClothes = values[18],
                Pregnant = values[19],
                Intercept = 1
            };
        }
    }
}
