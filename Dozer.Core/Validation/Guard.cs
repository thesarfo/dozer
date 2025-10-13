using System;
using System.Collections.Generic;
using System.Linq;
using Dozer.Core.Exceptions;

namespace Dozer.Core.Validation;

/// <summary>
/// Provides validation methods for method parameters.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Ensures that the specified value is not null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public static void AgainstNull<T>(T value, string paramName) where T : class
    {
        if (value == null)
        {
            throw new ArgumentNullException(paramName, $"Parameter '{paramName}' cannot be null.");
        }
    }

    /// <summary>
    /// Ensures that the specified string is not null or empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentException">Thrown when the string is null or empty.</exception>
    public static void AgainstNullOrEmpty(string value, string paramName)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException($"Parameter '{paramName}' cannot be null or empty.", paramName);
        }
    }

    /// <summary>
    /// Ensures that the specified string is not null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentException">Thrown when the string is null, empty, or whitespace.</exception>
    public static void AgainstNullOrWhiteSpace(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Parameter '{paramName}' cannot be null, empty, or whitespace.", paramName);
        }
    }

    /// <summary>
    /// Ensures that the specified entity has a primary key defined.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="hasKey">Whether the entity has a key.</param>
    /// <param name="entityType">The entity type name.</param>
    /// <exception cref="EntityValidationException">Thrown when no key is defined.</exception>
    public static void AgainstMissingKey<T>(bool hasKey, string entityType) where T : class
    {
        if (!hasKey)
        {
            throw new EntityValidationException(
                entityType, 
                "Entity must have a property marked with [Key] attribute."
            );
        }
    }

    /// <summary>
    /// Ensures that a collection is not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="value">The collection to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentException">Thrown when the collection is null or empty.</exception>
    public static void AgainstNullOrEmpty<T>(IEnumerable<T> value, string paramName)
    {
        if (value == null || !value.Any())
        {
            throw new ArgumentException($"Parameter '{paramName}' cannot be null or empty.", paramName);
        }
    }

    /// <summary>
    /// Ensures that a value is within a specified range.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is out of range.</exception>
    public static void AgainstOutOfRange<T>(T value, T min, T max, string paramName) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                $"Parameter '{paramName}' must be between {min} and {max}."
            );
        }
    }
}

