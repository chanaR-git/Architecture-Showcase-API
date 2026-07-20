# REPOSITORY_RULES

- **Interface Pattern:** Base repository interfaces define async methods returning entities or tuples (e.g., Task<(IEnumerable<Gift> Items, int TotalCount)>).
- **Implementation Pattern:** Repository classes inherit from interface, inject ChineseSaleDbContext and ILogger in constructor.
- **Query Style:** Use EF Core queries with AsNoTracking() and Include() for navigation properties.
- **Mapping:** Repositories return domain entities; services map to DTOs.
- **Logging:** Use LoggingHelper.LogMethodStart, LogMethodWithCount, LogDatabaseError, and LogCreated for consistent logging.
