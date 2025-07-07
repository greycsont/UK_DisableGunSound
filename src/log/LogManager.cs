
public static class LogManager
{
    public static ILog log { get; set; }
    public static void LogInfo(object data)    => log?.LogInfo(data);
    public static void LogWarning(object data) => log?.LogWarning(data);
    public static void LogError(object data)   => log?.LogError(data);
    public static void LogDebug(object data)   => log?.LogDebug(data);
}