using HarmonyLib;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchMenuWorldMap
    {

        private static Sprite[] spadeIdle;
        private static Sprite[] spadeWalk;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuWorldMap), "Start", MethodType.Normal)]
        private static void PatchMenuWorldMapStart()
        {
            Sprite[] sprites = Plugin.moddedBundle.LoadAssetWithSubAssets<Sprite>("AdventureMap_Spade");

            spadeIdle = new Sprite[7];

            spadeIdle[0] = sprites[0];
            spadeIdle[1] = sprites[1];
            spadeIdle[2] = sprites[2];
            spadeIdle[3] = sprites[3];
            spadeIdle[4] = sprites[2];
            spadeIdle[5] = sprites[1];
            spadeIdle[6] = sprites[0];

            spadeIdle[0].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeIdle[1].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeIdle[2].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeIdle[3].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeIdle[4].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeIdle[5].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeIdle[6].hideFlags = HideFlags.DontUnloadUnusedAsset;

            spadeWalk = new Sprite[4];

            spadeWalk[0] = sprites[4];
            spadeWalk[1] = sprites[5];
            spadeWalk[2] = sprites[6];
            spadeWalk[3] = sprites[5];

            spadeWalk[0].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeWalk[1].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeWalk[2].hideFlags = HideFlags.DontUnloadUnusedAsset;
            spadeWalk[3].hideFlags = HideFlags.DontUnloadUnusedAsset;

        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuWorldMap), "SetPlayerSprite", MethodType.Normal)]
        private static bool PatchSetPlayerSprite(bool walking, bool ___vehicleMode, ref SpriteRenderer ___playerSpriteRenderer, ref SpriteRenderer ___playerShadowRenderer, FPMap[] ___renderedMap, int ___currentMap, float ___animTimer)
        {
            //Skip if not Spade
            if (FPSaveManager.character != (FPCharacterID)5) return true;


            if (___vehicleMode)
            {
                ___playerSpriteRenderer.sprite = ___renderedMap[___currentMap].vehicle[1];
                if (___renderedMap[___currentMap].waterVehicle)
                {
                    ___playerShadowRenderer.sprite = null;
                    ___playerSpriteRenderer.transform.localPosition = new Vector3(0f, 0f, 0f);
                }
                else
                {
                    ___playerShadowRenderer.sprite = ___playerSpriteRenderer.sprite;
                    ___playerSpriteRenderer.transform.localPosition = new Vector3(0f, Mathf.Sin(0.017453292f * FPStage.platformTimer * 2f) * 4f, 0f);
                }
            }
            else if (walking)
            {
                ___playerSpriteRenderer.sprite = spadeWalk[((int)(___animTimer) % 4)];
                ___playerShadowRenderer.sprite = null;
                ___playerSpriteRenderer.transform.localPosition = new Vector3(0f, 0f, 0f);
            }
            else
            {
                ___playerSpriteRenderer.sprite = spadeIdle[Mathf.Min((int)((___animTimer) % 12),6)];
                ___playerShadowRenderer.sprite = null;
                ___playerSpriteRenderer.transform.localPosition = new Vector3(0f, 0f, 0f);
            }

            return false;

        }
    }
}
