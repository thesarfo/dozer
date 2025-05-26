using System;

namespace Dozer.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ColumnAttribute : Attribute
{
    public string Name { get; }
    public string DbType { get; }

    public ColumnAttribute(string name = null, string dbType = null)
    {
        Name = name;
        DbType = dbType;
    }
} 