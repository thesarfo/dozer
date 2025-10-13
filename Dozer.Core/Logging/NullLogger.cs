using System;

namespace Dozer.Core.Logging;

/// <summary>
/// A null logger that doesn't log anything. Used as a default when no logger is configured.
/// </summary>
public class NullLogger : IDozerLogger
{
    /// <summary>
    /// Gets a singleton instance of the null logger.
    /// </summary>
    public static NullLogger Instance { get; } = new NullLogger();

    private NullLogger()
    {
    }

    /// <inheritdoc/>
    public void LogDebug(string message)
    {
        // No-op
    }

    /// <inheritdoc/>
    public void LogInformation(string message)
    {
        // No-op
    }

    /// <inheritdoc/>
    public void LogWarning(string message)
    {
        // No-op
    }

    /// <inheritdoc/>
    public void LogError(string message, Exception exception = null)
    {
        // No-op
    }

    /// <inheritdoc/>
    public void LogSql(string sql, object parameters = null)
    {
        // No-op
    }
}

