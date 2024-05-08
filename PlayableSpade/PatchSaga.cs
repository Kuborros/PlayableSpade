using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchSaga
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Saga), "Start", MethodType.Normal)]
        private static void PatchSagaStart(ref Animator ___animator, Saga __instance) 
        {
            //If not playing as Spade, no touchie
            if (FPSaveManager.character == (FPCharacterID)5 && __instance != null && ___animator != null)
            {
                RuntimeAnimatorController spadeAnimatorSaga = Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("SagaSpade");
                RuntimeAnimatorController spadeAnimatorSaga2 = Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("Saga2Spade");

                if (__instance.name.Contains("Syntax")) //Code Black ver.
                {
                    ___animator.runtimeAnimatorController = spadeAnimatorSaga2;
                }
                else
                {
                    ___animator.runtimeAnimatorController = spadeAnimatorSaga;
                }
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Saga), "State_Default", MethodType.Normal)]
        static IEnumerable<CodeInstruction> SagaDefaultTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label spadStart = il.DefineLabel();
            Label spadEnd = il.DefineLabel();

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_S)
                {
                    Label[] targets = (Label[])codes[i].operand;
                    targets = targets.AddItem(spadStart).ToArray();
                    codes[i].operand = targets;
                    spadEnd = (Label)codes[i + 1].operand;
                    break;
                }
            }

            CodeInstruction groundCodeStart = new CodeInstruction(OpCodes.Ldarg_0);
            groundCodeStart.labels.Add(spadStart);

            codes.Add(groundCodeStart);
            codes.Add(new CodeInstruction(OpCodes.Ldfld, typeof(Saga).GetField("animator", BindingFlags.NonPublic | BindingFlags.Instance)));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "TrapSpade"));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, typeof(Animator).GetMethod("Play", [typeof(string)])));
            codes.Add(new CodeInstruction(OpCodes.Br, spadEnd));


            return codes;
        }

    }
}
