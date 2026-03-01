using HarmonyLib;
using UnityEngine;

namespace PlayableSpade.PlayerPatches
{
    internal class PatchBFSyntaxHunt
    {
        [HarmonyPostfix]
        [HarmonyPatch]
        static void PatchBSSyntaxHuntStart(ref GameObject ___playerBody)
        {
            if (FPSaveManager.character == PlayableSpade.spadeCharID)
            {
                GameObject carolDed = ___playerBody.transform.GetChild(1).gameObject;
                carolDed.GetComponent<SpriteRenderer>().sprite = PlayableSpade.moddedBundle.LoadAsset<Sprite>("spade_ko_0");
            }
        }
    }
}
