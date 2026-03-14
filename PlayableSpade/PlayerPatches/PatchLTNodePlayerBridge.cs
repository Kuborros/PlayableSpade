using HarmonyLib;

namespace PlayableSpade.PlayerPatches
{
    internal class PatchLTNodePlayerBridge
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LTNodePlayerBridge), "Action_Dismount", MethodType.Normal)]
        static void PatchPlayerBridge()
        {
            PatchFPPlayer.upDash = true;
        }
    }
}
