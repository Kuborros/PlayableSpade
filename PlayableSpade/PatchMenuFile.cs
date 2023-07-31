using HarmonyLib;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchMenuFile
    {
        private static bool clearReplaced = false;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuFile), "State_Main", MethodType.Normal)]
        static void PatchMenuFileStateMain(MenuFile __instance)
        {
                
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuFile), "GetFileInfo", MethodType.Normal)]
        static void PatchMenuFileInfo(int fileSlot, MenuFile __instance, ref FPHudDigit[] ___characterIcons)
        {
            if (___characterIcons[fileSlot - 1].digitFrames.Length < 8)
            {
                Sprite heart = ___characterIcons[fileSlot - 1].digitFrames[6];
                ___characterIcons[fileSlot - 1].digitFrames[6] = Plugin.moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Stock")[0];
                ___characterIcons[fileSlot - 1].digitFrames = ___characterIcons[fileSlot - 1].digitFrames.AddToArray(heart);
            }
        }
    }
}
