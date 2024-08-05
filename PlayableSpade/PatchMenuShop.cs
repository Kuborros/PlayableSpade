using HarmonyLib;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchMenuShop
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuShop), "Start", MethodType.Normal)]
        private static void PatchMenuShopStart(ref MenuText ___playerName, SpriteRenderer ___playerSprite)
        {
            if (FPSaveManager.character == (FPCharacterID)5)
            {
                ___playerName.GetComponent<TextMesh>().text = "Spade";
                ___playerSprite.sprite = Plugin.moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Stock")[0];
            }
        }
    }
}
