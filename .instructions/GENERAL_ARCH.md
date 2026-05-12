# GENERAL_ARCH

- **Naming:** PascalCase for class names, method names, and properties. camelCase for local variables and method parameters.
- **Interfaces:** Prefix interface names with 'I' (e.g., IGiftRepository).
- **Dependency Injection:** Use AddScoped for repositories and services. Use AddSingleton for shared services like ITokenService and IRedisCacheService.
- **Error Handling:** Use try-catch in repositories with LoggingHelper for logging, then re-throw. Use ExceptionHandlingMiddleware for global exception handling.
- **Logging:** Use Serilog with structured logging via LoggingHelper utility.
- **Reference:** Refer to REPOSITORY_RULES.md for repository layer patterns.
- **Reference:** Refer to CONTROLLER_RULES.md for controller layer patterns.
- **Mapping:** Business logic and DTO mapping must reside in the Service layer, never in Controllers or Repositories.
