using HarmonyLib;

namespace PlayableSpade
{
    internal class PatchPlayerBFF
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerBFF2000), "StartupSequence", MethodType.Normal)]
        static void PatchBFFStartup()
        {
            PatchBFFMicroMissile.BFFActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerBFF2000), "State_Victory", MethodType.Normal)]
        static void PatchBFFVictory()
        {
            PatchBFFMicroMissile.BFFActive = false;
        }
    }
}
