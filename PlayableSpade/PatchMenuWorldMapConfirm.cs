using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace PlayableSpade
{
    internal class PatchMenuWorldMapConfirm
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(MenuWorldMapConfirm), "State_Transition", MethodType.Normal)]
        static IEnumerable<CodeInstruction> MWConfirmTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label spadStart = il.DefineLabel();
            Label spadEnd = il.DefineLabel();

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_2)
                {
                    Label[] targets = (Label[])codes[i].operand;
                    targets = targets.AddItem(spadStart).ToArray();
                    codes[i].operand = targets;
                    codes[i+4].labels.Add(spadEnd);
                }

            }
            CodeInstruction spadCodeStart = new CodeInstruction(OpCodes.Ldloc_1);
            spadCodeStart.labels.Add(spadStart);

            codes.Add(spadCodeStart);
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "Tutorial1")); //Load up Lilac's tutorial
            codes.Add(new CodeInstruction(OpCodes.Br, spadEnd));

            return codes;
        }
    }
}
