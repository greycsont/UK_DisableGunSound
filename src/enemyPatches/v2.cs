/*
 * v2 Patches
 *
 * For this one.... It's quite complex
 * First, EnemyNailgun and EnemyRevolver uses themselves
 * The EnemyShotgun reuses the class shotgun
 *
 * Shit
 *
 */

using UnityEngine;
using HarmonyLib;

namespace DisableGunSound;


[HarmonyPatch(typeof(V2), nameof(V2.ShootWeapon))]
public static class V2ShootWeaponPatch
{
    public static void Prefix(V2 __instance)
    {
        var volume = InstanceConfig.Volume;

        Component[] components = __instance.weapons[__instance.currentWeapon].GetComponents<Component>();
        foreach (var comp in components)
        {
            LogManager.LogInfo("Component: " + comp.GetType().Name);
        }

        var aud = __instance.weapons[__instance.currentWeapon].GetComponent<AudioSource>();
        if (aud)
        {
            LogManager.LogInfo("Changed audiosource volume");
            aud.volume = volume;
        }


        /*var aud = __instance.altFlash.GetComponent<AudioSource>();
        if (aud)
        {
            aud.volume = volume;
        }*/
    }
}