using HarmonyLib;
using UnityEngine.SceneManagement;

namespace PlayableSpade
{
    internal class PatchArenaCameraFlash
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ArenaCameraFlash), "Start", MethodType.Normal)]
        static void PatchArenaFlash(ref int[] ___voicePlayerKO)
        {
            if (SceneManager.GetActiveScene().name != "Cutscene_BattlesphereEnding")
            {
                if (___voicePlayerKO != null && FPStage.currentStage.GetPlayerInstance_FPPlayer().characterID == (FPCharacterID)5)
                {
                    for (int i = 0; i < ___voicePlayerKO.Length; i++)
                    {
                        if (___voicePlayerKO[i] == 8) ___voicePlayerKO[i] = 9;
                    }
                }
            }
        }
    }
}
