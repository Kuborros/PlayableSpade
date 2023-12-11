using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace PlayableSpade
{
    internal class PatchBFWallRunZone
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(BFWallRunZone), "Update", MethodType.Normal)]
        static IEnumerable<CodeInstruction> BFWallRunZoneTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label groundStart = il.DefineLabel();
            Label groundEnd = il.DefineLabel();

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_3)
                {
                    Label[] targets = (Label[])codes[i].operand;
                    targets = targets.AddItem(groundStart).ToArray();
                    codes[i].operand = targets;
                    groundEnd = (Label)codes[i + 1].operand;
                    break;
                }
            }

            CodeInstruction groundCodeStart = new CodeInstruction(OpCodes.Ldarg_0);
            groundCodeStart.labels.Add(groundStart);

            codes.Add(groundCodeStart);
            codes.Add(new CodeInstruction(OpCodes.Call, PatchFPPlayer.m_GroundMoves));
            codes.Add(new CodeInstruction(OpCodes.Br, groundEnd));


            return codes;
        }
    }
}
