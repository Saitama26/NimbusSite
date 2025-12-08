namespace Common.Application.Behaviors;

/// <summary>
/// Интерфейс для поведения логирования команд/запросов
/// Используется в pipeline обработки команд для автоматического логирования
/// </summary>
public interface ILoggingBehavior
{
    // Интерфейс может быть использован для кастомных реализаций логирования
    // Базовая реализация будет в Common.Infrastructure
}

