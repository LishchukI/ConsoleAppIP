using System;

namespace ConsoleAppIP
{
    public class EmptyFileException : Exception
    {
        public EmptyFileException() : base("File is empty!") { }
    }
    public class IcorrectIPAdressException : Exception
    {
        public IcorrectIPAdressException() : base("Incorrect input IP adress!") { }
    }
}
