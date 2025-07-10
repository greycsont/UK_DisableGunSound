using HarmonyLib;
using Unity;
using UnityEngine;

namespace DisableGunSound;

[HarmonyPatch(typeof(TimeController), nameof(TimeController.ParryFlash))]
public static class ParryFlashPatch
{
    public static void Prefix(TimeController __instance)
    {
        var volume = InstanceConfig.Volume;
        var aud = __instance.parryLight.GetComponentsInChildren<AudioSource>();
        foreach (var audiosource in aud)
        {
            audiosource.volume = volume;
        }
    }
}