using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Concurrent;
using System.IO;

namespace MonoLibrary.Dependency.Loggers.FileLogger;

[ProviderAlias("FileLogger")]
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly IDisposable _onChangeToken;
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

    private StreamWriter _stream;

    public FileLoggerProvider(IOptionsMonitor<FileLoggerConfiguration> config)
    {
        OnConfigurationChanged(config.CurrentValue);
        _onChangeToken = config.OnChange(OnConfigurationChanged);
    }

    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new FileLogger(name, () => _stream));

    private void OnConfigurationChanged(FileLoggerConfiguration config)
    {
        if (_stream != null)
        {
            _stream.Flush();
            _stream.Dispose();
        }

        var file = new FileInfo(config.GetFullPath());

        file.Directory.Create();

        if (file.Exists)
        {
            _stream = new StreamWriter(file.Open(FileMode.Append, FileAccess.Write, FileShare.Read));
            _stream.WriteLine();
        }
        else
            _stream = new StreamWriter(file.Open(FileMode.Create, FileAccess.Write, FileShare.Read));
    }

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
        _stream?.Flush();
        _stream?.Dispose();
    }
}
