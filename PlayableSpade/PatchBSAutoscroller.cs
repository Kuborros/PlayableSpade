using HarmonyLib;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchBSAutoscroller
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BSAutoscroller), "Update", MethodType.Normal)]
        static void PatchBSAutoScroller(ref FPHudDigit ___hudDistanceMarker)
        {
            if (___hudDistanceMarker != null)
            {
                if (___hudDistanceMarker.digitFrames.Length < 16)
                {
                    ___hudDistanceMarker.digitFrames = ___hudDistanceMarker.digitFrames.AddToArray(Plugin.moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Stock")[0]);
                }
                if (FPStage.currentStage.GetPlayerInstance_FPPlayer().characterID == (FPCharacterID)5)
                {
                    ___hudDistanceMarker.SetDigitValue(15);
                }
            }
        }
    }
}
