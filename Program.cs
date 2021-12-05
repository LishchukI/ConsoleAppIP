using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Configuration;

namespace ConsoleAppIP
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = ConfigurationManager.AppSettings["FileName"];
            DateTime lastWriteTime = new DateTime();
            ConcurrentBag<string> listStringsOfFile = new ConcurrentBag<string>();
            string inputIP = null;
            bool isStringMatchFound;

            IAlgorithmComparing versionOfAlgorithm = new FirstAlgorithmComparing();
            //IVersionOfAlgorithm versionOfAlgorithm = new SecondVersionOfAlgorithm();

            ILogging logging = new LoggingInTextFile();

            FirstProgramLaunch(ref inputIP, args[0], ref lastWriteTime, ref listStringsOfFile, fileName, logging);
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

                        logging.SetLog("Info: Reading file...", true);
                        listStringsOfFile = GetFileListParallel(fileName);
                    }

                    logging.SetLog("Info: Comparing strings...", true);
                    isStringMatchFound = versionOfAlgorithm.ComparingStringsParallel(listStringsOfFile, inputIP);

                    logging.SetLog("Info: Loading results...", true);
                    logging.SetLog(OutputResult(isStringMatchFound), true);
                    isStringMatchFound = false;

                    Console.WriteLine("Enter the next IP, or 'quit' to exit:");
                    logging.SetLog("Info: Inputing data...", false);
                    inputIP = Console.ReadLine();
                }
                catch (IndexOutOfRangeException ex)
                {
                    ExceptionExit(ex.Message, logging);
                }
                catch (IOException ex)
                {
                    ExceptionExit(ex.Message, logging);
                }
                catch (EmptyFileException ex)
                {
                    ExceptionExit(ex.Message, logging);
                }
                catch (IcorrectIPAdressException ex)
                {
                    ExceptionExit(ex.Message + " Input data -- " + inputIP, logging);
                }
            } while (inputIP != "quit");
            logging.SetLog("Info: Closing program.", false);
        }

        private static void FirstProgramLaunch(ref string inputIP, string arg, ref DateTime lastWriteTime, ref ConcurrentBag<string> listStringsOfFile, string fileName, ILogging logging)
        {
            try
            {
                inputIP = arg;

                lastWriteTime = File.GetLastWriteTime(fileName);

                logging.SetLog("Info: Reading file...", true);
                listStringsOfFile = GetFileListParallel(fileName);
            }
            catch (IndexOutOfRangeException)
            {
                ExceptionExit("Error: Incorrect input data of '.bat' file.", logging);
            }
            catch (IOException ex)
            {
                ExceptionExit(ex.Message, logging);
            }
        }

        private static ConcurrentBag<string> GetFileListParallel(string fileName)
        {
            ConcurrentBag<string> listStringsOfFile = new ConcurrentBag<string>();

            Parallel.ForEach(File.ReadLines(fileName),
            () =>
            {
                return new ConcurrentBag<string>();
            },
            (line, loopState, index, list) =>
            {
                list.Add(line);
                return list;
            },
            list =>
            {
                foreach (string stringOfList in list)
                {
                    listStringsOfFile.Add(stringOfList);
                }

            });
            return listStringsOfFile;
        }

        private static string OutputResult(bool isStringMatchFound)
        {
            if (isStringMatchFound)
            {
                return "Info: Access disallowed.\n";
            }
            else
            {
                return "Info: Access allowed.\n";
            }
        }

        private static void ExceptionExit(string message, ILogging logging)
        {
            logging.SetLog("Error: " + message, true);
            logging.SetLog("Info: Closing program.", false);
            Console.ReadKey(true);
            Environment.Exit(0);
        }
    }
}
