using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchFPHudMaster
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPHudMaster), "Start", MethodType.Normal)]
        static void PatchHudMasterStart(ref FPHudDigit ___hudRevive)
        {
            if (___hudRevive.gameObject.GetComponents<FPHudDigit>().Length != 0)
            {
                foreach (FPHudDigit digit in ___hudRevive.gameObject.GetComponentsInChildren<FPHudDigit>())
                {
                    if (digit.name == "Hud Life Icon" && digit.digitFrames.Length <= 6)
                    {
                        Sprite[] spadStock = Plugin.moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Stock");
                        digit.digitFrames = digit.digitFrames.AddItem(digit.digitFrames[5]).ToArray();
                        digit.digitFrames[5] = spadStock[0];
                    }
                }
            }
        }

            [HarmonyPrefix]
        [HarmonyPatch(typeof(FPHudMaster), "LateUpdate", MethodType.Normal)]
        static void PatchHudMasterLateUpdate(FPHudMaster __instance, FPHudDigit[] ___hudLifeIcon, float ___lifeIconBlinkTimer)
        {
            if (__instance.targetPlayer.characterID == (FPCharacterID)5)
            {
                if (___hudLifeIcon[0].digitFrames.Length < 16)
                {
                    Sprite[] spadStock = Plugin.moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Stock");

                    ___hudLifeIcon[0].digitFrames = ___hudLifeIcon[0].digitFrames.AddToArray(spadStock[0]);
                    ___hudLifeIcon[0].digitFrames = ___hudLifeIcon[0].digitFrames.AddToArray(spadStock[1]);
                    ___hudLifeIcon[0].digitFrames = ___hudLifeIcon[0].digitFrames.AddToArray(spadStock[2]);
                    ___hudLifeIcon[0].digitFrames = ___hudLifeIcon[0].digitFrames.AddToArray(spadStock[1]);
                }
                ___hudLifeIcon[0].SetDigitValue(Mathf.Max(15, 15 + (int)___lifeIconBlinkTimer % 3));
            }
        }
    }
}
