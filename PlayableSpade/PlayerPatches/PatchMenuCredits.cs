using HarmonyLib;
using UnityEngine;

namespace PlayableSpade.PlayerPatches
{
    internal class PatchMenuCredits
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(MenuCredits), "Start", MethodType.Normal)]
        static void PatchMenuCreditsStartPre(ref float ___startY, ref float ___currentY, ref FPBaseObject[] ___cast, ref string[] ___castAnimation)
        {
            if (FPSaveManager.character == PlayableSpade.spadeCharID)
            {
                ___startY = -7150f;
                if (___cast != null && ___cast.Length > 18)
                    ___cast[18].GetComponent<Animator>().runtimeAnimatorController = PlayableSpade.moddedBundle.LoadAsset<RuntimeAnimatorController>("Spade Animator Player");
                if (___castAnimation != null && ___castAnimation.Length > 18)
                    ___castAnimation[18] = "Pose6";

                GameObject textCredits = GameObject.Find("ActorName (22)");
                if (textCredits != null)
                {
                    TextMesh textMesh = textCredits.GetComponent<TextMesh>();
                    if (textMesh != null)
                    {
                        textMesh.text += "\r\n\r\nPLAYABLE SPADE MOD\r\n" +
                            "\r\nKubo - Code" +
                            "\r\nVelaCosmos - Sprites" +
                            "\r\n23Lief - Sprites" +
                            "\r\nTreylina - Sprites" +
                            "\r\nCreeperboy90 - Sprites" +
                            "\r\nAlestance - Sprites" +
                            "\r\nFrisktaker - Badge Sprites" +
                            "\r\nFireHeadEngine - Key Art" +
                            "\r\nSleppySpirit - Map Sprites" +
                            "\r\nBobTheGUYYYYY - Music" +
                            "\r\nAnd everyone on the SMA Discord!";
                    }
                }
            }
        }
    }
}
