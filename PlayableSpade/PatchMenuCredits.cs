using HarmonyLib;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchMenuCredits
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuCredits), "Start", MethodType.Normal)]
        static void PatchMenuCreditsStart(ref AudioClip[] ___bgmCredits, ref Sprite[] ___characterSprites)
        {
            ___bgmCredits = ___bgmCredits.AddToArray(Plugin.moddedBundle.LoadAsset<AudioClip>("M_Theme_Spade"));
            ___characterSprites = ___characterSprites.AddToArray(Plugin.moddedBundle.LoadAsset<Sprite>("Spade_KeyArt"));
        }
    }
}
