using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchMenuCharacterSelect
    {

        static GameObject spadeSelector;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuCharacterSelect), "Start", MethodType.Normal)]
        static void PatchCharacterSelectStart(MenuCharacterSelect __instance, ref MenuCharacterWheel[] ___characterSprites, ref Sprite[] ___nameLabelSprites, ref MenuText[] ___infoText)
        {
            spadeSelector = GameObject.Instantiate(Plugin.moddedBundle.LoadAsset<GameObject>("Menu CS Character Spade"));
            MenuCharacterWheel spadeWheel = spadeSelector.GetComponent<MenuCharacterWheel>();
            spadeWheel.parentObject = __instance;
            spadeWheel.gameObject.transform.parent = __instance.transform;
            ___characterSprites = ___characterSprites.AddToArray(spadeWheel);

            ___nameLabelSprites = ___nameLabelSprites.AddToArray(Plugin.moddedBundle.LoadAsset<Sprite>("Spade_Name"));

            ___infoText[0].paragraph = ___infoText[0].paragraph.AddToArray("MODDED Type");
            ___infoText[1].paragraph = ___infoText[1].paragraph.AddToArray("Card Special");
            ___infoText[2].paragraph = ___infoText[2].paragraph.AddToArray("Jump");
            ___infoText[3].paragraph = ___infoText[3].paragraph.AddToArray("Card Throw");
            ___infoText[4].paragraph = ___infoText[4].paragraph.AddToArray("Dodge Dash");

        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(MenuCharacterSelect), "State_SelectCharacter", MethodType.Normal)]
        static IEnumerable<CodeInstruction> CharacterSelectTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_3)
                {
                    codes[i].opcode = OpCodes.Ldc_I4_4;
                }
            }
            return codes;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuCharacterSelect), "State_CharacterConfirm", MethodType.Normal)]
        static void PatchCharacterConfirm(MenuCharacterSelect __instance, ref FPHudDigit ___characterIcon)
        {
            if (__instance.character == 4)
            {
                ___characterIcon.digitFrames = ___characterIcon.digitFrames.AddToArray(Plugin.moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Stock")[0]);
                ___characterIcon.SetDigitValue(16);
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(MenuCharacterSelect), "State_CharacterConfirm", MethodType.Normal)]
        static IEnumerable<CodeInstruction> CharacterConfirmTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label spadeSelect = il.DefineLabel();
            Label spadeBr = il.DefineLabel();

            System.Object staticVal = null;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                //Save file character id write
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_S)
                {
                    Label[] targets = (Label[])codes[i].operand;
                    staticVal = codes[i + 3].operand;
                    codes[i + 1].labels.Add(spadeBr);
                    targets = targets.AddItem(spadeSelect).ToArray();
                    codes[i].operand = targets;
                }

            }

            //SaveFile magic
            CodeInstruction idCodeStart = new CodeInstruction(OpCodes.Ldc_I4_5);
            idCodeStart.labels.Add(spadeSelect);

            codes.Add(idCodeStart);
            codes.Add(new CodeInstruction(OpCodes.Stsfld, staticVal));
            codes.Add(new CodeInstruction(OpCodes.Br, spadeBr));

            return codes;


        }
    }
}
