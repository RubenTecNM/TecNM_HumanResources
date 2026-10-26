using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace HumanResourcesProduction.Crosscutting.Logging;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logDirectory;
    private readonly object _lock = new();

    public FileLoggerProvider(string logDirectory)
    {
        _logDirectory = logDirectory;
        Directory.CreateDirectory(_logDirectory);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _logDirectory, _lock);
    }

    public void Dispose()
    {
    }
}

public class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _logDirectory;
    private readonly object _lock;

    public FileLogger(string categoryName, string logDirectory, object @lock)
    {
        _categoryName = categoryName;
        _logDirectory = logDirectory;
        _lock = @lock;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message) && exception == null) return;

        var now = DateTime.Now;
        var dateStr = now.ToString("yyyy-MM-dd");
        var logTime = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var levelTag = logLevel switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO ",
            LogLevel.Warning => "WARN ",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "FATAL",
            _ => "INFO "
        };

        var sb = new StringBuilder();
        sb.Append($"[{logTime}] [{levelTag}] [{_categoryName}] {message}");
        if (exception != null)
        {
            sb.AppendLine();
            sb.Append(exception.ToString());
        }
        sb.AppendLine();

        var text = sb.ToString();

        lock (_lock)
        {
            try
            {
                // 1. Registro en archivo general diario
                var appLogPath = Path.Combine(_logDirectory, $"app-{dateStr}.log");
                File.AppendAllText(appLogPath, text, Encoding.UTF8);

                // 2. Registro dedicado en archivo exclusivo de sincronización Excel
                if (_categoryName.Contains("Sync", StringComparison.OrdinalIgnoreCase) ||
                    _categoryName.Contains("Excel", StringComparison.OrdinalIgnoreCase) ||
                    message.Contains("[ExcelSync]", StringComparison.OrdinalIgnoreCase))
                {
                    var syncLogPath = Path.Combine(_logDirectory, $"sync-excel-{dateStr}.log");
                    File.AppendAllText(syncLogPath, text, Encoding.UTF8);
                }
            }
            catch
            {
                // El logging no debe interrumpir la ejecucion si hay error de I/O
            }
        }
    }
}

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string logDirectory)
    {
        builder.AddProvider(new FileLoggerProvider(logDirectory));
        return builder;
    }
}
