namespace Identity.Domain.Transcriptions;

/// <summary>
/// Короткие идентификаторы интеграционных событий Identity
/// </summary>
public static class EventTypes
{
    public const string UserLoggedIn = "Identity.UserLoggedIn";
    public const string UserLoggedOut = "Identity.UserLoggedOut";
    public const string TokenGenerated = "Identity.TokenGenerated";
    public const string TokenRefreshed = "Identity.TokenRefreshed";
    public const string PasswordChanged = "Identity.PasswordChanged";
}

