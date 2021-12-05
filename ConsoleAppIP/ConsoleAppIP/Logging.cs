using System;
using System.IO;

namespace ConsoleAppIP
{
    public interface ILogging
    {
        public void SetLog(string message, bool isOutputingInConsole);
    }

    public class LoggingInTextFile : ILogging
    {
        public void SetLog(string message, bool isOutputingInConsole)
        {
            File.AppendAllText("Log.txt", DateTime.Now + "|" + message + "\n");
            if (isOutputingInConsole)
            {
                Console.WriteLine(message);
            }
        }
    }
}
