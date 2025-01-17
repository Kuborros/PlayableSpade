﻿using FP2Lib.Player;
using HarmonyLib;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchBFFMicroMissile
    {



        public static bool BFFActive = false;
        private static RuntimeAnimatorController missileAnim;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BFFMicroMissile), "Start", MethodType.Normal)]
        static void PatchBFFMicroMissileStart(BFFMicroMissile __instance)
        {
            if (missileAnim == null) missileAnim = __instance.gameObject.GetComponent<Animator>().runtimeAnimatorController;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BFFMicroMissile), "State_Default", MethodType.Normal)]
        static void PatchBFFMicroMissileDefault(BFFMicroMissile __instance, ref float ___speed, float ___explodeTimer)
        {
            //Run all this only when playing as Spade
            if (PlayerHandler.currentCharacter.id == 5)
            {

                if (!BFFActive)
                {
                    if ((___explodeTimer > 150f && ___explodeTimer < 175f) || (___explodeTimer > 120f && ___explodeTimer < 140f) || (___explodeTimer > 90f && ___explodeTimer < 110f) || (___explodeTimer > 60f && ___explodeTimer < 80f))
                    {
                        ___speed = 0;
                    }
                    else
                    {
                        ___speed = 20;
                    }

                    if (__instance.target.enemy != null)
                    {
                        if (__instance.target.enemy.cannotBeFrozen == true)
                        {
                            __instance.attackPower = 1;
                        }
                        else __instance.attackPower = 3;
                    }
                }
                else
                {
                    //Reset the missile into a missile - and not a card!
                    __instance.gameObject.GetComponent<Animator>().runtimeAnimatorController = missileAnim;
                    __instance.attackPower = 20;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BFFMicroMissile), "Collide", MethodType.Normal)]
        static bool PatchBFFMicroMissileCollide(BFFMicroMissile __instance)
        {
            if (!BFFActive)
            {
                __instance.collision = true;
                FPAudio.PlaySfx(25);
                FPStage.CreateStageObject(Explosion.classID, __instance.position.x, __instance.position.y);

                return false;
            }
            else return true;
        }

        //REMOVE once mod package file is recompiled in modern FP2 version. Fix for 1.0.0 issue which gets transferred onto new builds.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BFFMicroMissile), "State_Done", MethodType.Normal)]
        static void PatchBFFMicroMissileDone(BFFMicroMissile __instance)
        {
            __instance.activationMode = FPActivationMode.NEVER_ACTIVE;
            __instance.gameObject.SetActive(false);

        }

    }
}
