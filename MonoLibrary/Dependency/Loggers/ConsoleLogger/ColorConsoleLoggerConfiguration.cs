using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;

namespace MonoLibrary.Dependency.Loggers.ConsoleLogger
{
    public sealed class ColorConsoleLoggerConfiguration
    {
        public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
        {
            [LogLevel.Trace] = ConsoleColor.DarkBlue,
            [LogLevel.Debug] = ConsoleColor.Blue,
            [LogLevel.Information] = ConsoleColor.Green,
            [LogLevel.Warning] = ConsoleColor.DarkYellow,
            [LogLevel.Error] = ConsoleColor.Red,
            [LogLevel.Critical] = ConsoleColor.DarkMagenta
        };
    }
}
