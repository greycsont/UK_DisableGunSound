using UnityEngine;
using HarmonyLib;

namespace DisableGunSound;

[HarmonyPatch(typeof(Nailgun), nameof(Nailgun.SuperSaw))]
public static class NailgunSuperSawPatch
{
    public static void Prefix(Nailgun __instance)
    {
        var volume = InstanceConfig.Volume;

        var aud = __instance.muzzleFlash2.GetComponent<AudioSource>();
        if (aud != null)
        {
            aud.volume = volume;
        }
    }
}


[HarmonyPatch(typeof(Nailgun), nameof(Nailgun.Shoot))]
public static class NailgunPatch
{
    public static bool Prefix(Nailgun __instance)
    {
        var volume = InstanceConfig.Volume;

        __instance.UpdateAnimationWeight();
        __instance.fireCooldown = __instance.currentFireRate;
        __instance.shotSuccesfully = true;
        if (__instance.variation == 1 && (!__instance.wid || __instance.wid.delay == 0f))
        {
            if (__instance.altVersion)
            {
                __instance.wc.naiSaws -= 1f;
            }
            else
            {
                __instance.wc.naiAmmo -= 1f;
            }
        }
        __instance.anim.SetTrigger("Shoot");
        __instance.barrelNum++;
        if (__instance.barrelNum >= __instance.shootPoints.Length)
        {
            __instance.barrelNum = 0;
        }
        GameObject gameObject;
        if (__instance.burnOut)
        {
            gameObject = Object.Instantiate<GameObject>(__instance.muzzleFlash2, __instance.shootPoints[__instance.barrelNum].transform);
        }
        else
        {
            gameObject = Object.Instantiate<GameObject>(__instance.muzzleFlash, __instance.shootPoints[__instance.barrelNum].transform);
        }

        AudioSource component = gameObject.GetComponent<AudioSource>();
        component.Stop();
        component.volume = volume;
        component.Play();

        if (!__instance.altVersion)
        {
            if (__instance.burnOut)
            {
                __instance.currentSpread = __instance.spread * 2f;
            }
            else
            {
                __instance.currentSpread = __instance.spread;
            }
        }
        else if (__instance.burnOut)
        {
            __instance.currentSpread = 45f;
        }
        else if (__instance.altVersion && __instance.variation == 0)
        {
            if (__instance.heatSinks < 1f)
            {
                __instance.currentSpread = 45f;
            }
            else
            {
                __instance.currentSpread = Mathf.Lerp(0f, 45f, Mathf.Max(0f, __instance.heatUp - 0.25f));
            }
        }
        else
        {
            __instance.currentSpread = 0f;
        }
        GameObject gameObject2;
        if (__instance.burnOut)
        {
            gameObject2 = Object.Instantiate<GameObject>(__instance.heatedNail, __instance.cc.transform.position + __instance.cc.transform.forward, __instance.transform.rotation);
        }
        else
        {
            gameObject2 = Object.Instantiate<GameObject>(__instance.nail, __instance.cc.transform.position + __instance.cc.transform.forward, __instance.transform.rotation);
        }
        if (__instance.altVersion && __instance.variation == 0 && __instance.heatSinks >= 1f)
        {
            __instance.heatUp = Mathf.MoveTowards(__instance.heatUp, 1f, 0.125f);
        }
        gameObject2.transform.forward = __instance.cc.transform.forward;
        if (Physics.Raycast(__instance.cc.transform.position, __instance.cc.transform.forward, 1f, LayerMaskDefaults.Get(LMD.Environment)))
        {
            gameObject2.transform.position = __instance.cc.transform.position;
        }
        if (__instance.targeter.CurrentTarget && __instance.targeter.IsAutoAimed)
        {
            gameObject2.transform.position = __instance.cc.transform.position + (__instance.targeter.CurrentTarget.bounds.center - __instance.cc.transform.position).normalized;
            gameObject2.transform.LookAt(__instance.targeter.CurrentTarget.bounds.center);
        }
        gameObject2.transform.Rotate(Random.Range(-__instance.currentSpread / 3f, __instance.currentSpread / 3f), Random.Range(-__instance.currentSpread / 3f, __instance.currentSpread / 3f), Random.Range(-__instance.currentSpread / 3f, __instance.currentSpread / 3f));
        Rigidbody rigidbody;
        if (gameObject2.TryGetComponent<Rigidbody>(out rigidbody))
        {
            rigidbody.velocity = gameObject2.transform.forward * 200f;
        }
        Nail nail;
        if (gameObject2.TryGetComponent<Nail>(out nail))
        {
            nail.sourceWeapon = __instance.gc.currentWeapon;
            nail.weaponType = __instance.projectileVariationTypes[__instance.variation];
            if (__instance.altVersion && __instance.variation != 1)
            {
                if (__instance.heatSinks >= 1f && __instance.variation != 2)
                {
                    nail.hitAmount = Mathf.Lerp(3f, 1f, __instance.heatUp);
                }
                else
                {
                    nail.hitAmount = 1f;
                }
            }
            if (nail.sawblade)
            {
                nail.ForceCheckSawbladeRicochet();
            }
        }
        if (!__instance.burnOut)
        {
            __instance.cc.CameraShake(0.1f);
        }
        else
        {
            __instance.cc.CameraShake(0.35f);
        }
        if (__instance.altVersion)
        {
            MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.Sawblade);
        }

        return false;
    }
}