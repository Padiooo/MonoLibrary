using Microsoft.Extensions.Logging;

using System;
using System.IO;

namespace MonoLibrary.Dependency.Loggers.FileLogger;

public sealed class FileLogger(string name, Func<StreamWriter> streamFactory) : ILogger
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var stream = streamFactory.Invoke();

        stream.WriteLine($"[ {DateTime.Now:dd/MM/yyyy HH:mm:ss.fff} {logLevel,-12}] {name}");
        stream.WriteLine(LoggerFormatter.Format(formatter.Invoke(state, exception), exception));
        stream.Flush();
    }
}
