using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Программа сортирует 2 исходных файла содержащих строки с числами (по одному числу на каждую строку) с именами 
/// "source1.txt" и "source2.txt" и делает из них третий файл с именем "SortedFile.txt" в котором содержатся значения
/// из исходных файлов отсортированные по возрастанию. Размер исходных файлов неограничен и может превышать размер
/// доступной оперативной памяти.
/// </summary>

namespace SplitAndSort
{
    public class SortHelper
    {
        private long _currentLong = 0;
        private bool _nextNumber = true;
        private readonly StreamReader _streamReader;
        public void NextNumber()
        {
            _nextNumber = true;
        }
        public bool ReturnLong(out long l)
        {
            if (_nextNumber)
            {
                if (Int64.TryParse(_streamReader.ReadLine(), NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture,
                    out _currentLong))
                {
                    _nextNumber = false;
                    l = _currentLong;
                    return true;
                }
                else
                {
                    _streamReader.Close();
                    l = 0;
                    return false;
                }
            }
            else
            {
                l = _currentLong;
                return true;
            }
        }
        public SortHelper(string path)
        {
            _streamReader = new StreamReader(path, Encoding.Default);
        }
    }
    internal class Program
    {
        #region Paths
        static readonly string ProgramDirectory = Directory.GetCurrentDirectory();
        static readonly string TempDirectory = ProgramDirectory + @"\temp";
        static readonly string SourceFile1 = ProgramDirectory + @"\source1.txt";
        static readonly string SourceFile2 = ProgramDirectory + @"\source2.txt";
        #endregion
        static void SplitAndSort()
        {
            Action del1 = SplitFile1;
            Action del2 = SplitFile2;
            Directory.CreateDirectory(TempDirectory);
            Task task1 = new Task(del1);
            Task task2 = new Task(del2);
            task1.Start();
            task2.Start();
            task1.Wait();
            task2.Wait();
            Console.WriteLine("Files are splitted.");
            FinalSort();
            Thread.Sleep(2000);
            Directory.Delete(TempDirectory, true);
        }
        static void SplitFile1()
        {
            if (File.Exists(SourceFile1))
            {
                Console.WriteLine("Splitting has started (source1)");
                List<long> tempFile = new List<long>();
                bool isFileEnded = false;
                using (StreamReader sr = new StreamReader(SourceFile1, Encoding.Default))
                {
                    long templong = 0;
                    int counter = 0;
                    while (!isFileEnded)
                    {
                        for (int i = 0; i < 24000000; i++)
                        {
                            if (Int64.TryParse(sr.ReadLine(), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out templong))
                            {
                                tempFile.Add(templong);
                            }
                            else
                            {
                                isFileEnded = true;
                                break;
                            }
                        }
                        counter++;
                        tempFile.Sort();
                        using (StreamWriter sw = new StreamWriter(TempDirectory + $@"\temp1 {counter}.txt", false, Encoding.Default))
                        {
                            tempFile.ForEach(l => sw.WriteLine(l));
                        }
                        tempFile.Clear();
                    }
                }
                Console.WriteLine("source1 has splitted");
            }
            else
            {
                Console.WriteLine("source1 not found");
            }
        }
        static void SplitFile2()
        {
            if (File.Exists(SourceFile2))
            {
                Console.WriteLine("Splitting has started (source2)");
                List<long> tempFile = new List<long>();
                bool isFileEnded = false;
                using (StreamReader sr = new StreamReader(SourceFile2, Encoding.Default))
                {
                    long templong = 0;
                    int counter = 0;
                    while (!isFileEnded)
                    {
                        for (int i = 0; i < 24000000; i++)
                        {
                            if (Int64.TryParse(sr.ReadLine(), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out templong))
                            {
                                tempFile.Add(templong);
                            }
                            else
                            {
                                isFileEnded = true;
                                break;
                            }
                        }
                        counter++;
                        tempFile.Sort();
                        using (StreamWriter sw = new StreamWriter(TempDirectory + $@"\temp2 {counter}.txt", false, Encoding.Default))
                        {
                            tempFile.ForEach(l => sw.WriteLine(l));
                        }
                        tempFile.Clear();
                    }
                }
                Console.WriteLine("source2 has splitted");
            }
            else
            {
                Console.WriteLine("source2 not found");
            }
        }
        static void FinalSort()
        {
            Console.WriteLine("Final sorting has started");
            string[] files = Directory.GetFiles(TempDirectory);
            List<long> lineOfLongs = new List<long>(files.Length);
            List<SortHelper> sortHelpers = new List<SortHelper>(files.Length);
            for (int i = 0; i < files.Length; i++)
            {
                sortHelpers.Add(new SortHelper(files[i]));
            }
            List<long> longsToWrite = new List<long>();
            for (int filesLeft = files.Length; filesLeft > 0;)
            {
                for (int i = 0; i < sortHelpers.Count(); i++)
                {
                    if (sortHelpers[i] != null && sortHelpers[i].ReturnLong(out long longNumber))
                    {
                        lineOfLongs.Add(longNumber);
                    }
                    else
                    {
                        sortHelpers.Remove(sortHelpers[i]);
                        filesLeft--;
                    }
                }
                if (lineOfLongs.Count > 0)
                {
                    long minLong = lineOfLongs.Min(m => m); // Predicate<T>
                    longsToWrite.Add(minLong);
                    int minPos = lineOfLongs.IndexOf(minLong);
                    if (sortHelpers[minPos] != null)
                    {
                        sortHelpers[minPos].NextNumber();
                    }
                }
                if (filesLeft == 0 && lineOfLongs.Count == 0 || longsToWrite.Count == 1000000)
                {
                    using (StreamWriter sw = new StreamWriter(ProgramDirectory + $@"\SortedFile.txt", true, Encoding.Default))
                    {
                        foreach (var l in longsToWrite)
                        {
                            sw.WriteLine(l);
                        }
                    }
                    longsToWrite.Clear();
                }
                lineOfLongs.Clear();
            }
        }

        public static void Main(string[] args)
        {
            SplitAndSort();
            Console.WriteLine("Sorting is over. Press any key to exit.");
            Console.ReadKey();
        }
    }
}