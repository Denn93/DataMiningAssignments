using System.Data;

namespace Brennis.DataMining.Assignments.DataAccess.CsvReader
{
    public interface ICsvReader
    {
        DataTable ReadToDataTable(string fileName, string tableName);
    }
}
