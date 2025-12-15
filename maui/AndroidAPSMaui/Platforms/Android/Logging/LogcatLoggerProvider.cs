#if ANDROID
using System;
using Android.Util;
using Microsoft.Extensions.Logging;

namespace AndroidAPSMaui.Platforms.Android.Logging;

/// <summary>
/// Routes .NET logging to Android logcat with a consistent tag so debug output is easy to filter.
/// </summary>
public class LogcatLoggerProvider : ILoggerProvider
{
    private readonly string _tag;

    public LogcatLoggerProvider(string tag)
    {
        _tag = tag;
    }

    public ILogger CreateLogger(string categoryName) => new LogcatLogger(_tag, categoryName);

    public void Dispose()
    {
    }

    private sealed class LogcatLogger : ILogger
    {
        private readonly string _tag;
        private readonly string _category;

        public LogcatLogger(string tag, string category)
        {
            _tag = tag;
            _category = category;
        }

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message) && exception == null)
            {
                return;
            }

            var fullMessage = string.IsNullOrEmpty(_category) ? message : $"[{_category}] {message}";
            if (exception != null)
            {
                fullMessage += $" Exception: {exception}";
            }

            switch (logLevel)
            {
                case LogLevel.Trace:
                    Log.Verbose(_tag, fullMessage);
                    break;
                case LogLevel.Debug:
                    Log.Debug(_tag, fullMessage);
                    break;
                case LogLevel.Information:
                    Log.Info(_tag, fullMessage);
                    break;
                case LogLevel.Warning:
                    Log.Warn(_tag, fullMessage);
                    break;
                case LogLevel.Error:
                    Log.Error(_tag, fullMessage);
                    break;
                case LogLevel.Critical:
                    Log.Wtf(_tag, fullMessage);
                    break;
                default:
                    Log.Debug(_tag, fullMessage);
                    break;
            }
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
#endif
