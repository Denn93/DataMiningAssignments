namespace Brennis.DataMining.Assignments.Common.Extensions
{
    public static class StringExtension
    {
        public static string Format(this string value)
        {
            return value.Trim().ToLower();
        }
    }
}
