using System;
using System.Text;

namespace MonoLibrary.Dependency.Loggers
{
    internal static class LoggerFormatter
    {
        public static string Format(string formated, Exception exception)
        {
            if (exception == null)
                return formated;
            else
            {
                var stb = new StringBuilder(formated)
                    .AppendLine()
                    .Append(exception.ToString());
                return stb.ToString();
            }
        }
    }
}
