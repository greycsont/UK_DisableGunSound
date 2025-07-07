using UnityEngine;
using HarmonyLib;

namespace DisableGunSound;

[HarmonyPatch(typeof(Revolver), nameof(Revolver.Shoot))]
public static class RevolverPatch
{
    // This includes both variants
    public static bool Prefix(ref int shotType, Revolver __instance)
    {
        var volume = InstanceConfig.Volume;

        __instance.cc.StopShake();
        __instance.shootReady = false;
        __instance.shootCharge = 0f;
        if (__instance.altVersion)
        {
            MonoSingleton<WeaponCharges>.Instance.revaltpickupcharges[__instance.gunVariation] = 2f;
        }
        if (shotType == 1)
        {
            GameObject gameObject = Object.Instantiate<GameObject>(__instance.revolverBeam, __instance.cc.transform.position, __instance.cc.transform.rotation);
            if (__instance.targeter.CurrentTarget && __instance.targeter.IsAutoAimed)
            {
                gameObject.transform.LookAt(__instance.targeter.CurrentTarget.bounds.center);
            }
            RevolverBeam component = gameObject.GetComponent<RevolverBeam>();
            component.sourceWeapon = __instance.gc.currentWeapon;
            component.alternateStartPoint = __instance.gunBarrel.transform.position;
            component.gunVariation = __instance.gunVariation;
            if (__instance.anim.GetCurrentAnimatorStateInfo(0).IsName("PickUp"))
            {
                component.quickDraw = true;
            }
            __instance.currentGunShot = Random.Range(0, __instance.gunShots.Length);
            __instance.gunAud.clip = __instance.gunShots[__instance.currentGunShot];

            __instance.gunAud.volume = volume;

            __instance.gunAud.pitch = Random.Range(0.9f, 1.1f);
            __instance.gunAud.Play();
            __instance.cam.fieldOfView = __instance.cam.fieldOfView + __instance.cc.defaultFov / 40f;
            MonoSingleton<RumbleManager>.Instance.SetVibrationTracked(RumbleProperties.GunFire, __instance.gameObject);
        }
        else if (shotType == 2)
        {
            GameObject gameObject2 = Object.Instantiate<GameObject>(__instance.revolverBeamSuper, __instance.cc.transform.position, __instance.cc.transform.rotation);
            if (__instance.targeter.CurrentTarget && __instance.targeter.IsAutoAimed)
            {
                gameObject2.transform.LookAt(__instance.targeter.CurrentTarget.bounds.center);
            }
            RevolverBeam component2 = gameObject2.GetComponent<RevolverBeam>();
            component2.sourceWeapon = __instance.gc.currentWeapon;
            component2.alternateStartPoint = __instance.gunBarrel.transform.position;
            component2.gunVariation = __instance.gunVariation;
            if (__instance.gunVariation == 2)
            {
                component2.ricochetAmount = Mathf.Min(3, Mathf.FloorToInt(__instance.pierceShotCharge / 25f));
            }
            __instance.pierceShotCharge = 0f;
            if (__instance.anim.GetCurrentAnimatorStateInfo(0).IsName("PickUp"))
            {
                component2.quickDraw = true;
            }
            __instance.pierceReady = false;
            __instance.pierceCharge = 0f;
            if (__instance.gunVariation == 0)
            {
                __instance.screenAud.clip = __instance.chargingSound;
                __instance.screenAud.loop = true;
                if (__instance.altVersion)
                {
                    __instance.screenAud.pitch = 0.5f;
                }
                else
                {
                    __instance.screenAud.pitch = 1f;
                }

                __instance.screenAud.volume = volume;

                __instance.screenAud.Play();
            }
            else if (!__instance.wid || __instance.wid.delay == 0f)
            {
                __instance.wc.rev2charge -= (float)(__instance.altVersion ? 300 : 100);
            }
            if (__instance.superGunSound)
            {
                AudioSource aud = Object.Instantiate<AudioSource>(__instance.superGunSound);
                aud.Stop();
                aud.volume = volume;
                aud.Play();
            }
            if (__instance.gunVariation == 2 && __instance.twirlShotSound)
            {
                GameObject redvarObject = Object.Instantiate<GameObject>(__instance.twirlShotSound, __instance.transform.position, Quaternion.identity);
                AudioSource aud = redvarObject.GetComponent<AudioSource>();
                aud.Stop();
                aud.volume = volume;
                aud.Play();
            }
            __instance.cam.fieldOfView = __instance.cam.fieldOfView + __instance.cc.defaultFov / 20f;
            MonoSingleton<RumbleManager>.Instance.SetVibrationTracked(RumbleProperties.GunFireStrong, __instance.gameObject);
        }
        if (!__instance.altVersion)
        {
            __instance.cylinder.DoTurn();
        }
        __instance.anim.SetFloat("RandomChance", Random.Range(0f, 1f));
        if (shotType == 1)
        {
            __instance.anim.SetTrigger("Shoot");
        }
        else
        {
            __instance.anim.SetTrigger("ChargeShoot");
        }
        __instance.gunReady = false;

        return false;
    }
}