using UnityEngine;
using HarmonyLib;

namespace DisableGunSound;

[HarmonyPatch(typeof(Railcannon), nameof(Railcannon.Shoot))]
public static class RailcannonShootPatch
{
    public static bool Prefix(Railcannon __instance)
    {
        GameObject gameObject = Object.Instantiate<GameObject>(__instance.beam, __instance.cc.GetDefaultPos(), __instance.cc.transform.rotation);
		if (__instance.targeter.CurrentTarget && __instance.targeter.IsAutoAimed)
		{
			gameObject.transform.LookAt(__instance.targeter.CurrentTarget.bounds.center);
		}
		if (__instance.variation != 1)
		{
			RevolverBeam revolverBeam;
			if (gameObject.TryGetComponent<RevolverBeam>(out revolverBeam))
			{
				revolverBeam.sourceWeapon = __instance.gc.currentWeapon;
				revolverBeam.alternateStartPoint = __instance.shootPoint.position;
			}
		}
		else
		{
			gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * 250f, ForceMode.VelocityChange);
			Harpoon harpoon;
			if (gameObject.TryGetComponent<Harpoon>(out harpoon))
			{
				harpoon.sourceWeapon = __instance.gameObject;
			}
		}
		//Object.Instantiate<GameObject>(__instance.fireSound);
		__instance.anim.SetTrigger("Shoot");
		__instance.cc.CameraShake(2f);
		MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.GunFireStrong);

        return false;
    }
}