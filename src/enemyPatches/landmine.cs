using HarmonyLib;

namespace DisableGunSound;

[HarmonyPatch(typeof(Landmine), nameof(Landmine.Activate))]
public static class LandMineActivatePatch
{
    public static void Prefix(Landmine __instance)
    {
        var volume = InstanceConfig.Volume;

        __instance.aud.volume = volume;
    }
}