using UnityEngine;
using HarmonyLib;
using System.Collections;

namespace DisableGunSound;


[HarmonyPatch(typeof(ShotgunHammer), nameof(ShotgunHammer.ShootSaw))]
public static class ShotgunHammerShootSawPatch
{
    public static bool Prefix(ShotgunHammer __instance)
    {
        var volume = InstanceConfig.Volume;

        __instance.gunReady = true;
        __instance.transform.localPosition = __instance.wpos.currentDefault;
        Vector3 a = MonoSingleton<CameraController>.Instance.transform.forward;
        if (__instance.targeter.CurrentTarget && __instance.targeter.IsAutoAimed)
        {
            a = (__instance.targeter.CurrentTarget.bounds.center - MonoSingleton<CameraController>.Instance.GetDefaultPos()).normalized;
        }
        Vector3 position = MonoSingleton<CameraController>.Instance.GetDefaultPos() + a * 0.5f;
        RaycastHit raycastHit;
        if (Physics.Raycast(MonoSingleton<CameraController>.Instance.GetDefaultPos(), a, out raycastHit, 5f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
        {
            position = raycastHit.point - a * 5f;
        }
        Chainsaw chainsaw = Object.Instantiate<Chainsaw>(__instance.chainsaw, position, Random.rotation);
        chainsaw.weaponType = "hammer" + __instance.variation.ToString();
        chainsaw.CheckMultipleRicochets(true);
        chainsaw.sourceWeapon = __instance.gc.currentWeapon;
        chainsaw.attachedTransform = MonoSingleton<PlayerTracker>.Instance.GetTarget();
        chainsaw.lineStartTransform = __instance.chainsawAttachPoint;
        chainsaw.GetComponent<Rigidbody>().AddForce(a * (__instance.chargeForce + 10f) * 1.5f, ForceMode.VelocityChange);
        __instance.currentChainsaws.Add(chainsaw);
        __instance.chainsawBladeRenderer.material = __instance.chainsawBladeMaterial;
        __instance.chainsawBladeScroll.scrollSpeedX = 0f;
        __instance.chainsawAttachPoint.gameObject.SetActive(false);

        AudioSource aud = Object.Instantiate<AudioSource>(__instance.nadeSpawnSound);
        aud.Stop();
        aud.volume = volume;
        aud.Play();

        __instance.anim.Play("SawingShot");
        MonoSingleton<CameraController>.Instance.CameraShake(1f);
        __instance.chargeForce = 0f;

        return false;
    }
}


[HarmonyPatch(typeof(ShotgunHammer), nameof(ShotgunHammer.ThrowNade))]
public static class ShotgunHammerThrowNadePatch
{
    public static bool Prefix(ShotgunHammer __instance)
    {
        var volume = InstanceConfig.Volume;

        MonoSingleton<WeaponCharges>.Instance.shoAltNadeCharge = 0f;
        __instance.pulledOut = 0.3f;
        __instance.gunReady = false;
        __instance.aboutToSecondary = false;
        Vector3 a = MonoSingleton<CameraController>.Instance.transform.forward;
        if (__instance.targeter.CurrentTarget && __instance.targeter.IsAutoAimed)
        {
            a = (__instance.targeter.CurrentTarget.bounds.center - MonoSingleton<CameraController>.Instance.GetDefaultPos()).normalized;
        }
        GameObject gameObject = Object.Instantiate<GameObject>(__instance.grenade, MonoSingleton<CameraController>.Instance.GetDefaultPos() + a * 2f - MonoSingleton<CameraController>.Instance.transform.up * 0.5f, Random.rotation);
        Rigidbody rigidbody;
        if (gameObject.TryGetComponent<Rigidbody>(out rigidbody))
        {
            rigidbody.AddForce(MonoSingleton<CameraController>.Instance.transform.forward * 3f + Vector3.up * 7.5f + (MonoSingleton<NewMovement>.Instance.ridingRocket ? MonoSingleton<NewMovement>.Instance.ridingRocket.rb.velocity : MonoSingleton<NewMovement>.Instance.rb.velocity), ForceMode.VelocityChange);
        }
        Grenade grenade;
        if (gameObject.TryGetComponent<Grenade>(out grenade))
        {
            grenade.sourceWeapon = __instance.gameObject;
        }
        __instance.anim.Play("NadeSpawn", -1, 0f);
        
        AudioSource aud = Object.Instantiate<AudioSource>(__instance.nadeSpawnSound);
        aud.Stop();
        aud.volume = volume;
        aud.Play();

        return false;
    }
}


[HarmonyPatch(typeof(ShotgunHammer), nameof(ShotgunHammer.ImpactEffects))]
public static class ShotgunHammerImpactEffectsPatch
{
    public static bool Prefix(ShotgunHammer __instance)
    {
        var volume = InstanceConfig.Volume;

        Vector3 position = (__instance.hitPosition != Vector3.zero) ? (__instance.hitPosition - (__instance.hitPosition - MonoSingleton<CameraController>.Instance.GetDefaultPos()).normalized) : (MonoSingleton<CameraController>.Instance.GetDefaultPos() + __instance.direction * 2.5f);
		if (__instance.primaryCharge > 0)
		{
			GameObject gameObject = Object.Instantiate<GameObject>((__instance.primaryCharge == 3) ? __instance.overPumpExplosion : __instance.pumpExplosion, position, Quaternion.LookRotation(__instance.direction));
			foreach (Explosion explosion in gameObject.GetComponentsInChildren<Explosion>())
			{
				explosion.sourceWeapon = __instance.gameObject;
				explosion.hitterWeapon = "hammer";
				if (__instance.primaryCharge == 2)
				{
					explosion.maxSize *= 2f;
				}
			}
			AudioSource audioSource;
			if (__instance.primaryCharge == 2 && gameObject.TryGetComponent<AudioSource>(out audioSource))
			{
				audioSource.volume = 1f;
				audioSource.pitch -= 0.4f;
			}
			__instance.primaryCharge = 0;
		}
		if (__instance.forceWeakHit || __instance.tier == 0)
		{
			__instance.anim.Play("Fire", -1, 0f);
		}
		else if (__instance.tier == 1)
		{
			__instance.anim.Play("FireStrong", -1, 0f);
		}
		else
		{
			__instance.anim.Play("FireStrongest", -1, 0f);
		}

		GameObject hitsound = Object.Instantiate<GameObject>(__instance.hitImpactParticle[__instance.forceWeakHit ? 0 : __instance.tier], position, MonoSingleton<CameraController>.Instance.transform.rotation);
        AudioSource hitsoundAudioSource = hitsound.GetComponent<AudioSource>();
        hitsoundAudioSource.Stop();
        hitsoundAudioSource.volume = volume;
        hitsoundAudioSource.Play();

        return false;
    }
}


[HarmonyPatch(typeof(ShotgunHammer), nameof(ShotgunHammer.Impact))]
public static class ShotgunHammerImpactPatch
{
    public static bool Prefix(ShotgunHammer __instance)
    {
        __instance.impactRoutine = __instance.StartCoroutine(ImpactRoutine(__instance));
        return false;
    }
    public static IEnumerator ImpactRoutine(ShotgunHammer __instance)
    {
        var volume = InstanceConfig.Volume;


        __instance.hitEnemy = null;
        __instance.hitGrenade = null;
        __instance.target = null;
        __instance.hitPosition = Vector3.zero;
        __instance.hammerCooldown = 0.5f;
        Vector3 position = MonoSingleton<CameraController>.Instance.GetDefaultPos();
        __instance.direction = MonoSingleton<CameraController>.Instance.transform.forward;
        if (__instance.targeter.CurrentTarget && __instance.targeter.IsAutoAimed)
        {
            __instance.direction = (__instance.targeter.CurrentTarget.bounds.center - MonoSingleton<CameraController>.Instance.GetDefaultPos()).normalized;
        }
        if (MonoSingleton<ObjectTracker>.Instance.grenadeList.Count > 0 || MonoSingleton<WeaponCharges>.Instance.shoSawAmount > 0 || MonoSingleton<ObjectTracker>.Instance.landmineList.Count > 0)
        {
            Collider[] cols = Physics.OverlapSphere(position, 0.01f);
            if (cols.Length != 0)
            {
                int num;
                for (int i = 0; i < cols.Length; i = num + 1)
                {
                    Transform transform = cols[i].transform;
                    ParryHelper parryHelper;
                    if (transform.TryGetComponent<ParryHelper>(out parryHelper))
                    {
                        transform = parryHelper.target;
                    }
                    if (MonoSingleton<ObjectTracker>.Instance.grenadeList.Count > 0 && transform.gameObject.layer == 10)
                    {
                        Grenade componentInParent = transform.GetComponentInParent<Grenade>();
                        if (componentInParent)
                        {
                            __instance.hitGrenade = componentInParent;
                            cols[i].enabled = false;

                            //Edited
                            AudioSource aud = Object.Instantiate<AudioSource>(__instance.hitSound, __instance.transform.position, Quaternion.identity);
                            aud.Stop();
                            aud.volume = volume;
                            aud.Play();

                            MonoSingleton<TimeController>.Instance.TrueStop(0.25f);
                            yield return new WaitForSeconds(0.01f);
                            __instance.HitNade();
                        }
                    }
                    else if (MonoSingleton<WeaponCharges>.Instance.shoSawAmount > 0 || MonoSingleton<ObjectTracker>.Instance.landmineList.Count > 0)
                    {
                        Chainsaw chainsaw;
                        Landmine landmine;
                        if (MonoSingleton<WeaponCharges>.Instance.shoSawAmount > 0 && transform.TryGetComponent<Chainsaw>(out chainsaw))
                        {
                            // edited
                            AudioSource aud = Object.Instantiate<AudioSource>(__instance.hitSound, __instance.transform.position, Quaternion.identity);
                            aud.Stop();
                            aud.volume = volume;
                            aud.Play();

                            chainsaw.GetPunched();
                            chainsaw.transform.position = MonoSingleton<CameraController>.Instance.GetDefaultPos() + __instance.direction;
                            chainsaw.rb.velocity = (Punch.GetParryLookTarget() - chainsaw.transform.position).normalized * 105f;
                        }
                        else if (MonoSingleton<ObjectTracker>.Instance.landmineList.Count > 0 && transform.TryGetComponent<Landmine>(out landmine))
                        {
                            landmine.transform.LookAt(Punch.GetParryLookTarget());
                            landmine.Parry();
                            
                            AudioSource aud = Object.Instantiate<AudioSource>(__instance.hitSound, __instance.transform.position, Quaternion.identity);
                            aud.Stop();
                            aud.volume = volume;
                            aud.Play();

                            __instance.anim.Play("Fire", -1, 0f);
                            MonoSingleton<TimeController>.Instance.TrueStop(0.25f);
                            yield return new WaitForSeconds(0.01f);
                        }
                    }
                    num = i;
                }
            }
            cols = null;
        }
        if (MonoSingleton<WeaponCharges>.Instance.shoSawAmount > 0 || MonoSingleton<ObjectTracker>.Instance.landmineList.Count > 0)
        {
            RaycastHit[] rhits = Physics.RaycastAll(position, __instance.direction, 8f, 16384, QueryTriggerInteraction.Collide);
            int num;
            for (int i = 0; i < rhits.Length; i = num + 1)
            {
                Transform transform2 = rhits[i].transform;
                ParryHelper parryHelper2;
                if (transform2.TryGetComponent<ParryHelper>(out parryHelper2))
                {
                    transform2 = parryHelper2.target;
                }
                Chainsaw chainsaw2;
                Landmine landmine2;
                if (transform2.TryGetComponent<Chainsaw>(out chainsaw2))
                {
                    AudioSource aud = Object.Instantiate<AudioSource>(__instance.hitSound, __instance.transform.position, Quaternion.identity);
                    aud.Stop();
                    aud.volume = volume;
                    aud.Play();

                    chainsaw2.GetPunched();
                    chainsaw2.transform.position = MonoSingleton<CameraController>.Instance.GetDefaultPos() + __instance.direction;
                    chainsaw2.rb.velocity = (Punch.GetParryLookTarget() - chainsaw2.transform.position).normalized * 105f;
                }
                else if (transform2.TryGetComponent<Landmine>(out landmine2))
                {
                    landmine2.transform.LookAt(Punch.GetParryLookTarget());
                    landmine2.Parry();

                    AudioSource aud = Object.Instantiate<AudioSource>(__instance.hitSound, __instance.transform.position, Quaternion.identity);
                    aud.Stop();
                    aud.volume = volume;
                    aud.Play();

                    __instance.anim.Play("Fire", -1, 0f);
                    MonoSingleton<TimeController>.Instance.TrueStop(0.25f);
                    yield return new WaitForSeconds(0.01f);
                }
                num = i;
            }
            rhits = null;
        }
        RaycastHit rhit;
        if (Physics.Raycast(position, __instance.direction, out rhit, 8f, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment), QueryTriggerInteraction.Collide))
        {
            if (rhit.transform.gameObject.layer == 11 || rhit.transform.gameObject.layer == 10)
            {
                ParryHelper parryHelper3;
                EnemyIdentifierIdentifier enemyIdentifierIdentifier2;
                if (rhit.transform.gameObject.TryGetComponent<ParryHelper>(out parryHelper3))
                {
                    EnemyIdentifierIdentifier enemyIdentifierIdentifier;
                    EnemyIdentifier enemyIdentifier;
                    if (parryHelper3.target.TryGetComponent<EnemyIdentifierIdentifier>(out enemyIdentifierIdentifier) && enemyIdentifierIdentifier.eid && !enemyIdentifierIdentifier.eid.dead)
                    {
                        __instance.hitEnemy = enemyIdentifierIdentifier.eid;
                    }
                    else if (parryHelper3.target.TryGetComponent<EnemyIdentifier>(out enemyIdentifier) && !enemyIdentifier.dead)
                    {
                        __instance.hitEnemy = enemyIdentifier;
                    }
                }
                else if (rhit.transform.TryGetComponent<EnemyIdentifierIdentifier>(out enemyIdentifierIdentifier2) && enemyIdentifierIdentifier2.eid && !enemyIdentifierIdentifier2.eid.dead)
                {
                    __instance.hitEnemy = enemyIdentifierIdentifier2.eid;
                }
                else if (MonoSingleton<ObjectTracker>.Instance.grenadeList.Count > 0 && rhit.transform.gameObject.layer == 10)
                {
                    Grenade componentInParent2 = rhit.transform.GetComponentInParent<Grenade>();
                    if (componentInParent2)
                    {
                        __instance.hitGrenade = componentInParent2;
                        rhit.collider.enabled = false;

                        AudioSource aud = Object.Instantiate<AudioSource>(__instance.hitSound, __instance.transform.position, Quaternion.identity);
                        aud.Stop();
                        aud.volume = volume;
                        aud.Play();

                        __instance.anim.Play("Fire", -1, 0f);
                        MonoSingleton<TimeController>.Instance.TrueStop(0.25f);
                        yield return new WaitForSeconds(0.01f);
                        __instance.HitNade();
                    }
                }
            }
            __instance.target = rhit.transform;
            __instance.hitPosition = rhit.point;
        }
        if (__instance.hitEnemy == null)
        {
            Vector3 vector = position + __instance.direction * 2.5f;
            Collider[] array = Physics.OverlapSphere(vector, 2.5f);
            if (array.Length != 0)
            {
                float num2 = 2.5f;
                for (int j = 0; j < array.Length; j++)
                {
                    ParryHelper parryHelper4;
                    Collider collider;
                    if (array[j].TryGetComponent<ParryHelper>(out parryHelper4) && parryHelper4.target.TryGetComponent<Collider>(out collider))
                    {
                        array[j] = collider;
                    }
                    if (array[j].gameObject.layer == 10 || array[j].gameObject.layer == 11)
                    {
                        Vector3 vector2 = array[j].ClosestPoint(vector);
                        if (!Physics.Raycast(position, vector2 - position, out rhit, Vector3.Distance(vector2, position), LayerMaskDefaults.Get(LMD.Environment)))
                        {
                            float num3 = Vector3.Distance(vector, vector2);
                            if (num3 < num2)
                            {
                                Transform transform3 = (array[j].attachedRigidbody != null) ? array[j].attachedRigidbody.transform : array[j].transform;
                                EnemyIdentifier enemyIdentifier2 = null;
                                EnemyIdentifierIdentifier enemyIdentifierIdentifier3;
                                if (transform3.TryGetComponent<EnemyIdentifierIdentifier>(out enemyIdentifierIdentifier3))
                                {
                                    enemyIdentifier2 = enemyIdentifierIdentifier3.eid;
                                }
                                else
                                {
                                    transform3.TryGetComponent<EnemyIdentifier>(out enemyIdentifier2);
                                }
                                if (enemyIdentifier2 && (!enemyIdentifier2.dead || __instance.hitEnemy == null))
                                {
                                    __instance.hitEnemy = enemyIdentifierIdentifier3.eid;
                                    num2 = num3;
                                    __instance.target = transform3;
                                    __instance.hitPosition = vector2;
                                }
                            }
                        }
                    }
                }
            }
            RaycastHit[] array2 = Physics.SphereCastAll(position + __instance.direction * 2.5f, 2.5f, __instance.direction, 3f, LayerMaskDefaults.Get(LMD.Enemies));
            if (array2.Length != 0)
            {
                float num4 = -1f;
                if (__instance.hitEnemy != null)
                {
                    num4 = Vector3.Dot(__instance.direction, __instance.hitPosition - position);
                }
                for (int k = 0; k < array2.Length; k++)
                {
                    if (!Physics.Raycast(position, array2[k].point - position, out rhit, Vector3.Distance(array2[k].point, position), LayerMaskDefaults.Get(LMD.Environment)))
                    {
                        float num5 = Vector3.Dot(__instance.direction, array2[k].point - position);
                        if (num5 > num4)
                        {
                            Transform transform4 = array2[k].transform;
                            Vector3 point = array2[k].point;
                            ParryHelper parryHelper5;
                            if (transform4.TryGetComponent<ParryHelper>(out parryHelper5))
                            {
                                transform4 = parryHelper5.target.transform;
                            }
                            EnemyIdentifier enemyIdentifier3 = null;
                            EnemyIdentifierIdentifier enemyIdentifierIdentifier4;
                            if (transform4.TryGetComponent<EnemyIdentifierIdentifier>(out enemyIdentifierIdentifier4))
                            {
                                enemyIdentifier3 = enemyIdentifierIdentifier4.eid;
                            }
                            else
                            {
                                transform4.TryGetComponent<EnemyIdentifier>(out enemyIdentifier3);
                            }
                            if (enemyIdentifier3 && (!enemyIdentifier3.dead || __instance.hitEnemy == null))
                            {
                                __instance.hitEnemy = enemyIdentifier3;
                                num4 = num5;
                                __instance.target = transform4;
                                __instance.hitPosition = point;
                            }
                        }
                    }
                }
            }
        }
        __instance.forceWeakHit = true;
        if (__instance.target != null)
        {
            Breakable breakable;
            Glass glass;
            if (__instance.hitEnemy != null)
            {
                float num6 = 0.05f;
                __instance.damage = 3f;
                if (__instance.tier == 2)
                {
                    num6 = 0.5f;
                    __instance.damage = 10f;
                }
                else if (__instance.tier == 1)
                {
                    num6 = 0.25f;
                    __instance.damage = 6f;
                }
                if (__instance.hitEnemy.dead)
                {
                    num6 = 0f;
                }
                if (num6 > 0f)
                {
                    __instance.forceWeakHit = false;
                    __instance.launchPlayer = true;

                    AudioSource aud = Object.Instantiate<AudioSource>(__instance.hitSound, __instance.transform.position, Quaternion.identity);
                    aud.Stop();
                    aud.volume = volume;
                    aud.Play();

                    MonoSingleton<TimeController>.Instance.TrueStop(num6);
                    yield return new WaitForSeconds(0.01f);
                }
                else
                {
                    __instance.launchPlayer = false;
                }
                __instance.DeliverDamage();
            }
            else if (__instance.target.TryGetComponent<Breakable>(out breakable) && !breakable.precisionOnly && !breakable.specialCaseOnly && !breakable.unbreakable)
            {
                breakable.Break();
            }
            else if (__instance.target.TryGetComponent<Glass>(out glass) && !glass.broken)
            {
                glass.Shatter();
            }
        }
        if (!__instance.hitGrenade && ((__instance.hitEnemy == null && __instance.target != null && LayerMaskDefaults.IsMatchingLayer(__instance.target.gameObject.layer, LMD.Environment)) || (__instance.hitEnemy && __instance.hitEnemy.dead)))
        {
            MonoSingleton<NewMovement>.Instance.Launch(-__instance.direction * ((float)(100 * __instance.tier + 300) / ((float)(MonoSingleton<NewMovement>.Instance.hammerJumps + 3) / 3f)), 8f, false);
            MonoSingleton<NewMovement>.Instance.hammerJumps++;
            MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(position, __instance.direction, 8f, 10, 2f);
        }
        __instance.ImpactEffects();
        __instance.hitEnemy = null;
        __instance.hitGrenade = null;
        __instance.impactRoutine = null;
        yield break;
    }
}