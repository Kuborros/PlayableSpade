using HarmonyLib;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchMenuGlobalPause
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuGlobalPause),"Start",MethodType.Normal)]
        static void PatchMenuGlobalPauseStart(ref MenuGlobalPause __instance)
        {
            if (FPSaveManager.character == (FPCharacterID)5)
            {
                int npcnumber = FPSaveManager.GetNPCNumber("Spade");
                FPSaveManager.npcFlag[npcnumber] = (byte)Mathf.Max(1, FPSaveManager.npcFlag[npcnumber]);
                Sprite spadSprite = Plugin.moddedBundle.LoadAsset<Sprite>("spade_pause");
                __instance.playerSprites = __instance.playerSprites.AddToArray(spadSprite);
                __instance.playerInfoSprite.sprite = __instance.playerSprites[4];
                __instance.playerInfoName.GetComponent<TextMesh>().text = "Spade";


                __instance.menuOptions[2].locked = true;

            }
        }
    }
}
