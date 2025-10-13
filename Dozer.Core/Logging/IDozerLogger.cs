using System;

namespace Dozer.Core.Logging;

/// <summary>
/// Represents a logger for Dozer ORM operations.
/// </summary>
public interface IDozerLogger
{
    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogDebug(string message);

    /// <summary>
    /// Logs an information message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogInformation(string message);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogWarning(string message);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception associated with the error.</param>
    void LogError(string message, Exception exception = null);

    /// <summary>
    /// Logs SQL command execution.
    /// </summary>
    /// <param name="sql">The SQL command.</param>
    /// <param name="parameters">The SQL parameters.</param>
    void LogSql(string sql, object parameters = null);
}

