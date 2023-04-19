﻿using HarmonyLib;

namespace PlayableSpade
{
    internal class PatchMenuTutorialPrompt
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuTutorialPrompt), "State_Transition", MethodType.Normal)]
        static void PatchTutorialTransition(MenuTutorialPrompt __instance, ref bool ___goingToTutorial)
        {
            if (FPSaveManager.character == (FPCharacterID)5)
            {
                //For now simply always skip tutorial
                ___goingToTutorial = false;
            }
        }

    }
}
