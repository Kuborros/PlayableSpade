using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchFPStage
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPStage), "Start", MethodType.Normal)]
        static void PatchStart(ref FPPlayer[] ___playerList)
        {
            if (FPStage.stageNameString != "Nalao Lake" || FPStage.stageNameString != "The Battlesphere")
            {
                PatchBFFMicroMissile.BFFActive = false;
            }
            if (FPStage.stageNameString == "Bakunawa Chase") PatchBFFMicroMissile.BFFActive = true;
        }
    }
}
