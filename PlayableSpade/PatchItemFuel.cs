using HarmonyLib;
using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchItemFuel
    {
        static readonly MethodInfo m_FuelCollide = SymbolExtensions.GetMethodInfo(() => SpadeFuelCollision());
        public static void SpadeFuelCollision() 
        {
            foreach (FPPlayer player in FPStage.player)
            {
                player.invincibilityTime = Mathf.Max(player.invincibilityTime, 120f);
                player.flashTime = Mathf.Max(player.flashTime, 240f);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemFuel), "Start", MethodType.Normal)]
        static void PatchStart(ref Sprite[] ___iconSprite)
        {
            ___iconSprite = ___iconSprite.AddToArray(___iconSprite[3]);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ItemFuel), "CollisionCheck", MethodType.Normal)]
        static IEnumerable<CodeInstruction> ItemFuelTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_2)
                {
                    Label[] targets = (Label[])codes[i].operand;
                    targets = targets.AddItem(targets[2]).ToArray();
                    codes[i].operand = targets;
                }

            }
            return codes;
        }

    }
}
