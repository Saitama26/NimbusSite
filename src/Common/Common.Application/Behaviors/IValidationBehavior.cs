namespace Common.Application.Behaviors;

/// <summary>
/// Интерфейс для поведения валидации команд
/// Используется в pipeline обработки команд для автоматической валидации перед выполнением
/// </summary>
public interface IValidationBehavior
{
    // Интерфейс может быть использован для кастомных реализаций валидации
    // Базовая реализация будет в Common.Infrastructure с FluentValidation
}

