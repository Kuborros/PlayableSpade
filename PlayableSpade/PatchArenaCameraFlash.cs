using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchArenaCameraFlash
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ArenaCameraFlash), "Start", MethodType.Normal)]
        static void PatchArenaFlash(ref int[] ___voicePlayerKO)
        {
            if (___voicePlayerKO != null && FPStage.currentStage.GetPlayerInstance_FPPlayer().characterID == (FPCharacterID)5)
            {
                for (int i = 0; i < ___voicePlayerKO.Length; i++)
                {
                    if (___voicePlayerKO[i] == 8) ___voicePlayerKO[i] = 9;
                }
            }   
        }
    }
}
