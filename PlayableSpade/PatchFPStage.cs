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
            //Ensure value is reset on stages which lack BFF2000.
            if (FPStage.stageNameString != "Nalao Lake" || FPStage.stageNameString != "The Battlesphere")
            {
                PatchBFFMicroMissile.BFFActive = false;
            }
            //Mark BFF2000 as active in Bakunawa Chase. 
            if (FPStage.stageNameString == "Bakunawa Chase") PatchBFFMicroMissile.BFFActive = true;
        }
    }
}
