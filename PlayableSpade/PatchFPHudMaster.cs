using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchFPHudMaster
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPHudMaster), "LateUpdate", MethodType.Normal)]
        static void PatchHudMasterLateUpdate(FPHudMaster __instance, FPHudDigit[] ___hudLifeIcon, float ___lifeIconBlinkTimer)
        {
            if (__instance.targetPlayer.characterID == (FPCharacterID)5)
            {
                if (___hudLifeIcon[0].digitFrames.Length < 16)
                {
                    Sprite[] spadStock = Plugin.moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Stock");

                    ___hudLifeIcon[0].digitFrames = ___hudLifeIcon[0].digitFrames.AddToArray(spadStock[0]);
                    ___hudLifeIcon[0].digitFrames = ___hudLifeIcon[0].digitFrames.AddToArray(spadStock[1]);
                    ___hudLifeIcon[0].digitFrames = ___hudLifeIcon[0].digitFrames.AddToArray(spadStock[2]);
                    ___hudLifeIcon[0].digitFrames = ___hudLifeIcon[0].digitFrames.AddToArray(spadStock[1]);
                }
                ___hudLifeIcon[0].SetDigitValue(Mathf.Max(15, 15 + (int)___lifeIconBlinkTimer % 3));
            }
        }
    }
}
