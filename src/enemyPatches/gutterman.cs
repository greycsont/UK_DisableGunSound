using UnityEngine;
using HarmonyLib;

namespace DisableGunSound;

[HarmonyPatch(typeof(Gutterman), nameof(Gutterman.FixedUpdate))]
public static class GuttermanShootPatch
{
    public static void Prefix(Gutterman __instance)
    {
        var volume = InstanceConfig.Volume;

        var aud = __instance.beam.GetComponent<AudioSource>();
        if (aud != null)
        {
            aud.volume = volume;
        }
    }
}