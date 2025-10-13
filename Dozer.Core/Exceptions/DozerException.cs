using System;

namespace Dozer.Core.Exceptions;

/// <summary>
/// Base exception class for all Dozer ORM exceptions.
/// </summary>
public class DozerException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DozerException"/> class.
    /// </summary>
    public DozerException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DozerException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DozerException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DozerException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DozerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

