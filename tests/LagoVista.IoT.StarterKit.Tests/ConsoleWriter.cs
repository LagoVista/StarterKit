// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7b0c4e40538934f60784e3475f258dd1f4f708e44ee1ec6d033f319fa2a0b98d
// IndexVersion: 2
// --- END CODE INDEX META ---
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
