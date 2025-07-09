/*
 * RocketLauncher patches
 *
 * For rocket and cannonball, you can just access the audiosource
 * 
 */



using UnityEngine;
using HarmonyLib;

namespace DisableGunSound;

[HarmonyPatch(typeof(RocketLauncher), nameof(RocketLauncher.Shoot))]
public static class RocketLauncherShootPatch
{
    public static void Prefix(RocketLauncher __instance)
    {
        var volume = InstanceConfig.Volume;
        
        if (__instance.aud)
        {
            __instance.aud.volume = volume;
        }
    }
}

[HarmonyPatch(typeof(RocketLauncher), nameof(RocketLauncher.ShootCannonball))]
public static class RocketLauncherShootCannonballPatch
{
    public static void Prefix(RocketLauncher __instance)
    {
        var volume = InstanceConfig.Volume;
        if (__instance.aud)
        {
            __instance.aud.volume = volume;
        }
    }
}