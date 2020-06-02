using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public class Sort
    {
        #region Paths

        public static readonly string ProgramDirectory = Directory.GetCurrentDirectory();
        public static readonly string TempDirectory = ProgramDirectory + @"\temp";
        private static readonly string SourceFile1 = ProgramDirectory + @"\source1.txt";
        private static readonly string SourceFile2 = ProgramDirectory + @"\source2.txt";

        #endregion Paths

        public static void SplitAndSort()
        {
            Directory.CreateDirectory(TempDirectory);
            Task t1 = Task.Run(() => SplitFile(SourceFile1, 1));
            Task t2 = Task.Run(() => SplitFile(SourceFile2, 2));
            Task.WaitAll(new[] { t1, t2 });
            Console.WriteLine("Files are splitted");
            FinalSort();
            Thread.Sleep(2000);
            Directory.Delete(TempDirectory, true);
        }

        private static void SplitFile(string path, int index)
        {
            if (File.Exists(path))
            {
                Console.WriteLine($"Splitting has started (source{index})");
                List<long> tempLongList = new List<long>();
                bool isFileEnded = false;
                using (StreamReader sr = new StreamReader(SourceFile1, Encoding.Default))
                {
                    long tempLong = 0;
                    int counter = 0;
                    while (!isFileEnded)
                    {
                        for (int i = 0; i < 24000000; i++)
                        {
                            if (Int64.TryParse(sr.ReadLine(), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out tempLong))
                            {
                                tempLongList.Add(tempLong);
                            }
                            else
                            {
                                isFileEnded = true;
                                break;
                            }
                        }
                        counter++;
                        tempLongList.Sort();
                        using (StreamWriter sw = new StreamWriter(TempDirectory + $@"\temp{index} {counter}.txt", false, Encoding.Default))
                        {
                            tempLongList.ForEach(l => sw.WriteLine(l));
                        }
                        tempLongList.Clear();
                    }
                }
                Console.WriteLine($"source{index} has splitted");
            }
            else
            {
                Console.WriteLine($"source{index} not found");
            }
        }

        private static void FinalSort()
        {
            Console.WriteLine("Final sorting has started");
            StreamWriter sw = new StreamWriter(ProgramDirectory + $@"\SortedFile.txt", true, Encoding.Default);
            string[] filesPaths = Directory.GetFiles(TempDirectory);
            List<long> longsForSort = new List<long>(filesPaths.Length);
            List<SortInfoHolder> sortInfoHolders = new List<SortInfoHolder>(filesPaths.Length);
            for (int i = 0; i < filesPaths.Length; i++)
            {
                sortInfoHolders.Add(new SortInfoHolder(filesPaths[i]));
            }
            for (int filesLeft = filesPaths.Length; filesLeft > 0;)
            {
                for (int i = 0; i < sortInfoHolders.Count; i++)
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
                    sw.WriteLine(minLong);
                    int minPos = longsForSort.IndexOf(minLong);
                    if (sortInfoHolders[minPos] != null)
                    {
                        sortInfoHolders[minPos].NextNumber();
                    }
                }
                longsForSort.Clear();
            }
            sw.Close();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Sort.SplitAndSort();
            sw.Stop();
            Console.WriteLine($"Sorting is over. Elapsed time: {sw.ElapsedMilliseconds} ms" + "\n" + "Press any key to exit.");
            Console.ReadKey();
        }
    }
}