using System.Data;

namespace Brennis.DataMining.Assignments.DataAccess.ZeroRAlgorithm
{
    public interface IZeroRAlgorithm
    {
        void Process(DataTable dataSet, string targetColumn);
        void Print();

    }
}
