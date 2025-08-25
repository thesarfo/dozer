using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dozer.Core.Mapping;

namespace Dozer.Core.Data;

public class EntityTracker<T> where T : class
{
    private readonly Dictionary<object, EntityState> _entityStates;
    private readonly Dictionary<object, Dictionary<PropertyInfo, object>> _originalValues;
    private readonly EntityMapper<T> _mapper;

    public EntityTracker()
    {
        _entityStates = new Dictionary<object, EntityState>();
        _originalValues = new Dictionary<object, Dictionary<PropertyInfo, object>>();
        _mapper = new EntityMapper<T>();
    }

    public void TrackEntity(T entity)
    {
        if (entity == null) return;

        var key = GetEntityKey(entity);
        if (!_entityStates.ContainsKey(key))
        {
            _entityStates[key] = EntityState.Added;
            StoreOriginalValues(entity, key);
        }
    }

    public void MarkAsModified(T entity)
    {
        if (entity == null) return;

        var key = GetEntityKey(entity);
        if (_entityStates.ContainsKey(key) && _entityStates[key] == EntityState.Unchanged)
        {
            _entityStates[key] = EntityState.Modified;
        }
    }

    public void MarkAsDeleted(T entity)
    {
        if (entity == null) return;

        var key = GetEntityKey(entity);
        _entityStates[key] = EntityState.Deleted;
    }

    public EntityState GetEntityState(T entity)
    {
        if (entity == null) return EntityState.Unchanged;

        var key = GetEntityKey(entity);
        return _entityStates.TryGetValue(key, out var state) ? state : EntityState.Unchanged;
    }

    public bool IsModified(T entity)
    {
        return GetEntityState(entity) == EntityState.Modified;
    }

    public bool IsAdded(T entity)
    {
        return GetEntityState(entity) == EntityState.Added;
    }

    public bool IsDeleted(T entity)
    {
        return GetEntityState(entity) == EntityState.Deleted;
    }

    public Dictionary<PropertyInfo, object> GetModifiedProperties(T entity)
    {
        if (!IsModified(entity)) return new Dictionary<PropertyInfo, object>();

        var key = GetEntityKey(entity);
        if (!_originalValues.ContainsKey(key)) return new Dictionary<PropertyInfo, object>();

        var modifiedProperties = new Dictionary<PropertyInfo, object>();
        var originalValues = _originalValues[key];

        foreach (var property in _mapper.ColumnMappings.Keys)
        {
            if (property == _mapper.KeyProperty) continue; // Skip primary key

            var currentValue = property.GetValue(entity);
            if (originalValues.TryGetValue(property, out var originalValue))
            {
                if (!Equals(currentValue, originalValue))
                {
                    modifiedProperties[property] = currentValue;
                }
            }
        }

        return modifiedProperties;
    }

    public void AcceptChanges(T entity)
    {
        if (entity == null) return;

        var key = GetEntityKey(entity);
        var state = GetEntityState(entity);

        switch (state)
        {
            case EntityState.Added:
            case EntityState.Modified:
                _entityStates[key] = EntityState.Unchanged;
                StoreOriginalValues(entity, key);
                break;
            case EntityState.Deleted:
                _entityStates.Remove(key);
                _originalValues.Remove(key);
                break;
        }
    }

    public void Clear()
    {
        _entityStates.Clear();
        _originalValues.Clear();
    }

    private object GetEntityKey(T entity)
    {
        if (_mapper.KeyProperty == null)
            throw new InvalidOperationException("Entity must have a key property for change tracking");

        var keyValue = _mapper.KeyProperty.GetValue(entity);
        if (keyValue == null)
            throw new InvalidOperationException("Entity key cannot be null for change tracking");

        return keyValue;
    }

    private void StoreOriginalValues(T entity, object key)
    {
        var originalValues = new Dictionary<PropertyInfo, object>();
        
        foreach (var property in _mapper.ColumnMappings.Keys)
        {
            originalValues[property] = property.GetValue(entity);
        }

        _originalValues[key] = originalValues;
    }
}
