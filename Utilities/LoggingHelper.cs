namespace Chinese_sale_api.Utilities
{
    /// <summary>
    /// Helper class for consistent logging across repositories and services.
    /// Provides methods for common logging patterns while keeping code clean.
    /// </summary>
    public static class LoggingHelper
    {
        /// <summary>
        /// Logs the start of a method execution
        /// </summary>
        public static void LogMethodStart(ILogger logger, string methodName, string className, object? parameters = null)
        {
            if (parameters != null)
                logger.LogInformation("[{Class}.{Method}] Started with parameters: {@Parameters}", className, methodName, parameters);
            else
                logger.LogInformation("[{Class}.{Method}] Started", className, methodName);
        }

        /// <summary>
        /// Logs successful method completion with result
        /// </summary>
        public static void LogMethodSuccess(ILogger logger, string methodName, string className, object? result = null)
        {
            if (result != null)
                logger.LogInformation("[{Class}.{Method}] Completed successfully. Result: {@Result}", className, methodName, result);
            else
                logger.LogInformation("[{Class}.{Method}] Completed successfully", className, methodName);
        }

        /// <summary>
        /// Logs method execution with item count
        /// </summary>
        public static void LogMethodWithCount(ILogger logger, string methodName, string className, int count)
        {
            logger.LogInformation("[{Class}.{Method}] Completed. Found {Count} item(s)", className, methodName, count);
        }

        /// <summary>
        /// Logs when data is not found
        /// </summary>
        public static void LogNotFound(ILogger logger, string methodName, string className, string identifier)
        {
            logger.LogWarning("[{Class}.{Method}] Not found: {Identifier}", className, methodName, identifier);
        }

        /// <summary>
        /// Logs validation errors
        /// </summary>
        public static void LogValidationError(ILogger logger, string methodName, string className, string errorMessage)
        {
            logger.LogWarning("[{Class}.{Method}] Validation error: {Error}", className, methodName, errorMessage);
        }

        /// <summary>
        /// Logs when attempting to create a duplicate
        /// </summary>
        public static void LogDuplicateAttempt(ILogger logger, string methodName, string className, string itemName)
        {
            logger.LogWarning("[{Class}.{Method}] Duplicate attempt: {Item}", className, methodName, itemName);
        }

        /// <summary>
        /// Logs database operation errors
        /// </summary>
        public static void LogDatabaseError(ILogger logger, string methodName, string className, Exception ex)
        {
            logger.LogError(ex, "[{Class}.{Method}] Database error occurred", className, methodName);
        }

        /// <summary>
        /// Logs unexpected errors
        /// </summary>
        public static void LogUnexpectedError(ILogger logger, string methodName, string className, Exception ex)
        {
            logger.LogError(ex, "[{Class}.{Method}] Unexpected error", className, methodName);
        }

        /// <summary>
        /// Logs when data is successfully created
        /// </summary>
        public static void LogCreated(ILogger logger, string methodName, string className, object item)
        {
            logger.LogInformation("[{Class}.{Method}] Created: {@Item}", className, methodName, item);
        }

        /// <summary>
        /// Logs when data is successfully updated
        /// </summary>
        public static void LogUpdated(ILogger logger, string methodName, string className, object item)
        {
            logger.LogInformation("[{Class}.{Method}] Updated: {@Item}", className, methodName, item);
        }

        /// <summary>
        /// Logs when data is successfully deleted
        /// </summary>
        public static void LogDeleted(ILogger logger, string methodName, string className, string identifier)
        {
            logger.LogInformation("[{Class}.{Method}] Deleted: {Identifier}", className, methodName, identifier);
        }
    }
}
