using System.Data;

namespace Brennis.DataMining.Assignments.DataAccess.OneRAlgorithm
{
    interface IOneRAlgorithm
    {
        void Process(DataTable data, string targetColumn);
        void Print();
    }
}
