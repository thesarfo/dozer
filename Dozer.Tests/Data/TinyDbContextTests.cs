using Dozer.Core.Attributes;
using Dozer.Core.Data;
using Microsoft.Data.Sqlite;

namespace Dozer.Tests.Data;

public class TinyDbContextTests : IDisposable
{
    private readonly DbConnectionFactory _connectionFactory;
    private readonly TinyDbContext _context;
    private readonly SqliteConnection _connection;

    public TinyDbContextTests()
    {
        // Setup SQLite in-memory database
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        // Create test table
        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserName TEXT NOT NULL,
                    Email TEXT,
                    CreatedAt TEXT,
                    Balance DECIMAL
                )";
            cmd.ExecuteNonQuery();
        }

        _connectionFactory = new DbConnectionFactory(_connection.ConnectionString, SqliteFactory.Instance);
        _context = new TinyDbContext(_connectionFactory);
    }

    [Table("Users")]
    private class TestUser
    {
        [Key]
        public int Id { get; set; }

        [Column("UserName")]
        public string Username { get; set; }

        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Balance { get; set; }
    }

    [Fact]
    public void Insert_ShouldCreateNewRecord()
    {
        // Arrange
        var user = new TestUser
        {
            Username = "testuser",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            Balance = 100.50m
        };

        // Act
        _context.Insert(user);

        // Assert
        var users = _context.List<TestUser>();
        Assert.Single(users);
        var savedUser = users.First();
        Assert.Equal(user.Username, savedUser.Username);
        Assert.Equal(user.Email, savedUser.Email);
        Assert.Equal(user.Balance, savedUser.Balance);
    }

    [Fact]
    public void Update_ShouldModifyExistingRecord()
    {
        // Arrange
        var user = new TestUser
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        _context.Insert(user);
        var users = _context.List<TestUser>();
        var savedUser = users.First();

        // Act
        savedUser.Email = "updated@example.com";
        _context.Update(savedUser);

        // Assert
        users = _context.List<TestUser>();
        Assert.Single(users);
        Assert.Equal("updated@example.com", users.First().Email);
    }

    [Fact]
    public void Delete_ShouldRemoveRecord()
    {
        // Arrange
        var user = new TestUser
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        _context.Insert(user);
        var users = _context.List<TestUser>();
        var savedUser = users.First();

        // Act
        _context.Delete(savedUser);

        // Assert
        users = _context.List<TestUser>();
        Assert.Empty(users);
    }

    [Fact]
    public void List_ShouldReturnAllRecords()
    {
        // Arrange
        var users = new[]
        {
            new TestUser { Username = "user1", Email = "user1@example.com" },
            new TestUser { Username = "user2", Email = "user2@example.com" },
            new TestUser { Username = "user3", Email = "user3@example.com" }
        };

        foreach (var user in users)
        {
            _context.Insert(user);
        }

        // Act
        var result = _context.List<TestUser>();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(users[0].Username, result[0].Username);
        Assert.Equal(users[1].Username, result[1].Username);
        Assert.Equal(users[2].Username, result[2].Username);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _connection?.Dispose();
    }
} 