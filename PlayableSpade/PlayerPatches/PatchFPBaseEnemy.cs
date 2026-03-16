using HarmonyLib;
using PlayableSpade.Objects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PlayableSpade.PlayerPatches
{
    internal class PatchFPBaseEnemy
    {
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(FPBaseEnemy),"DamageCheck",MethodType.Normal)]
        static void PatchFPBaseEnemyDamageCheck(ref int __result, ref FPBaseEnemy __instance, ref float ___firstHitAdvantage, ref float ___deathSpinSpeed)
        {
            if (__result == 0 && FPSaveManager.character == PlayableSpade.spadeCharID)
            {
                FPBaseObject objectRef = null;
                Vector2 hitBoxCenter = new Vector2(__instance.position.x + (__instance.hbWeakpoint.left + __instance.hbWeakpoint.right) * 0.5f, __instance.position.y + (__instance.hbWeakpoint.bottom + __instance.hbWeakpoint.top) * 0.5f);
                Vector2 blockBoxCenter = new Vector2(__instance.position.x + (__instance.hbBlock.left + __instance.hbBlock.right) * 0.5f, __instance.position.y + (__instance.hbBlock.bottom + __instance.hbBlock.top) * 0.5f);

                while (FPStage.ForEach(SpadeCaptureCard.classID, ref objectRef))
                {
                    SpadeCaptureCard spadeCaptureCard = (SpadeCaptureCard)objectRef;
                    if (spadeCaptureCard.enemyCollision || !(spadeCaptureCard.faction != __instance.faction))
                    {
                        continue;
                    }
                    if (FPCollision.CheckOOBB(__instance, __instance.hbWeakpoint, objectRef, spadeCaptureCard.hbTouch))
                    {
                        __instance.invincibility = 2f;
                        __instance.health -= spadeCaptureCard.attackPower * __instance.receivedDamageMultiplier;
                        __instance.lastReceivedDamage += spadeCaptureCard.attackPower * __instance.receivedDamageMultiplier;
                        __instance.lastReceivedDamageUnmodified += spadeCaptureCard.attackPower;
                        __instance.badgePacifist = false;
                        ___firstHitAdvantage = 0f;
                        spadeCaptureCard.enemyCollision = true;
                        spadeCaptureCard.ignoreTerrain = true;
                        spadeCaptureCard.hbTouch.enabled = false;
                        FPStage.CreateStageObject(HitSpark.classID, (spadeCaptureCard.position.x + hitBoxCenter.x * 2f) * (1f / 3f) + Random.Range(-10f, 10f), (spadeCaptureCard.position.y + hitBoxCenter.y * 2f) * (1f / 3f) + Random.Range(-10f, 10f));
                        if (!__instance.cannotBeKilled && __instance.health <= 0f)
                        {
                            __instance.SetDeath(spadeCaptureCard.attackKnockback.x, 4.5f + spadeCaptureCard.attackKnockback.y);
                            ___deathSpinSpeed = 0f - Mathf.Max(Mathf.Abs(spadeCaptureCard.attackKnockback.x * 1.5f), 8f);
                            FPStage.ForEachBreak();
                            __result = 2;
                            break;
                        }
                        if (__instance.cannotBeKilled && __instance.health < 1f)
                        {
                            __instance.health = 1f;
                        }
                        FPAudio.PlayHitSfx(5);
                        FPStage.ForEachBreak();
                        __result = 1;
                        break;
                    }
                    if (FPCollision.CheckOOBB(__instance, __instance.hbBlock, objectRef, spadeCaptureCard.hbTouch))
                    {
                        __instance.invincibility = 2f;
                        spadeCaptureCard.enemyCollision = true;
                        FPStage.CreateStageObject(HitBlock.classID, (spadeCaptureCard.position.x + blockBoxCenter.x * 2f) * (1f / 3f) + Random.Range(-10f, 10f), (spadeCaptureCard.position.y + blockBoxCenter.y * 2f) * (1f / 3f) + Random.Range(-10f, 10f));
                        FPStage.ForEachBreak();
                        __result = 3;
                        break;
                    }
                }
                if (__result != 0)
                {
                    __instance.health = FPCommon.RoundToQuantumWithinErrorThreshold(__instance.health, 0.1f);
                    __instance.damageSource = objectRef;
                    return;
                }

            }
        }
    }
}
