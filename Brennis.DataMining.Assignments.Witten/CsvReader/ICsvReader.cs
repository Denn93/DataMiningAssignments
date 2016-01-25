using System.Data;

namespace Brennis.DataMining.Assignments.Witten.CsvReader
{
    public interface ICsvReader
    {
        DataTable ReadToDataTable(string fileName, string tableName);
    }
}
