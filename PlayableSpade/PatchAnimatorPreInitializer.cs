using HarmonyLib;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchAnimatorPreInitializer
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AnimatorPreInitializer), "Start", MethodType.Normal)]
        static void PatchAnimatorPreInit(ref AnimatorInitializationParams[] ___animatorsToInit)
        {
            AnimatorInitializationParams spadeInit = new AnimatorInitializationParams();
            spadeInit.animator = Plugin.moddedBundle.LoadAsset<Animator>("Spade Animator Player");

            AnimatorInitializationClipParams[] clipsToInit = {

                new AnimatorInitializationClipParams("Idle"),
                new AnimatorInitializationClipParams("Running"),
                new AnimatorInitializationClipParams("Rolling"),
                new AnimatorInitializationClipParams("Jumping"),
                new AnimatorInitializationClipParams("Throw"),
                new AnimatorInitializationClipParams("AirThrow"),
                new AnimatorInitializationClipParams("Pose1")

            };
            spadeInit.animationClipsToPlay = clipsToInit;

            ___animatorsToInit = ___animatorsToInit.AddToArray(spadeInit);
        }
    }
}
