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

    }
}
