using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Logging.Models;
using System;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit.Tests
{
    public class ConsoleWriter : ILogWriter
    {
        public Task WriteError(LogRecord record)
        {
            Console.WriteLine(record.Message);
            return Task.CompletedTask;
        }

        public Task WriteEvent(LogRecord record)
        {

            Console.WriteLine(record.Message);
            return Task.CompletedTask;
        }
    }
}
