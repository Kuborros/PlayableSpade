using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchMenuFile
    {
        private static bool clearReplaced = false;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuFile), "State_Main", MethodType.Normal)]
        static void PatchMenuFileStateMain(MenuFile __instance)
        {
            if (!clearReplaced)
            {
                foreach (MenuFilePanel file in __instance.files)
                {
                    FPHudDigit[] elements = file.gameObject.GetComponentsInChildren<FPHudDigit>();
                    foreach (FPHudDigit element in elements)
                    {
                        if (element.digitValue == 5 && element.digitFrames.Length == 6)
                        {
                            GameObject[] objects = file.gameObject.GetComponentsInChildren<GameObject>();
                            foreach (GameObject gameObject in objects)
                            {
                                if (gameObject.name == "Clear")
                                {
                                    SuperTextMesh stm = gameObject.GetComponent<SuperTextMesh>();
                                    stm.text = "Modded";
                                    stm.Text = "Modded";
                                }
                                if (gameObject.name == "Shadow")
                                {
                                    SuperTextMesh stm = gameObject.GetComponent<SuperTextMesh>();
                                    if (stm.text == "Clear")
                                    {
                                        stm.text = "Modded";
                                        stm.Text = "Modded";
                                    }
                                }
                                clearReplaced = true;
                            }
                        }
                    }
                }
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPHudDigit), "SetDigitValue", MethodType.Normal)]
        static void PatchFPHudDigit(FPHudDigit __instance, ref Sprite[] ___digitFrames) 
        {
            if (__instance.name == "PortraitCharacter")
            {
                if (___digitFrames[5] == null) {
                    Sprite spadeicon = Plugin.moddedBundle.LoadAsset<Sprite>("spade_portrait");

                    ___digitFrames[5] = spadeicon;
                    ___digitFrames = ___digitFrames.AddToArray(null);
                }
            }
        }

    }
}
