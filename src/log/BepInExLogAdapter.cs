using BepInEx.Logging;

namespace DisableGunSound;

public class BepInExLogAdapter : ILog
{
    private readonly ManualLogSource log;

    public BepInExLogAdapter(ManualLogSource log)
    {
        this.log = log;
    }

    public void LogInfo(object data) => log.LogInfo(data);
    public void LogWarning(object data) => log.LogWarning(data);
    public void LogError(object data) => log.LogError(data);
    public void LogDebug(object data) => log.LogDebug(data);
}
