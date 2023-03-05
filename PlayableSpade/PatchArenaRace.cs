﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchArenaRace
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ArenaRace), "Start", MethodType.Normal)]
        static void PatchArenaRaceStart(ArenaRace __instance, ref FPHudDigit ___hudDistanceMarker)
        {
            if (FPStage.currentStage.GetPlayerInstance_FPPlayer().characterID == (FPCharacterID)5 && ___hudDistanceMarker.digitFrames.Length < 16)
            {
                ___hudDistanceMarker.digitFrames = ___hudDistanceMarker.digitFrames.AddToArray(Plugin.moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Stock")[0]);
                ___hudDistanceMarker.SetDigitValue(16);
            }
        }
    }
}
