using DotNetEnv;

namespace Common.Infrastructure.Configuration;

/// <summary>
/// Утилита для поиска корня проекта и загрузки .env файла
/// </summary>
public static class ProjectRootHelper
{
    /// <summary>
    /// Находит корень проекта по файлу NimbusSite.sln
    /// </summary>
    /// <param name="startDirectory">Начальная директория для поиска (по умолчанию - текущая)</param>
    /// <returns>Путь к корню проекта или null, если не найден</returns>
    public static string? FindProjectRoot(string? startDirectory = null)
    {
        var currentDir = startDirectory ?? Directory.GetCurrentDirectory();
        var solutionRoot = currentDir;
        
        while (!File.Exists(Path.Combine(solutionRoot, "NimbusSite.sln")) && 
               Directory.GetParent(solutionRoot) != null)
        {
            solutionRoot = Directory.GetParent(solutionRoot)!.FullName;
        }

        // Проверяем, что нашли корень (файл .sln существует)
        if (File.Exists(Path.Combine(solutionRoot, "NimbusSite.sln")))
        {
            return solutionRoot;
        }

        return null;
    }

    /// <summary>
    /// Загружает .env файл из корня проекта
    /// </summary>
    /// <param name="startDirectory">Начальная директория для поиска корня проекта</param>
    /// <returns>true, если .env файл был найден и загружен, иначе false</returns>
    public static bool LoadEnvFromProjectRoot(string? startDirectory = null)
    {
        var projectRoot = FindProjectRoot(startDirectory);
        if (projectRoot == null)
        {
            return false;
        }

        var envPath = Path.Combine(projectRoot, ".env");
        if (File.Exists(envPath))
        {
            try
            {
                Env.Load(envPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }
}

