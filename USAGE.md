# Dozer ORM - Usage Guide

This guide provides detailed examples of how to use the Dozer ORM library.

## Table of Contents

1. [Entity Definition](#entity-definition)
2. [Basic CRUD Operations](#basic-crud-operations)
3. [Fluent Query API](#fluent-query-api)
4. [Transactions](#transactions)
5. [Async Operations](#async-operations)
6. [Schema Generation](#schema-generation)

## Entity Definition

Define your entities using custom attributes:

```csharp
[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Column("UserName")]
    public string Username { get; set; }

    public string Email { get; set; }
}

[Table("Products")]
public class Product
{
    [Key]
    public int Id { get; set; }

    [Column("ProductName", "TEXT")]
    public string Name { get; set; }

    [Column("Price", "DECIMAL(10,2)")]
    public decimal Price { get; set; }

    [Column("IsActive", "BOOLEAN")]
    public bool IsActive { get; set; }

    [Column("CreatedDate", "DATETIME")]
    public DateTime CreatedDate { get; set; }
}
```

### Attributes

- `[Table("TableName")]`: Specifies the database table name
- `[Key]`: Marks the primary key property (auto-increment by default)
- `[Column("ColumnName", "DbType")]`: Maps property to specific column name and type

## Basic CRUD Operations

### Setup

```csharp
var connectionFactory = new DbConnectionFactory(
    "Data Source=mydb.db",
    SqliteFactory.Instance
);

using var db = new TinyDbContext(connectionFactory);
```

### Create Table

```csharp
db.EnsureTableExists<User>();
db.EnsureTableExists<Product>();
```

### Insert

```csharp
var user = new User { Username = "john", Email = "john@example.com" };
db.Insert(user);
Console.WriteLine($"Inserted user with ID: {user.Id}"); // it will set an auto increment id
```

### Read

```csharp
// Find by ID
var foundUser = db.FindById<User>(user.Id);

// List all
var allUsers = db.List<User>();
```

### Update

```csharp
user.Email = "new@example.com";
db.Update(user);
```

### Delete

```csharp
db.Delete(user);
```

## Fluent Query API

### Basic WHERE Clauses

```csharp
// Equals
var johnUsers = db.ExecuteQuery(db.Query<User>().WhereEquals("UserName", "john"));

// Greater than
var expensiveProducts = db.ExecuteQuery(db.Query<Product>().WhereGreaterThan("Price", 500));

// Less than
var cheapProducts = db.ExecuteQuery(db.Query<Product>().WhereLessThan("Price", 100));

// Contains (LIKE)
var usersWithJ = db.ExecuteQuery(db.Query<User>().WhereContains("UserName", "j"));
```

### Ordering

```csharp
// Ascending
var orderedUsers = db.ExecuteQuery(db.Query<User>().OrderBy("UserName", true));

// Descending
var reverseOrderedUsers = db.ExecuteQuery(db.Query<User>().OrderBy("UserName", false));
```

### Pagination

```csharp
// Limit results
var first5Users = db.ExecuteQuery(db.Query<User>().Limit(5));

// Offset
var skipFirst10 = db.ExecuteQuery(db.Query<User>().Offset(10).Limit(5));
```

### Complex Queries

```csharp
var complexQuery = db.Query<User>()
    .WhereContains("Email", "example")
    .OrderBy("UserName", false)
    .Limit(5);
var results = db.ExecuteQuery(complexQuery);
```

### Count Queries

```csharp
var count = db.Count(db.Query<User>().WhereContains("Email", "example"));
```

## Transactions

```csharp
var transaction = db.BeginTransaction();
try
{
    db.Insert(user1);
    db.Insert(user2);
    db.Insert(product1);
    
    db.CommitTransaction(transaction);
    Console.WriteLine("All operations committed successfully");
}
catch (Exception ex)
{
    db.RollbackTransaction(transaction);
    Console.WriteLine($"Transaction rolled back: {ex.Message}");
}
```

## Async Operations

Use `AsyncTinyDbContext` for async operations:

```csharp
using var asyncDb = new AsyncTinyDbContext(connectionFactory);

// Async insert
await asyncDb.InsertAsync(user);

// Async read
var users = await asyncDb.ListAsync<User>();
var foundUser = await asyncDb.FindByIdAsync<User>(1);

// Async query
var asyncQuery = asyncDb.Query<User>().WhereEquals("UserName", "john");
var results = await asyncDb.ExecuteQueryAsync(asyncQuery);
```

## Schema Generation

### Check if Table Exists

```csharp
if (db.TableExists<User>())
{
    Console.WriteLine("Users table exists");
}
```

### Drop Table

```csharp
db.DropTable<User>();
```

### Generated SQL Examples

The ORM generates SQL like this:

**CREATE TABLE:**
```sql
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserName TEXT NOT NULL,
    Email TEXT
);
```

**INSERT:**
```sql
INSERT INTO Users (UserName, Email) VALUES (@UserName, @Email)
```

**SELECT with WHERE:**
```sql
SELECT Id, UserName, Email FROM Users WHERE UserName = @param0
```

**UPDATE:**
```sql
UPDATE Users SET UserName = @UserName, Email = @Email WHERE Id = @Id
```

**DELETE:**
```sql
DELETE FROM Users WHERE Id = @Id
```

For more examples, see the `Dozer.Sample` project in the repository.
