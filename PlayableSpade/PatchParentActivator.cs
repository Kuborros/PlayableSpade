using HarmonyLib;

namespace PlayableSpade
{
    internal class PatchParentActivator
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ParentActivator), "Start", MethodType.Normal)]
        static void PatchPAStart(ParentActivator __instance)
        {
            if (FPSaveManager.character == (FPCharacterID)5 && !__instance.carol)
            {
                if (__instance.destroyIfUnqualified)
                {
                    FPStage.DestroyStageObject(__instance);
                }
                else
                {
                    __instance.activationMode = FPActivationMode.NEVER_ACTIVE;
                }
            }
        }
    }
}
