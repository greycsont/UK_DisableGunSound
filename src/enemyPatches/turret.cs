
using UnityEngine;
using HarmonyLib;


namespace DisableGunSound;


[HarmonyPatch(typeof(Turret), nameof(Turret.Shoot))]
public static class TurretShootPatch
{
    public static void Prefix(Turret __instance)
    {
        var volume = InstanceConfig.Volume;

        var aud = __instance.beam.GetComponent<AudioSource>();
        if (aud != null)
        {
            aud.volume = volume;
        }
    }
}

