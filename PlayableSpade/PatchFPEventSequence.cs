using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace PlayableSpade
{
    internal class PatchFPEventSequence
    {

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
