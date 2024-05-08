using HarmonyLib;

namespace PlayableSpade
{
    internal class PatchBakunawaFusion
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Bakunawa), "Activate", MethodType.Normal)]
        static void PatchBakunawaActivate(Bakunawa __instance)
        {
            if (FPSaveManager.character == (FPCharacterID)5)
            {
                __instance.assistRoster.Add(Bakunawa.Assist.Milla);
                __instance.assistRoster.Add(Bakunawa.Assist.Neera);
                __instance.assistRoster.Add(Bakunawa.Assist.Carol);
                __instance.assistRoster.Add(Bakunawa.Assist.Lilac);
                __instance.assistRoster.Add(Bakunawa.Assist.Merga);
            }
        }
    }
}
