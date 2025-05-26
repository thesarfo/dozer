using System;
using System.Linq;
using Dozer.Core.Mapping;
using Dozer.Core.Tests.TestModels;
using Xunit;

namespace Dozer.Core.Tests.Mapping;

public class EntityMapperTests
{
    private readonly EntityMapper<User> _mapper;

    public EntityMapperTests()
    {
        _mapper = new EntityMapper<User>();
    }

    [Fact]
    public void TableName_ShouldMatchAttribute()
    {
        Assert.Equal("Users", _mapper.TableName);
    }

    [Fact]
    public void KeyProperty_ShouldBeIdProperty()
    {
        Assert.NotNull(_mapper.KeyProperty);
        Assert.Equal("Id", _mapper.KeyProperty.Name);
        Assert.True(_mapper.IsAutoIncrement);
    }

    [Fact]
    public void ColumnMappings_ShouldMapCorrectly()
    {
        var mappings = _mapper.ColumnMappings;
        
        var usernameProperty = typeof(User).GetProperty("Username");
        var emailProperty = typeof(User).GetProperty("Email");
        var createdAtProperty = typeof(User).GetProperty("CreatedAt");

        Assert.Equal("UserName", mappings[usernameProperty]);
        Assert.Equal("EmailAddress", mappings[emailProperty]);
        Assert.Equal("CreatedAt", mappings[createdAtProperty]); // Default mapping
    }

    [Fact]
    public void DbTypeMappings_ShouldMapCorrectly()
    {
        var mappings = _mapper.DbTypeMappings;
        
        var idProperty = typeof(User).GetProperty("Id");
        var usernameProperty = typeof(User).GetProperty("Username");
        var balanceProperty = typeof(User).GetProperty("Balance");
        var createdAtProperty = typeof(User).GetProperty("CreatedAt");

        Assert.Equal("INT", mappings[idProperty]);
        Assert.Equal("NVARCHAR(100)", mappings[usernameProperty]);
        Assert.Equal("DECIMAL(10,2)", mappings[balanceProperty]);
        Assert.Equal("DATETIME2", mappings[createdAtProperty]);
    }

    [Fact]
    public void Constructor_ShouldMapAllProperties()
    {
        Assert.Equal(5, _mapper.ColumnMappings.Count);
        Assert.Equal(5, _mapper.DbTypeMappings.Count);
    }
} 