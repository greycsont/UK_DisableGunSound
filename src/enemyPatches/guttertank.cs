using UnityEngine;
using HarmonyLib;


namespace DisableGunSound;

[HarmonyPatch(typeof(Guttertank), nameof(Guttertank.FireRocket))]
public static class GutterTankFireRocketPatch
{
    public static void Prefix(Guttertank __instance)
    {
        var volume = InstanceConfig.Volume;

        var aud = __instance.rocketParticle.GetComponent<AudioSource>();
        if (aud != null)
        {
            aud.volume = volume;
        }
    }
}