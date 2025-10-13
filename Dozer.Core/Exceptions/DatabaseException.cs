using System;

namespace Dozer.Core.Exceptions;

/// <summary>
/// Exception thrown when a database operation fails.
/// </summary>
public class DatabaseException : DozerException
{
    /// <summary>
    /// Gets the SQL command that failed, if available.
    /// </summary>
    public string SqlCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public DatabaseException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DatabaseException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="sqlCommand">The SQL command that failed.</param>
    /// <param name="innerException">The inner exception.</param>
    public DatabaseException(string message, string sqlCommand, Exception innerException) 
        : base($"{message} SQL: {sqlCommand}", innerException)
    {
        SqlCommand = sqlCommand;
    }
}

