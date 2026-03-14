using HarmonyLib;

namespace PlayableSpade.PlayerPatches
{
    internal class PatchGO3DColumn
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GO3DColumn), "Action_Dismount", MethodType.Normal)]
        static void Patch3DColumnDismount()
        {
            PatchFPPlayer.upDash = true;
        }
    }
}
