using System.Data;
using System.IO;
using System.Reflection;
using Brennis.DataMining.Assignments.Common.Enum;
using Brennis.DataMining.Assignments.Common.Extensions;

namespace Brennis.DataMining.Assignments.DataAccess.CsvReader
{
    public class CsvReader : ICsvReader
    {
        public DataTable ReadToDataTable(string fileName, string tableName)
        {
            DataTable result = new DataTable(tableName);
            fileName = "Resources\\" + fileName;

            string executableLocation = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
            string fileLocation = null;
            if (executableLocation != null)
                fileLocation = Path.Combine(executableLocation, fileName);
            
            //TODO TypeOfNum variable
            return (fileLocation != null)
                ? File.ReadAllLines(fileLocation).ToDataTable(tableName, TypeOfNumericProbabilityEnum.NormalDistribution)
                : result;
        }
    }
}
