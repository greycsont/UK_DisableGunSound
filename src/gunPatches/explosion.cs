
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(Explosion), nameof(Explosion.Start))]
public static class ExplosionStartPatch
{
    public static void Prefix()
    {
        
    }
}