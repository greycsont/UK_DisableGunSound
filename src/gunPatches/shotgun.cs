/* Shotgun patches
 * 
 * You need to replace the audio by block and modify the original methods.
 * 
 */





using HarmonyLib;
using UnityEngine;

namespace DisableGunSound;


[HarmonyPatch(typeof(Shotgun), nameof(Shotgun.ShootSaw))]
public static class ShotgunShootSawPatch
{
    public static bool Prefix(Shotgun __instance)
    {
        var volume = InstanceConfig.Volume;

        __instance.gunReady = true;
        __instance.transform.localPosition = __instance.wpos.currentDefault;
        Vector3 vector = __instance.cam.transform.forward;
        if (__instance.targeter.CurrentTarget && __instance.targeter.IsAutoAimed)
        {
            vector = (__instance.targeter.CurrentTarget.bounds.center - MonoSingleton<CameraController>.Instance.GetDefaultPos()).normalized;
        }
        foreach (Transform transform in __instance.shootPoints)
        {
            Vector3 position = MonoSingleton<CameraController>.Instance.GetDefaultPos() + vector * 0.5f;
            RaycastHit raycastHit;
            if (Physics.Raycast(MonoSingleton<CameraController>.Instance.GetDefaultPos(), vector, out raycastHit, 5f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
            {
                position = raycastHit.point - vector * 5f;
            }
            Chainsaw chainsaw = Object.Instantiate<Chainsaw>(__instance.chainsaw, position, Random.rotation);
            chainsaw.weaponType = "shotgun" + __instance.variation.ToString();
            chainsaw.CheckMultipleRicochets(true);
            chainsaw.sourceWeapon = __instance.gc.currentWeapon;
            chainsaw.attachedTransform = MonoSingleton<PlayerTracker>.Instance.GetTarget();
            chainsaw.lineStartTransform = __instance.chainsawAttachPoint;
            chainsaw.GetComponent<Rigidbody>().AddForce(vector * (__instance.grenadeForce + 10f) * 1.5f, ForceMode.VelocityChange);
            __instance.currentChainsaws.Add(chainsaw);
        }
        __instance.chainsawBladeRenderer.material = __instance.chainsawBladeMaterial;
        __instance.chainsawBladeScroll.scrollSpeedX = 0f;
        __instance.chainsawAttachPoint.gameObject.SetActive(false);
        Object.Instantiate<GameObject>(__instance.grenadeSoundBubble).GetComponent<AudioSource>().volume = 0f;
        __instance.anim.Play("FireNoReload");
        __instance.gunAud.clip = __instance.shootSound;

        __instance.gunAud.volume = volume;

        __instance.gunAud.panStereo = 0f;
        __instance.gunAud.pitch = Random.Range(0.75f, 0.85f);
        __instance.gunAud.Play();
        __instance.cc.CameraShake(1f);
        __instance.releasingHeat = false;
        __instance.grenadeForce = 0f;

        return false;
    }
}


[HarmonyPatch(typeof(Shotgun), nameof(Shotgun.ShootSinks))]
public static class ShotgunShootSinksPatch
{
    public static bool Prefix(Shotgun __instance)
    {
        var volume = InstanceConfig.Volume;

        __instance.gunReady = false;
        __instance.transform.localPosition = __instance.wpos.currentDefault;
        foreach (Transform transform in __instance.shootPoints)
        {
            GameObject gameObject = Object.Instantiate<GameObject>(__instance.grenade, __instance.cam.transform.position + __instance.cam.transform.forward * 0.5f, Random.rotation);
            gameObject.GetComponentInChildren<Grenade>().sourceWeapon = __instance.gc.currentWeapon;
            gameObject.GetComponent<Rigidbody>().AddForce(__instance.grenadeVector * (__instance.grenadeForce + 10f), ForceMode.VelocityChange);
        }
        Object.Instantiate<GameObject>(__instance.grenadeSoundBubble).GetComponent<AudioSource>().volume = 0f;
        __instance.anim.SetTrigger("Secondary Fire");
        __instance.gunAud.clip = __instance.shootSound;

        __instance.gunAud.volume = volume;

        __instance.gunAud.panStereo = 0f;
        __instance.gunAud.pitch = Random.Range(0.75f, 0.85f);
        __instance.gunAud.Play();
        __instance.cc.CameraShake(1f);
        __instance.meterOverride = true;
        __instance.chargeSlider.value = 0f;
        __instance.sliderFill.color = Color.black;
        foreach (Transform transform2 in __instance.shootPoints)
        {
            Object.Instantiate<GameObject>(__instance.muzzleFlash, transform2.transform.position, transform2.transform.rotation);
        }
        __instance.releasingHeat = false;
        __instance.tempColor.a = 0f;
        __instance.heatSinkSMR.sharedMaterials[3].SetColor("_TintColor", __instance.tempColor);
        __instance.grenadeForce = 0f;

        return false;
    }
}

[HarmonyPatch(typeof(Shotgun), nameof(Shotgun.Shoot))]
public static class ShotgunShootPatch
{
    public static bool Prefix(Shotgun __instance)
    {
        var volume = InstanceConfig.Volume;

        __instance.gunReady = false;
        int num = 12;
        if (__instance.variation == 1)
        {
            switch (__instance.primaryCharge)
            {
                case 0:
                    num = 10;
                    __instance.gunAud.pitch = Random.Range(1.15f, 1.25f);
                    break;
                case 1:
                    num = 16;
                    __instance.gunAud.pitch = Random.Range(0.95f, 1.05f);
                    break;
                case 2:
                    num = 24;
                    __instance.gunAud.pitch = Random.Range(0.75f, 0.85f);
                    break;
                case 3:
                    num = 0;
                    __instance.gunAud.pitch = Random.Range(0.75f, 0.85f);
                    break;
            }
        }
        MonoSingleton<CameraController>.Instance.StopShake();
        Vector3 direction = __instance.cam.transform.forward;
        if (__instance.targeter.CurrentTarget && __instance.targeter.IsAutoAimed)
        {
            direction = (__instance.targeter.CurrentTarget.bounds.center - MonoSingleton<CameraController>.Instance.GetDefaultPos()).normalized;
        }
        __instance.rhits = Physics.RaycastAll(__instance.cam.transform.position, direction, 4f, LayerMaskDefaults.Get(LMD.Enemies));
        if (__instance.rhits.Length != 0)
        {
            foreach (RaycastHit raycastHit in __instance.rhits)
            {
                if (raycastHit.collider.gameObject.CompareTag("Body"))
                {
                    EnemyIdentifierIdentifier componentInParent = raycastHit.collider.GetComponentInParent<EnemyIdentifierIdentifier>();
                    if (componentInParent && componentInParent.eid)
                    {
                        EnemyIdentifier eid = componentInParent.eid;
                        if (!eid.dead && !eid.blessed && __instance.anim.GetCurrentAnimatorStateInfo(0).IsName("Equip"))
                        {
                            MonoSingleton<StyleHUD>.Instance.AddPoints(50, "ultrakill.quickdraw", __instance.gc.currentWeapon, eid, -1, "", "");
                        }
                        eid.hitter = "shotgunzone";
                        if (!eid.hitterWeapons.Contains("shotgun" + __instance.variation.ToString()))
                        {
                            eid.hitterWeapons.Add("shotgun" + __instance.variation.ToString());
                        }
                        eid.DeliverDamage(raycastHit.collider.gameObject, (eid.transform.position - __instance.transform.position).normalized * 10000f, raycastHit.point, 4f, false, 0f, __instance.gameObject, false, false);
                    }
                }
            }
        }
        MonoSingleton<RumbleManager>.Instance.SetVibrationTracked(RumbleProperties.GunFireProjectiles, __instance.gameObject);
        if (__instance.variation != 1 || __instance.primaryCharge != 3)
        {
            for (int j = 0; j < num; j++)
            {
                GameObject gameObject = Object.Instantiate<GameObject>(__instance.bullet, __instance.cam.transform.position, __instance.cam.transform.rotation);
                Projectile component = gameObject.GetComponent<Projectile>();
                component.weaponType = "shotgun" + __instance.variation.ToString();
                component.sourceWeapon = __instance.gc.currentWeapon;
                if (__instance.targeter.CurrentTarget && __instance.targeter.IsAutoAimed)
                {
                    gameObject.transform.LookAt(__instance.targeter.CurrentTarget.bounds.center);
                }
                if (__instance.variation == 1)
                {
                    switch (__instance.primaryCharge)
                    {
                        case 0:
                            gameObject.transform.Rotate(Random.Range(-__instance.spread / 1.5f, __instance.spread / 1.5f), Random.Range(-__instance.spread / 1.5f, __instance.spread / 1.5f), Random.Range(-__instance.spread / 1.5f, __instance.spread / 1.5f));
                            break;
                        case 1:
                            gameObject.transform.Rotate(Random.Range(-__instance.spread, __instance.spread), Random.Range(-__instance.spread, __instance.spread), Random.Range(-__instance.spread, __instance.spread));
                            break;
                        case 2:
                            gameObject.transform.Rotate(Random.Range(-__instance.spread * 2f, __instance.spread * 2f), Random.Range(-__instance.spread * 2f, __instance.spread * 2f), Random.Range(-__instance.spread * 2f, __instance.spread * 2f));
                            break;
                    }
                }
                else
                {
                    gameObject.transform.Rotate(Random.Range(-__instance.spread, __instance.spread), Random.Range(-__instance.spread, __instance.spread), Random.Range(-__instance.spread, __instance.spread));
                }
            }
        }
        else
        {
            Vector3 position = __instance.cam.transform.position + __instance.cam.transform.forward;
            RaycastHit raycastHit2;
            if (Physics.Raycast(__instance.cam.transform.position, __instance.cam.transform.forward, out raycastHit2, 1f, LayerMaskDefaults.Get(LMD.Environment)))
            {
                position = raycastHit2.point - __instance.cam.transform.forward * 0.1f;
            }
            GameObject gameObject2 = Object.Instantiate<GameObject>(__instance.explosion, position, __instance.cam.transform.rotation);
            if (__instance.targeter.CurrentTarget && __instance.targeter.IsAutoAimed)
            {
                gameObject2.transform.LookAt(__instance.targeter.CurrentTarget.bounds.center);
            }
            foreach (Explosion explosion in gameObject2.GetComponentsInChildren<Explosion>())
            {
                explosion.sourceWeapon = __instance.gc.currentWeapon;
                explosion.enemyDamageMultiplier = 1f;
                explosion.maxSize *= 1.5f;
                explosion.damage = 50;
            }
        }
        if (__instance.variation != 1)
        {
            __instance.gunAud.pitch = Random.Range(0.95f, 1.05f);
        }
        __instance.gunAud.clip = __instance.shootSound;

        __instance.gunAud.volume = volume;

        __instance.gunAud.panStereo = 0f;
        __instance.gunAud.Play();
        __instance.cc.CameraShake(1f);
        if (__instance.variation == 1)
        {
            __instance.anim.SetTrigger("PumpFire");
        }
        else
        {
            __instance.anim.SetTrigger("Fire");
        }
        foreach (Transform transform in __instance.shootPoints)
        {
            Object.Instantiate<GameObject>(__instance.muzzleFlash, transform.transform.position, transform.transform.rotation);
        }
        __instance.releasingHeat = false;
        __instance.tempColor.a = 1f;
        __instance.heatSinkSMR.sharedMaterials[3].SetColor("_TintColor", __instance.tempColor);
        if (__instance.variation == 1)
        {
            __instance.primaryCharge = 0;
        }

        return false;
    }
}