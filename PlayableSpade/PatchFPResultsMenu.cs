using FP2Lib.Badge;
using HarmonyLib;

namespace PlayableSpade
{
    internal class PatchFPResultsMenu
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPResultsMenu), "Update", MethodType.Normal)]
        private static void PatchResultsUpdate(float ___badgeCheckTimer)
        {
            if (___badgeCheckTimer < 61f && !FPStage.currentStage.disableBadgeChecks && FPSaveManager.character == (FPCharacterID)5)
            {
                if ((___badgeCheckTimer + FPStage.deltaTime) >= 60f)
                {
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.spaderunner"].id);
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.spadespeedrunner"].id);
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.spademaster"].id);
                }
            }
        }
    }
}
