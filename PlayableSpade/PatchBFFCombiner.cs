using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace PlayableSpade
{
    internal class PatchBFFCombiner
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(BFFCombinerCutscene), "Update", MethodType.Normal)]
        static IEnumerable<CodeInstruction> EventSequenceDefaultTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label carolCheck = il.DefineLabel();
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_S)
                {
                    Label[] targets = (Label[])codes[i].operand;
                    codes[i + 9].labels.Add(carolCheck);
                    targets = targets.AddItem(carolCheck).ToArray();
                    codes[i].operand = targets;
                }
            }
            return codes;
        }


    }
}
