using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Reflection;
using System.Text;

namespace Common.Infrastructure.Tenancy;

/// <summary>
/// Вспомогательный класс для создания database views из embedded SQL скриптов
/// </summary>
public static class ViewInitializer
{
    /// <summary>
    /// Создать views из embedded SQL скриптов для указанной сборки
    /// </summary>
    public static async Task CreateViewsAsync(
        string connectionString,
        Assembly assembly,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            logger.LogWarning("Connection string is empty, cannot create views");
            return;
        }

        // Получаем все embedded SQL скрипты из папки Views/Migrations
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.Contains("Views") && name.Contains("Migrations") && name.EndsWith(".sql"))
            .OrderBy(name => name)
            .ToList();

        if (resourceNames.Count == 0)
        {
            logger.LogDebug("No view scripts found in assembly {AssemblyName}", assembly.GetName().Name);
            return;
        }

        logger.LogInformation("Found {Count} view scripts in assembly {AssemblyName}", resourceNames.Count, assembly.GetName().Name);

        // Логируем connection string (без пароля) для отладки
        try
        {
            var builder = new MySqlConnectionStringBuilder(connectionString);
            var maskedConnectionString = builder.ConnectionString.Replace(builder.Password ?? "", "***");
            logger.LogDebug("Using connection string: {ConnectionString} (database: {Database})", 
                maskedConnectionString, builder.Database);
        }
        catch
        {
            logger.LogDebug("Using connection string: (masked)");
        }

        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        
        // Проверяем текущую базу данных
        await using var dbCheckCommand = new MySqlCommand("SELECT DATABASE()", connection);
        var currentDatabase = await dbCheckCommand.ExecuteScalarAsync(cancellationToken) as string;
        logger.LogDebug("Current database: {Database}", currentDatabase);

        foreach (var resourceName in resourceNames)
        {
            try
            {
                logger.LogDebug("Loading view script: {ResourceName}", resourceName);

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    logger.LogWarning("Failed to load embedded resource: {ResourceName}", resourceName);
                    continue;
                }

                using var reader = new StreamReader(stream, Encoding.UTF8);
                var scriptContent = reader.ReadToEnd();

                // Пропускаем пустые файлы или файлы только с комментариями
                if (string.IsNullOrWhiteSpace(scriptContent) ||
                    (scriptContent.Trim().StartsWith("--") && !scriptContent.Contains("CREATE", StringComparison.OrdinalIgnoreCase)))
                {
                    logger.LogDebug("Skipping empty or comment-only script: {ResourceName}", resourceName);
                    continue;
                }

                // Удаляем комментарии и лишние пробелы, но сохраняем структуру
                var cleanScript = scriptContent
                    .Split('\n')
                    .Where(line => !line.Trim().StartsWith("--"))
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Aggregate((a, b) => a + " " + b)
                    .Trim();

                // Удаляем точку с запятой в конце, если есть
                if (cleanScript.EndsWith(";"))
                {
                    cleanScript = cleanScript.Substring(0, cleanScript.Length - 1).Trim();
                }

                if (string.IsNullOrWhiteSpace(cleanScript) || !cleanScript.Contains("CREATE", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogWarning("Script {ResourceName} does not contain CREATE statement, skipping", resourceName);
                    continue;
                }

                try
                {
                    logger.LogInformation("Executing view creation SQL from {ResourceName}: {SqlCommand}", 
                        resourceName, cleanScript.Substring(0, Math.Min(150, cleanScript.Length)));
                    
                    await using var command = new MySqlCommand(cleanScript, connection);
                    var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                    logger.LogInformation("View creation SQL executed successfully from {ResourceName}, rows affected: {RowsAffected}", 
                        resourceName, rowsAffected);
                }
                catch (MySqlConnector.MySqlException ex) when (
                    ex.ErrorCode == MySqlConnector.MySqlErrorCode.TableExists ||
                    ex.ErrorCode == MySqlConnector.MySqlErrorCode.DuplicateKeyName ||
                    ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    // View уже существует - это нормально при повторном запуске
                    logger.LogInformation("View already exists (script: {ResourceName}), skipping. Error: {Error}", resourceName, ex.Message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "CRITICAL: Failed to execute view creation SQL from {ResourceName}. Error: {Error}. SQL: {SqlCommand}", 
                        resourceName, ex.Message, cleanScript);
                    // НЕ продолжаем выполнение - это критическая ошибка!
                    throw;
                }

                // Извлекаем имя view из команды CREATE VIEW для логирования
                // Поддерживаем как с обратными кавычками, так и без них
                var viewNameMatch = System.Text.RegularExpressions.Regex.Match(
                    cleanScript,
                    @"CREATE\s+(?:OR\s+REPLACE\s+)?VIEW\s+(?:`)?(\w+)(?:`)?",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (viewNameMatch.Success)
                {
                    var viewName = viewNameMatch.Groups[1].Value;
                    logger.LogInformation("View '{ViewName}' created successfully from {ResourceName}",
                        viewName, resourceName);
                    
                    // Проверяем, что view действительно существует
                    try
                    {
                        await using var checkCommand = new MySqlCommand(
                            "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = DATABASE() AND table_name = @viewName",
                            connection);
                        checkCommand.Parameters.AddWithValue("@viewName", viewName);
                        var viewExists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync(cancellationToken)) > 0;
                        
                        if (viewExists)
                        {
                            logger.LogInformation("View '{ViewName}' verified to exist in database {Database}", viewName, currentDatabase);
                            
                            // Пробуем выполнить тестовый запрос к view, чтобы убедиться, что она работает
                            try
                            {
                                await using var testCommand = new MySqlCommand($"SELECT COUNT(*) FROM `{viewName}`", connection);
                                var rowCount = Convert.ToInt32(await testCommand.ExecuteScalarAsync(cancellationToken));
                                logger.LogInformation("View '{ViewName}' is accessible, contains {RowCount} rows", viewName, rowCount);
                            }
                            catch (Exception testEx)
                            {
                                logger.LogError(testEx, "View '{ViewName}' exists but is NOT accessible! Error: {Error}", viewName, testEx.Message);
                            }
                        }
                        else
                        {
                            logger.LogWarning("View '{ViewName}' was created but not found in database {Database}!", viewName, currentDatabase);
                        }
                    }
                    catch (Exception checkEx)
                    {
                        logger.LogWarning(checkEx, "Failed to verify view '{ViewName}' existence", viewName);
                    }
                }
                else
                {
                    logger.LogInformation("View script executed successfully: {ResourceName}", resourceName);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to process view script {ResourceName}", resourceName);
                // Продолжаем выполнение для других скриптов
            }
        }

        logger.LogInformation("Views creation process completed for assembly {AssemblyName}", assembly.GetName().Name);
    }
}

