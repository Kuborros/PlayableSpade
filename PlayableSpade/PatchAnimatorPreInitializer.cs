using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            spadeInit.animator = Plugin.moddedBundle.LoadAsset<Animator>("");

            AnimatorInitializationClipParams[] clipsToInit = {

                new AnimatorInitializationClipParams(""),
                new AnimatorInitializationClipParams(""),
                new AnimatorInitializationClipParams(""),
                new AnimatorInitializationClipParams(""),
                new AnimatorInitializationClipParams(""),
                new AnimatorInitializationClipParams(""),
                new AnimatorInitializationClipParams("")

            };
            spadeInit.animationClipsToPlay = clipsToInit;

            ___animatorsToInit = ___animatorsToInit.AddToArray(spadeInit);
        }


    }
}
