using HarmonyLib;
using UnityEngine;

namespace PlayableSpade.BossPatches
{
    internal class PatchPlayerBossSpade
    {
        static bool FightStarted = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerBossSpade), "Start", MethodType.Normal)]
        static void PatchPlayerBossSpadeStart(PlayerBossSpade __instance)
        {
            FightStarted = false;
            //Spade's code lacks this line compared to other PlayerBosses - setting this prevents him from instantly shooting you at the start of the fight
            __instance.genericTimer = -30f;

            //Mirror Match
            if (FPSaveManager.character == PlayableSpade.spadeCharID)
            {
                SpriteRenderer playerSprite = __instance.GetComponent<SpriteRenderer>();
                if (playerSprite != null)
                {
                    playerSprite.color = new Color(0f, 1f, 1f);
                }
                SpriteOutline outline = __instance.GetComponent<SpriteOutline>();
                if (outline != null)
                {
                    outline.enabled = true;
                }
            }
        }

        //Black magic to fix outdated code in the game's files. Spade lacks a check if FPStage's timeEnabled bool is true, making him attack you before round starts (unlike other PlayerBoss instances)
        //This fixes it by simulating that functionality
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerBossSpade), "State_Running", MethodType.Normal)]
        static bool PatchPlayerBossSpadeRun(PlayerBossSpade __instance)
        {
            if (!FightStarted)
            {
                FightStarted = FPStage.timeEnabled;
            }

            if (__instance.targetToPursue == null)
            {
                //Set a target
                if (__instance.faction != "Player")
                    __instance.targetToPursue = FPStage.FindNearestPlayer(__instance, __instance.pursuitRange);
                else
                    __instance.targetToPursue = FPStage.FindNearestEnemy(__instance, __instance.pursuitRange);
            }
            return FightStarted;
        }

        //Needed to make him actually target you, the base code doesnt do it by itself
        //When in Player's team he is fine firing blindly.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerBossSpade), "State_ThrowCards", MethodType.Normal)]
        static void PatchPlayerBossSpadeCardThrow(PlayerBossSpade __instance)
        {   
            if (__instance.faction != "Player")
                __instance.Action_FacePlayer();
        }

        //Recharge his attack budget
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerBossSpade),"LateUpdate", MethodType.Normal)]
        static void PatchBossSpadeLateUpdate(PlayerBossSpade __instance)
        {
            if (__instance.energy < 100f)
            {
                __instance.energy += 0.6f * FPStage.deltaTime;
            }
        }

    }
}
