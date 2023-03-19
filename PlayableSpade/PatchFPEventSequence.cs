using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchFPEventSequence
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPEventSequence), "Start", MethodType.Normal)]
        static void PatchStateDefault(FPEventSequence __instance)
        {
            if (FPSaveManager.character == (FPCharacterID)5)
            {
                if (__instance.transform.parent != null && FPStage.stageNameString != "Nalao Lake")
                {
                    Transform cutsceneCarol = __instance.transform.parent.gameObject.transform.Find("Cutscene_Carol");
                    if (cutsceneCarol != null)
                    {
                        if (cutsceneCarol.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Spade Animator Player")
                        {
                            cutsceneCarol.gameObject.GetComponent<Animator>().runtimeAnimatorController = Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("Spade Animator Player");
                            cutsceneCarol.Find("tail").gameObject.SetActive(false);
                        }
                    }
                    Transform cutsceneCarolClassic = __instance.transform.parent.gameObject.transform.Find("Cutscene_Carol_Classic");
                    if (cutsceneCarolClassic != null)
                    {
                        if (cutsceneCarolClassic.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Spade Animator Player")
                        {
                            cutsceneCarolClassic.gameObject.GetComponent<Animator>().runtimeAnimatorController = Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("Spade Animator Player");
                            cutsceneCarolClassic.Find("tail").gameObject.SetActive(false);
                        }
                    }
                }
                if (__instance.transform.parent != null && FPStage.stageNameString == "Merga")
                {
                    Transform eventSequence = __instance.transform.parent.gameObject.transform;
                    if (eventSequence != null)
                    {
                        Transform cutsceneCarol = eventSequence.parent.gameObject.transform.Find("Cutscene_Carol");
                        if (cutsceneCarol != null)
                        {
                            if (cutsceneCarol.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Spade Animator Player")
                            {
                                cutsceneCarol.gameObject.GetComponent<Animator>().runtimeAnimatorController = Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("Spade Animator Player");
                                cutsceneCarol.Find("tail").gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPEventSequence), "State_Event", MethodType.Normal)]
        static void PatchStateEvent(FPEventSequence __instance)
        {
            if (FPSaveManager.character == (FPCharacterID)5)
            {
                if (__instance.transform.parent != null && (FPStage.stageNameString == "Merga"))
                {
                    Transform eventSequence = __instance.transform.parent.gameObject.transform;
                    if (eventSequence != null)
                    {
                        Transform cutsceneCarol = eventSequence.parent.gameObject.transform.Find("Cutscene_Carol");
                        if (cutsceneCarol != null)
                        {
                            __instance.Action_SkipScene();
                        }
                    }
                }
            }
        }


        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FPEventSequence), "Start", MethodType.Normal)]
        static IEnumerable<CodeInstruction> EventSequenceStartTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label carolCheck = il.DefineLabel();

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_S)
                {
                    Label[] targets = (Label[])codes[i].operand;
                    codes[i+8].labels.Add(carolCheck);
                    targets = targets.AddItem(carolCheck).ToArray();
                    codes[i].operand = targets;
                    break;
                }
            }
            return codes;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FPEventSequence), "State_Default", MethodType.Normal)]
        static IEnumerable<CodeInstruction> EventSequenceDefaultTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label carolCheck = il.DefineLabel();
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Switch && (codes[i - 1].opcode == OpCodes.Ldloc_2 || codes[i - 1].opcode == OpCodes.Ldloc_S))
                {
                    Label[] targets = (Label[])codes[i].operand;
                    codes[i + 8].labels.Add(carolCheck);
                    targets = targets.AddItem(carolCheck).ToArray();
                    codes[i].operand = targets;
                }
            }
            return codes;
        }
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FPEventSequence), "State_Event", MethodType.Normal)]
        static IEnumerable<CodeInstruction> EventSequenceEventTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label carolCheck = il.DefineLabel();
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_1)
                {
                    Label[] targets = (Label[])codes[i].operand;
                    codes[i + 8].labels.Add(carolCheck);
                    targets = targets.AddItem(carolCheck).ToArray();
                    codes[i].operand = targets;
                    break;
                }
            }
            return codes;
        }

    }
}
