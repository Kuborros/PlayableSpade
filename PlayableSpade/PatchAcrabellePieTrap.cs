using HarmonyLib;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchAcrabellePieTrap
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AcrabellePieTrap),"Start")]
        private static void PatchAcrabellePieTrapStart(ref Sprite[] ___characterBase,ref Sprite[] ___characterStruggle)
        {
            if (FPSaveManager.character == (FPCharacterID)5)
            {
                Sprite spadPied = (Sprite)Plugin.moddedBundle.LoadAssetWithSubAssets("Spade_Pie")[1];
                Sprite spadPiedHurt = (Sprite)Plugin.moddedBundle.LoadAssetWithSubAssets("Spade_Pie")[2];

                ___characterBase = ___characterBase.AddToArray(___characterBase[5]);
                ___characterBase[5] = spadPied;
                ___characterStruggle = ___characterStruggle.AddToArray(___characterStruggle[5]);
                ___characterStruggle[5] = spadPiedHurt;
            }
        }
    }
}
