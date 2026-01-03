using HarmonyLib;

namespace PlayableSpade
{
    internal class PatchPlayerShip
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerShip), "Start", MethodType.Normal)]
        static void PatchShipStart()
        {
            PatchBFFMicroMissile.BFFActive = true;
        }
    }
}
