using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchFPHudDigit
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPHudDigit), "SetDigitValue", MethodType.Normal)]
        static void PatchFPHudDigitValue(FPHudDigit __instance, ref Sprite[] ___digitFrames)
        {
            if (__instance.name == "PortraitCharacter")
            {
                if (___digitFrames[5] == null)
                {
                    Sprite spadeicon = Plugin.moddedBundle.LoadAsset<Sprite>("spade_portrait");

                    ___digitFrames[5] = spadeicon;
                    ___digitFrames = ___digitFrames.AddToArray(null);
                }
            }
        }
    }
}
