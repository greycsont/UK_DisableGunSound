using PluginConfig.API;
using PluginConfig.API.Functionals;

namespace DisableGunSound;

public static class pluginConfiguratorEntry
{
    private static PluginConfigurator _config;
    public static void Build()
    {
        _config = PluginConfigurator.Create(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_GUID);
        _config.SetIconWithURL(PathManager.GetCurrentPluginPath("icon.png"));
    }
}