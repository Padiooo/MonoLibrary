using Microsoft.Extensions.Logging;

using System;

namespace MonoLibrary.Dependency.Loggers.ConsoleLogger;

public sealed class ColorConsoleLogger(string name, Func<ColorConsoleLoggerConfiguration> getCurrentConfig) : ILogger
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);

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

        ColorConsoleLoggerConfiguration config = getCurrentConfig.Invoke();

        ConsoleColor originalColor = Console.ForegroundColor;
        ConsoleColor color = config.LogLevelToColorMap[logLevel];

        Console.ForegroundColor = color;
        Console.WriteLine($"[ {DateTime.Now:dd/MM/yyyy HH:mm:ss.fff} {logLevel,11} ] {name}");

        Console.ForegroundColor = originalColor;
        Console.WriteLine(LoggerFormatter.Format(formatter.Invoke(state, exception), exception));
    }
}
