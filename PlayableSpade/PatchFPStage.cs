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
            GameObject[] spadeObject = Plugin.moddedBundle.LoadAllAssets<GameObject>();
            ___playerList = ___playerList.AddItem(spadeObject[0].GetComponent<FPPlayer>()).ToArray();
        }
    }
}
