namespace ArchetypeCSharpCLI.Configuration;

/// <summary>
/// Cached access to the current application settings.
/// </summary>
public static class AppSettings
{
    private static AppConfig? _current;
    private static readonly object _lock = new();

    public static AppConfig Current
    {
        get
        {
            if (_current is not null) return _current;
            lock (_lock)
            {
                _current ??= ConfigBuilder.Build();
            }
            return _current;
        }
    }
}
