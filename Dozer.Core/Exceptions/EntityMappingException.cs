using System;

namespace Dozer.Core.Exceptions;

/// <summary>
/// Exception thrown when entity mapping fails.
/// </summary>
public class EntityMappingException : DozerException
{
    /// <summary>
    /// Gets the name of the entity type that failed mapping.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityMappingException"/> class.
    /// </summary>
    /// <param name="entityType">The entity type name.</param>
    /// <param name="message">The mapping error message.</param>
    public EntityMappingException(string entityType, string message) 
        : base($"Mapping failed for entity '{entityType}': {message}")
    {
        EntityType = entityType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityMappingException"/> class.
    /// </summary>
    /// <param name="entityType">The entity type name.</param>
    /// <param name="message">The mapping error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public EntityMappingException(string entityType, string message, Exception innerException) 
        : base($"Mapping failed for entity '{entityType}': {message}", innerException)
    {
        EntityType = entityType;
    }
}

