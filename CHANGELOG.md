# Changelog

All notable changes to Dozer ORM will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-07

### Added
- **Core ORM Features**
  - Basic CRUD operations (Insert, Update, Delete, List, FindById)
  - Custom attributes ([Table], [Column], [Key])
  - Reflection-based entity mapping
  - ADO.NET integration with database abstraction
  
- **Fluent Query API**
  - Chainable query builder
  - WHERE, ORDER BY, LIMIT, OFFSET support
  - Parameterized queries for SQL injection prevention
  - Convenience methods (WhereEquals, WhereGreaterThan, WhereLessThan, WhereContains)
  - Count and FirstOrDefault query methods
  
- **Advanced Features**
  - Transaction support (BeginTransaction, CommitTransaction, RollbackTransaction)
  - Schema generation (EnsureTableExists, TableExists, DropTable)
  - Async operations (AsyncTinyDbContext with async/await support)
  
- **Change Tracking System**
  - Entity state tracking (Added, Modified, Deleted, Unchanged)
  - Smart updates that only modify changed properties
  - Original value comparison
  - Manual and automatic change tracking
  
- **Identity Map**
  - Per-session entity cache
  - Referential integrity within context
  - Prevents duplicate entity instances
  - Cache management (IsInCache, GetFromCache, ClearCache, GetCacheCount)
  
- **Quality & Documentation**
  - XML documentation for all public APIs
  - Custom exception classes (DozerException, EntityValidationException, EntityMappingException, DatabaseException)
  - NuGet package configuration
  - Comprehensive usage guide
  
### Dependencies
- .NET 8.0

### Known Limitations
- No LINQ expression tree support (use fluent API)
- No navigation properties or relationships
- No migration system
- Single database connection per context
- SQLite-specific SQL generation (can be extended)

### Coming Soon
- LINQ expression tree support
- Migration system
- Navigation properties
- Multi-database support
- Bulk operations

---

## Future Releases

### [Unreleased]
- LINQ expression tree support for queries
- Database migration system
- Navigation properties and relationships
- Connection pooling
- Query caching
- Bulk insert/update/delete operations

