using HarmonyLib;

namespace PlayableSpade
{
    internal class PatchItemStarCard
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemStarCard), "Start", MethodType.Normal)]
        static void PatchItemCardStart(ItemStarCard __instance)
        {
            //Ensure Carol's Star Card does not spawn for Spade.
            if (FPSaveManager.character == PlayableSpade.spadeCharID)
            {
                if (FPStage.stageNameString == "Airship Sigwada" && __instance.disableForCharacter.Length > 0)
                {
                    __instance.disableForCharacter = __instance.disableForCharacter.AddToArray(PlayableSpade.spadeCharID);
                }
            }
        }
    }
}
