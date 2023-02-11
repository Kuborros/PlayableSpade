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
        static void PatchHudMasterLateUpdate(FPHudMaster __instance, FPHudDigit[] ___hudLifeIcon)
        {
            if (__instance.targetPlayer.characterID == (FPCharacterID)5)
            {
                if (___hudLifeIcon[0].digitFrames.Length < 16)
                {
                    ___hudLifeIcon[0].digitFrames = ___hudLifeIcon[0].digitFrames.AddToArray(Plugin.moddedBundle.LoadAsset<Sprite>("SpadStock"));
                    //___hudLifeIcon[0].digitFrames = ___hudLifeIcon[0].digitFrames.AddToArray(Plugin.moddedBundle.LoadAsset<Sprite>("SpadeStock1"));
                    //___hudLifeIcon[0].digitFrames = ___hudLifeIcon[0].digitFrames.AddToArray(Plugin.moddedBundle.LoadAsset<Sprite>("SpadeStock2"));
                    //___hudLifeIcon[0].digitFrames = ___hudLifeIcon[0].digitFrames.AddToArray(Plugin.moddedBundle.LoadAsset<Sprite>("SpadeStock3"));
                }
                ___hudLifeIcon[0].SetDigitValue(16);
            }
        }
    }
}
