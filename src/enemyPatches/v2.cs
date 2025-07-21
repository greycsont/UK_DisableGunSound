/*
 * v2 Patches
 *
 * For __instance one.... It's quite complex
 * First, EnemyNailgun and EnemyRevolver uses themselves
 * The EnemyShotgun reuses the class shotgun
 *
 * Shit
 *
 */

using UnityEngine;
using HarmonyLib;

namespace DisableGunSound;


[HarmonyPatch(typeof(EnemyNailgun), nameof(EnemyNailgun.Fire))]
public static class EnemyNailgunFirePatch
{
    public static void Prefix(EnemyNailgun __instance)
    {
        var volume = InstanceConfig.Volume;

        var aud = __instance.muzzleFlash.GetComponent<AudioSource>();
        if (aud)
        {
            aud.volume = volume;
        }
    }
}

[HarmonyPatch(typeof(EnemyNailgun), nameof(EnemyNailgun.AltFire))]
public static class EnemyNailgunAltFirePatch
{
    public static void Prefix(EnemyNailgun __instance)
    {
        var volume = InstanceConfig.Volume;

        var aud = __instance.muzzleFlash.GetComponent<AudioSource>();
        if (aud)
        {
            aud.volume = volume;
        }
    }
}

[HarmonyPatch(typeof(EnemyRevolver), nameof(EnemyRevolver.Fire))]
public static class EnemyRevolverFirePatch
{
    public static void Prefix(EnemyRevolver __instance)
    {
        var volume = InstanceConfig.Volume;

        var aud = __instance.muzzleFlash.GetComponent<AudioSource>();
        if (aud)
        {
            aud.volume = volume;
        }
    }
}

[HarmonyPatch(typeof(EnemyRevolver), nameof(EnemyRevolver.AltFire))]
public static class EnemyRevolverAltFirePatch
{
    public static void Prefix(EnemyRevolver __instance)
    {
        var volume = InstanceConfig.Volume;

        var aud = __instance.muzzleFlashAlt.GetComponent<AudioSource>();
        if (aud)
        {
            aud.volume = volume;
        }
    }
}

[HarmonyPatch(typeof(EnemyShotgun), nameof(EnemyShotgun.Fire))]
public static class EnemyShotgunFirePatch
{
    public static bool Prefix(EnemyShotgun __instance)
    {
        var volume = InstanceConfig.Volume;

        if (__instance.target == null)
        {
            return false;
        }
        __instance.gunReady = false;
        int num = 12;
        __instance.anim.SetTrigger("Shoot");
        Vector3 position = __instance.shootPoint.position;
        if (Vector3.Distance(__instance.transform.position, __instance.eid.transform.position) > Vector3.Distance(__instance.target.position, __instance.eid.transform.position))
        {
            position = new Vector3(__instance.eid.transform.position.x, __instance.transform.position.y, __instance.eid.transform.position.z);
        }
        GameObject gameObject = new GameObject();
        gameObject.AddComponent<ProjectileSpread>();
        gameObject.transform.position = __instance.transform.position;
        for (int i = 0; i < num; i++)
        {
            GameObject gameObject2;
            if (i == 0)
            {
                gameObject2 = Object.Instantiate<GameObject>(__instance.bullet, position, __instance.shootPoint.rotation, gameObject.transform);
            }
            else
            {
                Quaternion rotation = __instance.shootPoint.rotation * Quaternion.Euler(Random.Range(-__instance.spread, __instance.spread), Random.Range(-__instance.spread, __instance.spread), Random.Range(-__instance.spread, __instance.spread));
                gameObject2 = Object.Instantiate<GameObject>(__instance.bullet, position, rotation, gameObject.transform);
            }
            Projectile projectile;
            if (gameObject2.TryGetComponent<Projectile>(out projectile))
            {
                projectile.target = __instance.target;
                projectile.safeEnemyType = __instance.safeEnemyType;
                if (__instance.difficulty == 1)
                {
                    projectile.speed *= 0.75f;
                }
                else if (__instance.difficulty == 0)
                {
                    projectile.speed *= 0.5f;
                }
                projectile.damage *= __instance.damageMultiplier;
                projectile.spreaded = true;
            }
        }
        __instance.gunAud.clip = __instance.shootSound;
        __instance.gunAud.volume = volume;
        __instance.gunAud.panStereo = 0f;
        __instance.gunAud.pitch = Random.Range(0.95f, 1.05f);
        __instance.gunAud.Play();
        Object.Instantiate<GameObject>(__instance.muzzleFlash, __instance.shootPoint.position, __instance.shootPoint.rotation);

        return false;
    }
}

[HarmonyPatch(typeof(EnemyShotgun), nameof(EnemyShotgun.AltFire))]
public static class EnemyShotgunAltFirePatch
{
    public static bool Prefix(EnemyShotgun __instance)
    {
        var volume = InstanceConfig.Volume;

        if (__instance.target == null)
        {
            __instance.CancelAltCharge();
            return false;
        }
        __instance.gunReady = false;
        float d = 70f;
        if (__instance.difficulty == 1)
        {
            d = 50f;
        }
        else if (__instance.difficulty == 0)
        {
            d = 30f;
        }
        if (__instance.shootPoint == null)
        {
            return false;
        }
        Vector3 position = __instance.shootPoint.position;
        if (Vector3.Distance(__instance.transform.position, __instance.eid.transform.position) > Vector3.Distance(__instance.target.position, __instance.eid.transform.position))
        {
            position = new Vector3(__instance.eid.transform.position.x, __instance.transform.position.y, __instance.eid.transform.position.z);
        }
        GameObject gameObject = Object.Instantiate<GameObject>(__instance.grenade, position, Random.rotation);
        gameObject.GetComponent<Rigidbody>().AddForce(__instance.shootPoint.forward * d, ForceMode.VelocityChange);
        Grenade componentInChildren = gameObject.GetComponentInChildren<Grenade>();
        if (componentInChildren != null)
        {
            componentInChildren.enemy = true;
        }
        __instance.anim.SetTrigger("Secondary Fire");
        __instance.gunAud.clip = __instance.shootSound;
        __instance.gunAud.volume = volume;
        __instance.gunAud.panStereo = 0f;
        __instance.gunAud.pitch = Random.Range(0.75f, 0.85f);
        __instance.gunAud.Play();
        Object.Instantiate<GameObject>(__instance.muzzleFlash, __instance.shootPoint.position, __instance.shootPoint.rotation);
        __instance.CancelAltCharge();

        return false;
    }
}