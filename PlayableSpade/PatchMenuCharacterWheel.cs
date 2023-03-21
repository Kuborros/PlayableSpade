using HarmonyLib;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchMenuCharacterWheel
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuCharacterWheel), "Update", MethodType.Normal)]
        static bool ReplaceMenuCharacterUpdate(MenuCharacterWheel __instance, ref SpriteRenderer ___spriteRenderer)
        {

            if (__instance != null)
            {
                switch (__instance.name)
                {
                    case "Menu CS Character Milla":
                        __instance.rotationOffset = 44;
                        break;
                    case "Menu CS Character Carol":
                        __instance.rotationOffset = 108;
                        break;
                    case "Menu CS Character Lilac":
                        __instance.rotationOffset = 180;
                        break;
                    case "Menu CS Character Spade(Clone)":
                        __instance.rotationOffset = 252;
                        break;
                    case "Menu CS Character Neera":
                        __instance.rotationOffset = 324;
                        break;
                    default: break;
                }
            }

            float num = 5f * FPStage.frameScale;
            __instance.transform.position = new Vector3(__instance.startPosition.x + Mathf.Sin(0.017453292f * __instance.rotation) * __instance.distance.x, __instance.startPosition.y + Mathf.Cos(0.017453292f * __instance.rotation) * __instance.distance.y, Mathf.Cos(0.017453292f * __instance.rotation) * 5f + 5f);
            float z = __instance.transform.position.z;
            ___spriteRenderer.color = new Color(1f - z * 0.15f, 1f - z * 0.15f, 1f - z * 0.1f, 1f);
            if (__instance.parentObject != null)
            {
                __instance.rotation = (__instance.rotation * (num - 1f) + (float)__instance.parentObject.character * 72f + __instance.rotationOffset) / num;
            }

            return false;
        }
    }
}
