﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

using System;

namespace MonoLibrary.Dependency.Loggers.ConsoleLogger
{
    public static class ColorConsoleLoggerExtensions
    {
        public static ILoggingBuilder AddColorConsoleLogger(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ColorConsoleLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions<ColorConsoleLoggerConfiguration, ColorConsoleLoggerProvider>(builder.Services);

            return builder;
        }

        public static ILoggingBuilder AddColorConsoleLogger(this ILoggingBuilder builder, Action<ColorConsoleLoggerConfiguration> configure)
        {
            builder.AddColorConsoleLogger();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}
