namespace Dozer.Core.Data;

/// <summary>
/// Represents the state of an entity in the change tracking system.
/// </summary>
public enum EntityState
{
    /// <summary>
    /// The entity is unchanged and in sync with the database.
    /// </summary>
    Unchanged,
    
    /// <summary>
    /// The entity has been modified since it was loaded or last saved.
    /// </summary>
    Modified,
    
    /// <summary>
    /// The entity has been newly added and not yet persisted to the database.
    /// </summary>
    Added,
    
    /// <summary>
    /// The entity is marked for deletion.
    /// </summary>
    Deleted
}
