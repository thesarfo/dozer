using System;

namespace Dozer.Core.Exceptions;

/// <summary>
/// Exception thrown when entity validation fails.
/// </summary>
public class EntityValidationException : DozerException
{
    /// <summary>
    /// Gets the name of the entity type that failed validation.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityValidationException"/> class.
    /// </summary>
    /// <param name="entityType">The entity type name.</param>
    /// <param name="message">The validation error message.</param>
    public EntityValidationException(string entityType, string message) 
        : base($"Validation failed for entity '{entityType}': {message}")
    {
        EntityType = entityType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityValidationException"/> class.
    /// </summary>
    /// <param name="entityType">The entity type name.</param>
    /// <param name="message">The validation error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public EntityValidationException(string entityType, string message, Exception innerException) 
        : base($"Validation failed for entity '{entityType}': {message}", innerException)
    {
        EntityType = entityType;
    }
}

