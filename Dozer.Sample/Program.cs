using Dozer.Core.Attributes;
using Dozer.Core.Data;
using Dozer.Sample.Models;
using Microsoft.Data.Sqlite;

var connectionFactory = new DbConnectionFactory(
    "Data Source=mydb.db",
    SqliteFactory.Instance
);

using var db = new TinyDbContext(connectionFactory);

// using schema generation
Console.WriteLine("--- Schema Generation Demo ---");
db.EnsureTableExists<User>();
Console.WriteLine("Users table created/verified");

db.EnsureTableExists<Product>();
Console.WriteLine("Products table created/verified");

Console.WriteLine($"Users table exists: {db.TableExists<User>()}");
Console.WriteLine($"Products table exists: {db.TableExists<Product>()}");

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

// Test Product entity and transactions
Console.WriteLine("\n--- Product Entity & Transaction Demo ---");

var product1 = new Product 
{ 
    Name = "Laptop", 
    Description = "High-performance laptop", 
    Price = 999.99m, 
    IsActive = true, 
    CreatedDate = DateTime.Now,
    CategoryId = 1
};

var product2 = new Product 
{ 
    Name = "Mouse", 
    Description = "Wireless mouse", 
    Price = 29.99m, 
    IsActive = true, 
    CreatedDate = DateTime.Now,
    CategoryId = 2
};

// transaction support
var transaction = db.BeginTransaction();
try
{
    db.Insert(product1);
    db.Insert(product2);
    
    // Query products
    var allProducts = db.List<Product>();
    Console.WriteLine($"Total products: {allProducts.Count}");
    
    var expensiveProducts = db.ExecuteQuery(db.Query<Product>().WhereGreaterThan("Price", 500));
    Console.WriteLine($"Expensive products (>$500): {expensiveProducts.Count}");
    
    db.CommitTransaction(transaction);
    Console.WriteLine("Transaction committed successfully");
}
catch (Exception ex)
{
    db.RollbackTransaction(transaction);
    Console.WriteLine($"Transaction rolled back: {ex.Message}");
}

db.Delete(product1);
db.Delete(product2);
Console.WriteLine("Products cleaned up");

// async operations
Console.WriteLine("\n--- Async Operations Demo ---");
await User.TestAsyncOperations(connectionFactory);

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Column("UserName")]
    public string Username { get; set; }

    public string Email { get; set; }
    
    public static async Task TestAsyncOperations(DbConnectionFactory connectionFactory)
{
    using var asyncDb = new AsyncTinyDbContext(connectionFactory);
    
    // Ensure table exists
    // NB: AsyncTinyDbContext doesn't have EnsureTableExists yet, so we use sync version -> i'll add it later
    using var syncDb = new TinyDbContext(connectionFactory);
    syncDb.EnsureTableExists<User>();
    
    var asyncUser = new User { Username = "async_user", Email = "async@example.com" };
    
    // Async insert
    await asyncDb.InsertAsync(asyncUser);
    Console.WriteLine($"Async inserted user with ID: {asyncUser.Id}");
    
    // Async find by ID
    var foundAsyncUser = await asyncDb.FindByIdAsync<User>(asyncUser.Id);
    Console.WriteLine($"Async found user: {foundAsyncUser?.Username}");
    
    // Async list
    var allAsyncUsers = await asyncDb.ListAsync<User>();
    Console.WriteLine($"Async total users: {allAsyncUsers.Count}");
    
    // Async query
    var asyncQuery = asyncDb.Query<User>().WhereEquals("UserName", "async_user");
    var asyncResults = await asyncDb.ExecuteQueryAsync(asyncQuery);
    Console.WriteLine($"Async query results: {asyncResults.Count}");
    
    await asyncDb.DeleteAsync(asyncUser);
    Console.WriteLine("Async user cleaned up");
}
}
