using HarmonyLib;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchPlayerSpawnPoint
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerSpawnPoint), "Start", MethodType.Normal)]
        static void PatchSpawnPointStart(ref AudioClip[] ___characterMusic)
        {
            ___characterMusic = ___characterMusic.AddToArray(null);
        }
    }
}
