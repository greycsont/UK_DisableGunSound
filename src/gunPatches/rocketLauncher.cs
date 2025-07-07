using UnityEngine;
using HarmonyLib;

namespace DisableGunSound;

[HarmonyPatch(typeof(RocketLauncher), nameof(RocketLauncher.Shoot))]
public static class RocketLauncherShootPatch
{
    public static bool Prefix(RocketLauncher __instance)
    {
        if (__instance.variation == 1 && __instance.cbCharge > 0f)
        {
            __instance.chargeSound.Stop();
            __instance.cbCharge = 0f;
        }
        Object.Instantiate<GameObject>(__instance.muzzleFlash, __instance.shootPoint.position, MonoSingleton<CameraController>.Instance.transform.rotation);
        __instance.anim.SetTrigger("Fire");
        __instance.cooldown = __instance.rateOfFire;
        GameObject gameObject = Object.Instantiate<GameObject>(__instance.rocket, MonoSingleton<CameraController>.Instance.transform.position, MonoSingleton<CameraController>.Instance.transform.rotation);
        if (MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget && MonoSingleton<CameraFrustumTargeter>.Instance.IsAutoAimed)
        {
            gameObject.transform.LookAt(MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget.bounds.center);
        }
        Grenade component = gameObject.GetComponent<Grenade>();
        if (component)
        {
            component.sourceWeapon = MonoSingleton<GunControl>.Instance.currentWeapon;
        }
        MonoSingleton<CameraController>.Instance.CameraShake(0.75f);
        MonoSingleton<RumbleManager>.Instance.SetVibrationTracked(RumbleProperties.GunFire, __instance.gameObject);

        return false;
    }
}

[HarmonyPatch(typeof(RocketLauncher), nameof(RocketLauncher.ShootCannonball))]
public static class RocketLauncherShootCannonballPatch
{
    public static bool Prefix(RocketLauncher __instance)
    {
		__instance.transform.localPosition = __instance.wpos.currentDefault;
		Object.Instantiate<GameObject>(__instance.muzzleFlash, __instance.shootPoint.position, MonoSingleton<CameraController>.Instance.transform.rotation);
		__instance.anim.SetTrigger("Fire");
		__instance.cooldown = __instance.rateOfFire;
		Rigidbody rigidbody = Object.Instantiate<Rigidbody>(__instance.cannonBall, MonoSingleton<CameraController>.Instance.transform.position + MonoSingleton<CameraController>.Instance.transform.forward, MonoSingleton<CameraController>.Instance.transform.rotation);
		if (MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget && MonoSingleton<CameraFrustumTargeter>.Instance.IsAutoAimed)
		{
			rigidbody.transform.LookAt(MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget.bounds.center);
		}
		rigidbody.velocity = rigidbody.transform.forward * Mathf.Max(15f, __instance.cbCharge * 150f);
		Cannonball cannonball;
		if (rigidbody.TryGetComponent<Cannonball>(out cannonball))
		{
			cannonball.sourceWeapon = MonoSingleton<GunControl>.Instance.currentWeapon;
		}
		MonoSingleton<CameraController>.Instance.CameraShake(0.75f);
		__instance.cbCharge = 0f;
		__instance.firingCannonball = false;
        
        return false;
    }
}