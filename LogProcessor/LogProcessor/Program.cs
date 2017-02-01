using LogProcessor.LogProcessor;
using System;

namespace LogProcessor
{
    class Program
    {
        private const string LOG_FILE_PATH = "C:/access.log.0";
        private const string PROCESSED_LOG_FILE_PATH = "C:/access.log.processed.0";

        private static readonly ILogProcessor _logProcessor;

        static Program()
        {
            _logProcessor = new LogProcessor.LogProcessor();
        }
        
        static void Main(string[] args)
        {
            _logProcessor.Process(LOG_FILE_PATH, PROCESSED_LOG_FILE_PATH);
            Console.Write("Done! Any key to exit.");
            Console.Read();
        }
    }
}
