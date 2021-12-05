using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ConsoleAppIP
{
    class Program
    {
        private class EmptyFileException : Exception
        {
            public EmptyFileException() : base("File is empty!") { }
        }
        private class IcorrectIPAdressException : Exception
        {
            public IcorrectIPAdressException() : base("Incorrect input IP adress!") { }
        }

        //private static string inputIP;
        //private static bool isStringMatchFound = false;
        static void Main(string[] args)
        {
            string fileName = "IPs List.txt";
            string inputIP = null;
            bool isStringMatchFound;
            DateTime lastWriteTime = new DateTime();
            List<string> listStringsOfFile = new List<string>();

            try
            {
                inputIP = args[0];
                lastWriteTime = File.GetLastWriteTime(fileName);
                listStringsOfFile = GetFileListParallel(fileName);
            }
            catch (IndexOutOfRangeException)
            {
                ExceptionExit("Error: Incorrect input data of '.bat' file.");
            }
            catch (IOException ex)
            {
                ExceptionExit(ex.Message);
            }

            /*
            //*   Первый способ с созданием потоков, в зависимости колличества ядер
            //*   Закомментирован ещё дополнительный метод и переменные для этого способа

            int numberThreads = Environment.ProcessorCount;
            Thread[] threads = new Thread[numberThreads];


            int numberTakeElements;
            int numberSkipElements = 0;
            int addRemainder = 1;

            int listLength = listStringsOfFile.Count;
            int resultOfDivision = listLength / numberThreads;
            int resultOfRemainder = listLength % numberThreads;

            for (int i = 0; i < numberThreads; i++)
            {
                if (--resultOfRemainder < 0)
                {
                    addRemainder = 0;
                }
                numberTakeElements = resultOfDivision + addRemainder;

                threads[i] = new Thread(new ParameterizedThreadStart(ThreadProc));
                threads[i].Start(listStringsOfFile.Skip(numberSkipElements).Take(numberTakeElements).ToList());

                numberSkipElements += numberTakeElements;
            }

            foreach(Thread thread in threads)
            {
                thread.Join();
            }

            if (isStringMatchFound)
            {
                Logging("Warn: Access disallowed.\n", true);
                isStringMatchFound = false;
            }
            else
            {
                Logging("Info: Access allowed.\n", true);
            }
            Console.ReadKey();
            */

            do
            {
                try
                {
                    if (listStringsOfFile.Count == 0)
                    {
                        throw new EmptyFileException();
                    }
                    if (inputIP.Count(c => c == '.') != 3 || !IPAddress.TryParse(inputIP, out IPAddress address))
                    {
                        throw new IcorrectIPAdressException();
                    }

                    if (lastWriteTime != File.GetLastWriteTime(fileName))
                    {
                        lastWriteTime = File.GetLastWriteTime(fileName);

                        Logging("Info: Reading file...", true);
                        listStringsOfFile = GetFileListParallel(fileName);
                    }

                    //Второй способ
                    Logging("Info: Comparing strings...", true);
                    isStringMatchFound = ComparingStringsParallel(listStringsOfFile, inputIP);

                    Logging("Info: Loading results...", true);
                    OutputResult(isStringMatchFound);
                    isStringMatchFound = false;

                    Console.WriteLine("Enter the next IP, or 'quit' to exit");
                    Logging("Info: Inputing data...", false);
                    inputIP = Console.ReadLine();
                }
                catch (IndexOutOfRangeException ex)
                {
                    ExceptionExit(ex.Message);
                }
                catch (IOException ex)
                {
                    ExceptionExit(ex.Message);
                }
                catch (EmptyFileException ex)
                {
                    ExceptionExit(ex.Message);
                }
                catch (IcorrectIPAdressException ex)
                {
                    ExceptionExit(ex.Message + " Input data -- " + inputIP);
                }
            } while (inputIP != "quit");

            Logging("Info: Closing program.", false);
        }

        private static List<string> GetFileListParallel(string fileName)
        {
            List<string> listStringsOfFile = new List<string>();

            Parallel.ForEach(File.ReadLines(fileName),
            () =>
            {
                return new List<string>();
            },
            (line, loopState, index, list) =>
            {
                list.Add(line);
                return list;
            },
            list =>
            {
                lock (listStringsOfFile)
                {
                    listStringsOfFile.AddRange(list);
                }
            });
            return listStringsOfFile;
        }

        private static bool ComparingStringsParallel(List<string> listStringsOfFile, string inputIP)
        {
            bool isStringMatchFound = false;
            Parallel.ForEach(listStringsOfFile, (line, state) =>
            {
                if (Equals(line, inputIP))
                {
                    isStringMatchFound = true;
                    state.Stop();
                }
            });

            return isStringMatchFound;
        }

        private static void OutputResult(bool isStringMatchFound)
        {
            if (isStringMatchFound)
            {
                Logging("Info: Access disallowed.\n", true);
            }
            else
            {
                Logging("Info: Access allowed.\n", true);
            }
        }

        private static void Logging(string message, bool isOutputingInConsole)
        {
            File.AppendAllText("log.txt", DateTime.Now + "|" + message + "\n");
            if (isOutputingInConsole)
            {
                Console.WriteLine(message);
            }
        }

        private static void ExceptionExit(string message)
        {
            Logging("Error: " + message, true);
            Logging("Info: Closing program.", false);
            Console.ReadKey(true);
            Environment.Exit(0);
        }

        /*
        private static void ThreadProc(object listStringsOfFile)
        {
            foreach (string line in (List<string>)listStringsOfFile)
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " " + line);
                if (Equals(inputIP, line))
                {
                    isStringMatchFound = true;
                }
                if(isStringMatchFound)
                {
                    break;
                }
            }
        }
        */
    }
}
