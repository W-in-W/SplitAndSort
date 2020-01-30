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
    public class SortInfoHolder
    {
        private long _currentLong = 0;
        private bool _isNextNumber = true;
        private readonly StreamReader _streamReader;
        public void NextNumber()
        {
            _isNextNumber = true;
        }
        public bool ReturnLong(out long l)
        {
            if (_isNextNumber)
            {
                if (Int64.TryParse(_streamReader.ReadLine(), NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture,
                    out _currentLong))
                {
                    _isNextNumber = false;
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
        public SortInfoHolder(string path)
        {
            _streamReader = new StreamReader(path, Encoding.Default);
        }
    }
    public class Program
    {
        #region Paths
        public static readonly string ProgramDirectory = Directory.GetCurrentDirectory();
        public static readonly string TempDirectory = ProgramDirectory + @"\temp";
        static readonly string SourceFile1 = ProgramDirectory + @"\source1.txt";
        static readonly string SourceFile2 = ProgramDirectory + @"\source2.txt";
        #endregion
        static void SplitAndSort()
        {
            Action del1 = () => SplitFile(SourceFile1, 1);
            Action del2 = () => SplitFile(SourceFile2, 2);
            Directory.CreateDirectory(TempDirectory);
            Task task1 = new Task(del1);
            Task task2 = new Task(del2);
            task1.Start();
            task2.Start();
            task1.Wait();
            task2.Wait();
            Console.WriteLine("Files are splitted");
            FinalSort();
            Thread.Sleep(2000);
            Directory.Delete(TempDirectory, true);
        }
        static void SplitFile (string path, int index)
        {
            if (File.Exists(path))
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
                        using (StreamWriter sw = new StreamWriter(TempDirectory + $@"\temp{index} {counter}.txt", false, Encoding.Default))
                        {
                            tempFile.ForEach(l => sw.WriteLine(l));
                        }
                        tempFile.Clear();
                    }
                }
                Console.WriteLine($"source{index} has splitted");
            }
            else
            {
                Console.WriteLine($"source{index} not found");
            }
        }
        static void FinalSort()
        {
            Console.WriteLine("Final sorting has started");
            string[] filesPaths = Directory.GetFiles(TempDirectory);
            List<long> longsForSort = new List<long>(filesPaths.Length);
            List<SortInfoHolder> sortInfoHolders = new List<SortInfoHolder>(filesPaths.Length);
            List<long> longsToWrite = new List<long>();
            for (int i = 0; i < filesPaths.Length; i++)
            {
                sortInfoHolders.Add(new SortInfoHolder(filesPaths[i]));
            }
            for (int filesLeft = filesPaths.Length; filesLeft > 0;)
            {
                for (int i = 0; i < sortInfoHolders.Count(); i++)
                {
                    if (sortInfoHolders[i] != null && sortInfoHolders[i].ReturnLong(out long longNumber))
                    {
                        longsForSort.Add(longNumber);
                    }
                    else
                    {
                        sortInfoHolders.Remove(sortInfoHolders[i]);
                        filesLeft--;
                    }
                }
                if (longsForSort.Count > 0)
                {
                    long minLong = longsForSort.Min(m => m);
                    longsToWrite.Add(minLong);
                    int minPos = longsForSort.IndexOf(minLong);
                    if (sortInfoHolders[minPos] != null)
                    {
                        sortInfoHolders[minPos].NextNumber();
                    }
                }
                if (filesLeft == 0 && longsForSort.Count == 0 || longsToWrite.Count == 1000000)
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
                longsForSort.Clear();
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