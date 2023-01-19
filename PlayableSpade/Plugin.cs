using BepInEx;
using HarmonyLib;
using System.IO;
using UnityEngine;

namespace PlayableSpade
{
    [BepInPlugin("com.kuborro.plugins.fp2.playablespade", "PlayableSpade", "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static AssetBundle moddedBundle;
        private void Awake()
        {
            string assetPath = Path.Combine(Path.GetFullPath("."), "mod_overrides");
            moddedBundle = AssetBundle.LoadFromFile(Path.Combine(assetPath, "playablespade.assets"));
            if (moddedBundle == null)
            {
                Logger.LogError("Failed to load AssetBundle! Mod cannot work without it, exiting. Please reinstall it.");
                return;
            }

            var harmony = new Harmony("com.kuborro.plugins.fp2.playablespade");
            harmony.PatchAll(typeof(PatchFPPlayer));
            harmony.PatchAll(typeof(PatchItemFuel));
            harmony.PatchAll(typeof(PatchPlayerSpawnPoint));
            harmony.PatchAll(typeof(PatchFPStage));
            harmony.PatchAll(typeof(PatchMenuFile));
            harmony.PatchAll(typeof(PatchFPEventSequence));
            harmony.PatchAll(typeof(PatchParentActivator));
            harmony.PatchAll(typeof(PatchFPBreakable));
            harmony.PatchAll(typeof(PatchMenuCharacterSelect));
            harmony.PatchAll(typeof(PatchMenuPhoto));
            harmony.PatchAll(typeof(PatchBFFCombiner));
            harmony.PatchAll(typeof(PatchFPHudDigit));
            harmony.PatchAll(typeof(PatchFPHudMaster));
            harmony.PatchAll(typeof(PatchSBBeaconCutscene));
            harmony.PatchAll(typeof(PatchBakunawaFusion));
            harmony.PatchAll(typeof(PatchBFFMicroMissile));
            harmony.PatchAll(typeof(PatchPlayerBFF));
        }
    }
}
