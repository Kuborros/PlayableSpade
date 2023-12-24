
using HarmonyLib;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchFPHubNPC
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPHubNPC), "Start", MethodType.Normal)]
        static void PatchHubNPCStart(string ___NPCName, ref NPCDialog[] ___dialog, ref int[] ___sortedPriorityList, ref GameObject[] ___shopMenu)
        {
            if (___NPCName == "Ying")
            {
                GameObject spad = Plugin.moddedBundle.LoadAsset<GameObject>("SketchMenuSpad");
                ___shopMenu = ___shopMenu.AddToArray(spad);

                NPCDialog spadoodleDialog = ___dialog[1];
                spadoodleDialog.description = "Spadoodle";
                spadoodleDialog.lines = [new NPCDialogLine { text = "<j>OBSERVE</j>, my greatest work of art yet!", pose = "Pose2",jumpToLine = -5, options = new NPCDialogOption[0] }];
                ___dialog[1] = spadoodleDialog;
            }
        }
    }
}
