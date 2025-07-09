/*
 * v2 Patches
 *
 * I feel like I need to access all IEnemyWeapon
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

        var aud = __instance.altFlash.GetComponent<AudioSource>();
        if (aud)
        {
            aud.volume = volume;
        }
    }
}