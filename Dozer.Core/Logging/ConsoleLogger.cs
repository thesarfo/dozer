using System;

namespace Dozer.Core.Logging;

/// <summary>
/// A simple console-based logger implementation.
/// </summary>
public class ConsoleLogger : IDozerLogger
{
    private readonly bool _isEnabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
    /// </summary>
    /// <param name="isEnabled">Whether logging is enabled.</param>
    public ConsoleLogger(bool isEnabled = true)
    {
        _isEnabled = isEnabled;
    }

    /// <inheritdoc/>
    public void LogDebug(string message)
    {
        if (!_isEnabled) return;
        Log("DEBUG", message, ConsoleColor.Gray);
    }

    /// <inheritdoc/>
    public void LogInformation(string message)
    {
        if (!_isEnabled) return;
        Log("INFO", message, ConsoleColor.White);
    }

    /// <inheritdoc/>
    public void LogWarning(string message)
    {
        if (!_isEnabled) return;
        Log("WARN", message, ConsoleColor.Yellow);
    }

    /// <inheritdoc/>
    public void LogError(string message, Exception exception = null)
    {
        if (!_isEnabled) return;
        Log("ERROR", message, ConsoleColor.Red);
        if (exception != null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  Exception: {exception.Message}");
            Console.WriteLine($"  Stack Trace: {exception.StackTrace}");
            Console.ResetColor();
        }
    }

    /// <inheritdoc/>
    public void LogSql(string sql, object parameters = null)
    {
        if (!_isEnabled) return;
        Log("SQL", sql, ConsoleColor.Cyan);
        if (parameters != null)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"  Parameters: {parameters}");
            Console.ResetColor();
        }
    }

    private void Log(string level, string message, ConsoleColor color)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        Console.ForegroundColor = color;
        Console.WriteLine($"[{timestamp}] [{level}] {message}");
        Console.ResetColor();
    }
}

