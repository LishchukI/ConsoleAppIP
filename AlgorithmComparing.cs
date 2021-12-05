using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppIP
{
    public interface IAlgorithmComparing
    {
        public bool ComparingStringsParallel(ConcurrentBag<string> listStringsOfFile, string inputIP);
    }

    public class FirstAlgorithmComparing : IAlgorithmComparing
    {
        public bool ComparingStringsParallel(ConcurrentBag<string> listStringsOfFile, string inputIP)
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
    }

    public class SecondAlgorithmComparing : IAlgorithmComparing
    {
        private static string inputIP;
        private static bool isStringMatchFound = false;

        public bool ComparingStringsParallel(ConcurrentBag<string> listStringsOfFile, string inputIP)
        {
            SecondAlgorithmComparing.inputIP = inputIP;
            int numberThreads = Environment.ProcessorCount;
            Thread[] threads = new Thread[numberThreads];

            StartThreads(ref threads, listStringsOfFile);
            WaitAllThreads(threads);

            return isStringMatchFound;
        }

        private static void StartThreads(ref Thread[] threads, ConcurrentBag<string> listStringsOfFile)
        {
            int numberTakeElements;
            int numberSkipElements = 0;

            int listLength = listStringsOfFile.Count();
            int resultOfDivision = listLength / threads.Length;
            int resultOfRemainder = listLength % threads.Length;

            for (int i = 0; i < threads.Length; i++)
            {
                numberTakeElements = GetNumberTakeElements(resultOfDivision, ref resultOfRemainder);

                ConcurrentBag<string> listForCurrentThread = ListToConcurrentBag(listStringsOfFile.Skip(numberSkipElements).Take(numberTakeElements).ToList());
                StartThread(ref threads[i], listForCurrentThread);

                numberSkipElements += numberTakeElements;
            }
        }

        private static ConcurrentBag<T> ListToConcurrentBag<T>(List<T> list)
        {
            ConcurrentBag<T> concurrentBag = new ConcurrentBag<T>();
            foreach (T line in list)
            {
                concurrentBag.Add(line);
            }
            return concurrentBag;
        }

        private static int GetNumberTakeElements(int resultOfDivision, ref int resultOfRemainder)
        {
            int addRemainder = 0;
            if (--resultOfRemainder < 0)
            {
                ++addRemainder;
            }
            return resultOfDivision + addRemainder;
        }

        private static void StartThread(ref Thread thread, ConcurrentBag<string> listStringsOfFile)
        {
            thread = new Thread(new ParameterizedThreadStart(ThreadProc));
            thread.Start(listStringsOfFile);
        }

        private static void ThreadProc(object listStringsOfFile)
        {
            foreach (string line in (ConcurrentBag<string>)listStringsOfFile)
            {
                if (Equals(inputIP, line))
                {
                    isStringMatchFound = true;
                }
                if (isStringMatchFound)
                {
                    break;
                }
            }
        }

        private static void WaitAllThreads(Thread[] threads)
        {
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }
    }


}
