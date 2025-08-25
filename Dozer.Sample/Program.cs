using Dozer.Core.Attributes;
using Dozer.Core.Data;
using Microsoft.Data.Sqlite;

var connectionFactory = new DbConnectionFactory(
    "Data Source=mydb.db",
    SqliteFactory.Instance
);

// Create the table
using (var conn = connectionFactory.CreateConnection())
{
    conn.Open();
    using var cmd = connectionFactory.CreateCommand();
    cmd.Connection = conn;
    cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            UserName TEXT NOT NULL,
            Email TEXT
        )";
    cmd.ExecuteNonQuery();
}

using var db = new TinyDbContext(connectionFactory);

var user = new User { Username = "john", Email = "john@example.com" };

db.Insert(user);
Console.WriteLine($"Inserted user with ID: {user.Id}");

// FindById
var foundUser = db.FindById<User>(user.Id);
Console.WriteLine($"Found user: {foundUser?.Username} - {foundUser?.Email}");

user.Email = "new@example.com";
db.Update(user);

var users = db.List<User>();
Console.WriteLine($"Total users: {users.Count}");

//  fluent query API
Console.WriteLine("\n--- Testing Fluent Query API ---");

// Insert more users to test
var user2 = new User { Username = "jane", Email = "jane@example.com" };
var user3 = new User { Username = "bob", Email = "bob@example.com" };
db.Insert(user2);
db.Insert(user3);

// WHERE clause
var johnUsers = db.ExecuteQuery(db.Query<User>().WhereEquals("UserName", "john"));
Console.WriteLine($"Users named 'john': {johnUsers.Count}");

// LIKE
var usersWithJ = db.ExecuteQuery(db.Query<User>().WhereContains("UserName", "j"));
Console.WriteLine($"Users with 'j' in name: {usersWithJ.Count}");

// ORDER BY and LIMIT
var orderedUsers = db.ExecuteQuery(db.Query<User>().OrderBy("UserName", true).Limit(2));
Console.WriteLine($"First 2 users ordered by name: {orderedUsers.Count}");

// Complex query example
var complexQuery = db.Query<User>()
    .WhereContains("Email", "example")
    .OrderBy("UserName", false)
    .Limit(5);
var complexResults = db.ExecuteQuery(complexQuery);
Console.WriteLine($"Complex query results: {complexResults.Count}");

db.Delete(user);
db.Delete(user2);
db.Delete(user3);
Console.WriteLine("All test users deleted successfully");

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Column("UserName")]
    public string Username { get; set; }

    public string Email { get; set; }
}
