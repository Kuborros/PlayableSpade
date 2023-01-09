using HarmonyLib;
using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchFPStage
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPStage), "Start", MethodType.Normal)]
        static void PatchStart(ref FPPlayer[] ___playerList)
        {
            GameObject spadeObject = Plugin.moddedBundle.LoadAsset<GameObject>("Player Spade");
            ___playerList = ___playerList.AddItem(spadeObject.GetComponent<FPPlayer>()).ToArray();
        }
    }
}
