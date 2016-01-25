namespace Brennis.DataMining.Assignments.CommonWitten.Extensions
{
    public static class StringExtension
    {
        public static string Format(this string value)
        {
            return value.Trim().ToLower();
        }
    }
}
