namespace LogProcessor.LogProcessor
{
    public interface ILogProcessor
    {
        void Process(string filePath, string newFilePath);
    }
}
