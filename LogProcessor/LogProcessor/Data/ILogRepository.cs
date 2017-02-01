using LogProcessor.LogProcessor;
using System.Collections.Generic;

namespace LogProcessor.Data
{
    public interface ILogRepository
    {
        void Save(List<LogItemContract> items);
    }
}
