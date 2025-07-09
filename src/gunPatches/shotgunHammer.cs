/* ShotgunHammer patches
 * 
 * It can be directly replaces the audio by access the audiosource
 * 
 */


using UnityEngine;
using HarmonyLib;
using System.Collections;

namespace DisableGunSound;




[HarmonyPatch(typeof(ShotgunHammer), nameof(ShotgunHammer.ShootSaw))]
public static class ShotgunHammerShootSawPatch
{
    public static void Prefix(ShotgunHammer __instance)
    {
        var volume = InstanceConfig.Volume;

        if (__instance.nadeSpawnSound)
        {
            __instance.nadeSpawnSound.volume = volume;
        }
    }
}


[HarmonyPatch(typeof(ShotgunHammer), nameof(ShotgunHammer.ThrowNade))]
public static class ShotgunHammerThrowNadePatch
{
    public static void Prefix(ShotgunHammer __instance)
    {
        var volume = InstanceConfig.Volume;

        if (__instance.nadeSpawnSound)
        {
            __instance.nadeSpawnSound.volume = volume;
        }
    }
}


[HarmonyPatch(typeof(ShotgunHammer), nameof(ShotgunHammer.ImpactEffects))]
public static class ShotgunHammerImpactEffectsPatch
{
    public static void Prefix(ShotgunHammer __instance)
    {
        var volume = InstanceConfig.Volume;

        var hitsoundaud = __instance.hitImpactParticle[__instance.forceWeakHit ? 0 : __instance.tier].GetComponent<AudioSource>();

        if (hitsoundaud)
        {
            hitsoundaud.volume = volume;
        }
    }
}


[HarmonyPatch(typeof(ShotgunHammer), nameof(ShotgunHammer.Impact))]
public static class ShotgunHammerImpactPatch
{
    public static void Prefix(ShotgunHammer __instance)
    {
        var volume = InstanceConfig.Volume;
        if (__instance.hitSound)
        {
            __instance.hitSound.volume = volume;
        }
    }
}