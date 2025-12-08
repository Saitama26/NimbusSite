using DotNetEnv;
using Microsoft.Extensions.Configuration;

namespace Common.Infrastructure.Configuration;

/// <summary>
/// Утилита для загрузки переменных из .env, с возможностью явно указать путь.
/// Вызывать в Program перед построением конфигурации или сразу после.
/// </summary>
public static class EnvLoader
{
    /// <summary>
    /// Загружает .env из указанного пути или из корня (".env"), если путь не задан.
    /// Если .env не найден, пробует .env.template. Без исключений, если файлы отсутствуют.
    /// </summary>
    public static void Load(string? envFilePath = null)
    {
        var pathsToTry = new List<string>();

        if (string.IsNullOrWhiteSpace(envFilePath))
        {
            pathsToTry.Add(".env");
            pathsToTry.Add(".env.template");
        }
        else
        {
            pathsToTry.Add(envFilePath);
        }

        foreach (var path in pathsToTry)
        {
            try
            {
                Env.Load(path);
            }
            catch
            {
                // игнорируем, пробуем следующий
            }
        }

    }
}

