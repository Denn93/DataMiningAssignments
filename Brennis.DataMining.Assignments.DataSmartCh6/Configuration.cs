using System.Configuration;

namespace Brennis.DataMining.Assignments.DataSmartCh6
{
    public static class Configuration
    {
        public static string File => ConfigurationManager.AppSettings["file"];

        public static string TestSetFile => ConfigurationManager.AppSettings["testfile"];
    }
}