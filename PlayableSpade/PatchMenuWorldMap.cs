using FP2Lib.Badge;
using HarmonyLib;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchMenuWorldMap
    {

        private static Sprite[] spadeIdle;
        private static Sprite[] spadeWalk;
        private static MenuWorldMap currInstance;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuWorldMap), "Start", MethodType.Normal)]
        private static void PatchMenuWorldMapStart()
        {
            Sprite[] sprites = Plugin.moddedBundle.LoadAssetWithSubAssets<Sprite>("AdventureMap_Spade");

            spadeIdle = new Sprite[7];

            spadeIdle[0] = sprites[0];
            spadeIdle[1] = sprites[1];
            spadeIdle[2] = sprites[2];
            spadeIdle[3] = sprites[3];
            spadeIdle[4] = sprites[2];
            spadeIdle[5] = sprites[1];
            spadeIdle[6] = sprites[0];

            spadeIdle[0].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeIdle[1].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeIdle[2].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeIdle[3].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeIdle[4].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeIdle[5].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeIdle[6].hideFlags = HideFlags.DontUnloadUnusedAsset;

            spadeWalk = new Sprite[4];

            spadeWalk[0] = sprites[4];
            spadeWalk[1] = sprites[5];
            spadeWalk[2] = sprites[6];
            spadeWalk[3] = sprites[5];

            spadeWalk[0].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeWalk[1].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeWalk[2].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeWalk[3].hideFlags = HideFlags.DontUnloadUnusedAsset;

        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuWorldMap), "SetPlayerSprite", MethodType.Normal)]
        private static bool PatchSetPlayerSprite(bool walking, bool ___vehicleMode, ref SpriteRenderer ___playerSpriteRenderer, ref SpriteRenderer ___playerShadowRenderer, FPMap[] ___renderedMap, int ___currentMap, float ___animTimer)
        {
            //Skip if not Spade
            if (FPSaveManager.character != (FPCharacterID)5) return true;


            if (___vehicleMode)
            {
                ___playerSpriteRenderer.sprite = ___renderedMap[___currentMap].vehicle[1];
                if (___renderedMap[___currentMap].waterVehicle)
                {
                    ___playerShadowRenderer.sprite = null;
                    ___playerSpriteRenderer.transform.localPosition = new Vector3(0f, 0f, 0f);
                }
                else
                {
                    ___playerShadowRenderer.sprite = ___playerSpriteRenderer.sprite;
                    ___playerSpriteRenderer.transform.localPosition = new Vector3(0f, Mathf.Sin(0.017453292f * FPStage.platformTimer * 2f) * 4f, 0f);
                }
            }
            else if (walking)
            {
                ___playerSpriteRenderer.sprite = spadeWalk[((int)(___animTimer) % 4)];
                ___playerShadowRenderer.sprite = null;
                ___playerSpriteRenderer.transform.localPosition = new Vector3(0f, 0f, 0f);
            }
            else
            {
                ___playerSpriteRenderer.sprite = spadeIdle[Mathf.Min((int)((___animTimer) % 12), 6)];
                ___playerShadowRenderer.sprite = null;
                ___playerSpriteRenderer.transform.localPosition = new Vector3(0f, 0f, 0f);
            }

            return false;
        }


        internal static void State_WaitForMenu()
        {
            State_WaitForMenu(currInstance);
        }

        [HarmonyReversePatch(0)]
        [HarmonyPatch(typeof(MenuWorldMap), "State_WaitForMenu", MethodType.Normal)]
        public static void State_WaitForMenu(MenuWorldMap instance)
        {
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuWorldMap), "State_Default", MethodType.Normal)]
        private static void PatchStateDefault(bool ___cutsceneCheck, float ___badgeCheckTimer)
        {
            if (___cutsceneCheck && ___badgeCheckTimer > 0f && ___badgeCheckTimer < 26f && FPSaveManager.character == (FPCharacterID)5)
            {
                if ((___badgeCheckTimer + FPStage.deltaTime) >= 25f)
                {
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.spademaster"].id);
                }
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuWorldMap), "CutsceneCheck", MethodType.Normal)]
        private static bool PatchCutsceneCheck(MenuWorldMap __instance, ref bool ___cutsceneCheck, ref float ___badgeCheckTimer, ref GameObject ___targetMenu)
        {
            if (__instance.cutscenes.Length != 0 && FPSaveManager.character == (FPCharacterID)5)
            {
                currInstance = __instance;
                for (int i = 0; i < __instance.cutscenes.Length; i++)
                {
                    if (__instance.cutscenes[i].requiredStoryFlags.Length != 0)
                    {
                        int num = __instance.cutscenes[i].requiredStoryFlags.Length;
                        for (int j = 0; j < __instance.cutscenes[i].requiredStoryFlags.Length; j++)
                        {
                            if (FPSaveManager.storyFlag[__instance.cutscenes[i].requiredStoryFlags[j]] > 0)
                            {
                                num--;
                            }
                        }
                        for (int k = 0; k < __instance.cutscenes[i].deactivateAtFlag.Length; k++)
                        {
                            if (FPSaveManager.storyFlag[__instance.cutscenes[i].deactivateAtFlag[k]] > 0)
                            {
                                num = 99;
                            }
                        }
                        if (__instance.cutscenes[i].requiredMap >= 0 && FPSaveManager.lastMap != __instance.cutscenes[i].requiredMap)
                        {
                            num = 99;
                        }
                        if (__instance.cutscenes[i].requiredLocation >= 0 && FPSaveManager.lastMapLocation != __instance.cutscenes[i].requiredLocation)
                        {
                            num = 99;
                        }
                        bool flag = false;
                        for (int l = 0; l < __instance.cutscenes[i].dialogSequence.Length; l++)
                        {
                            if (FPSaveManager.character == (FPCharacterID)5 && __instance.cutscenes[i].dialogSequence[l].characters[1])
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            num = 99;
                        }
                        if (num <= 0)
                        {
                            CutsceneDialog cutsceneDialog = UnityEngine.Object.Instantiate<CutsceneDialog>(__instance.menuCutscene);
                            cutsceneDialog.currentScene = __instance.cutscenes[i].sceneID;
                            cutsceneDialog.dialogSystem = __instance.dialogSystem;
                            cutsceneDialog.dialogSequence = __instance.cutscenes[i].dialogSequence;
                            ___targetMenu = cutsceneDialog.gameObject;
                            __instance.state = new FPObjectState(State_WaitForMenu);
                        }
                        else
                        {
                            ___badgeCheckTimer = 1f;
                        }
                    }
                }
                ___cutsceneCheck = true;
                return false;
            }
            return true;
        }


    }
}