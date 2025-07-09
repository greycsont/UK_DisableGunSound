/*
 * Railcannon patches
 * 
 * For Electric and Screwdriver, you can just access the Gameobject's audiosource
 * But for malicious, if have fucking 3 audiosource
 * so you need to get all of them to make sure no any issue
 * 
 */



using UnityEngine;
using HarmonyLib;

namespace DisableGunSound;

[HarmonyPatch(typeof(Railcannon), nameof(Railcannon.Shoot))]
public static class RailcannonShootPatch
{
    public static void Prefix(Railcannon __instance)
    {
        var volume = InstanceConfig.Volume;

        if (__instance.variation == 2)
        {
            var aud = __instance.fireSound.GetComponentsInChildren<AudioSource>();
            foreach (var audiosource in aud)
            {
                audiosource.volume = volume;
            }
        }
        else
        {
            var aud = __instance.fireSound.GetComponent<AudioSource>();
            if (aud)
            {
                aud.volume = volume;
            }
        }
    }
    /*public static bool Prefix(Railcannon __instance)
    {
        var volume = InstanceConfig.Volume;

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
		var fireSound = Object.Instantiate<GameObject>(__instance.fireSound);
        var aud = fireSound.GetComponent<AudioSource>();
        aud.Stop();
        aud.volume = volume;
        aud.Play();

		__instance.anim.SetTrigger("Shoot");
		__instance.cc.CameraShake(2f);
		MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.GunFireStrong);

        return false;
    }*/
}