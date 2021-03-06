﻿using LogProcessor.Data;
using LogProcessor.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LogProcessor.LogProcessor
{
    public class LogProcessor : ILogProcessor
    {
        private readonly ILogRepository _logRepository;

        public LogProcessor() : this(new LogRepository()) { }

        public LogProcessor(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        private const string IP_REGEX = @"([ ]{1}[\d]{3}[\.]{1}[\d]{3}[\.]{1}[\d]{1,3}[\.]{1}[\d]{1,3}[ ]{1})";
        private const string DATE_REGEX = @"([\d]{10}[\.]{1}[\d]{3})";
        private const string URL_REGEX = @"([http]{4}.*?[ ]{1})|([\s]{1}[a-z0-9\.-][^\s]*?[:]{1}[0-9]{1,}[\s]{1})";
        
        private const string LOG_DATA_SEPARATOR = ";";
        private const string DATE_FORMAT = "dd/MM/yy";
                
        public void Process(string filePath, string newFilePath)
        {
            long totalMilliseconds = 0;

            var timer = new Stopwatch();
            timer.Start();

            var lines = ReadFile(filePath);

            timer.Stop();
            totalMilliseconds += timer.ElapsedMilliseconds;
            Console.WriteLine("Read file: " + timer.ElapsedMilliseconds + " ms");

            timer.Restart();

            var logList = Parse(lines);
            
            timer.Stop();
            totalMilliseconds += timer.ElapsedMilliseconds;
            Console.WriteLine("Parse file: " + timer.ElapsedMilliseconds + " ms");

            var databaseTask = new Task(() => {
                var databaseTimer = new Stopwatch();
                databaseTimer.Start();
                _logRepository.Save(logList);
                databaseTimer.Stop();
                totalMilliseconds += databaseTimer.ElapsedMilliseconds;
                Console.WriteLine("Write Database: " + databaseTimer.ElapsedMilliseconds + " ms");
            });

            var writeFileTask = new Task(() => {
                var fileTimer = new Stopwatch();
                fileTimer.Start();
                WriteFile(logList, newFilePath);
                fileTimer.Stop();
                totalMilliseconds += fileTimer.ElapsedMilliseconds;
                Console.WriteLine("Write File: " + fileTimer.ElapsedMilliseconds + " ms");
            });
                        
            databaseTask.Start();
            writeFileTask.Start();

            databaseTask.Wait();
            writeFileTask.Wait();

            Console.WriteLine("Total: " + totalMilliseconds + " ms");
        }

        private string[] ReadFile(string filePath)
        {
            return File.ReadAllLines(filePath);
        }

        private List<LogItemContract> Parse(string[] lines)
        {
            var ipRegex = new Regex(IP_REGEX);
            var dateRegex = new Regex(DATE_REGEX);
            var urlRegex = new Regex(URL_REGEX);

            var logList = new List<LogItemContract>();
            var logListLock = new object();
            
            Parallel.ForEach(lines, (string line) =>
            {
                var logItem = new LogItemContract();                
                logItem.Date = dateRegex.Match(line).Value.ToDateFromUnixTimestamp();
                logItem.IP = ipRegex.Match(line).Value.Trim();
                logItem.URL = urlRegex.Match(line).Value.Trim();

                lock (logListLock)
                {
                    logList.Add(logItem);
                }
            });

            return logList;
        }

        private void WriteFile(List<LogItemContract> items, string newFilePath)
        {
            using (var streamWriter = new StreamWriter(newFilePath))
            {
                streamWriter.Write(items.Aggregate(new StringBuilder(), (result, logItem) =>
                {
                    result.Append(logItem.Date.ToString(DATE_FORMAT));
                    result.Append(LOG_DATA_SEPARATOR);
                    result.Append(logItem.IP);
                    result.Append(LOG_DATA_SEPARATOR);
                    result.AppendLine(logItem.URL);
                    return result;
                }).ToString());
            }
        }
    }
}
