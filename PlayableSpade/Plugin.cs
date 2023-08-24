using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using UnityEngine;

namespace PlayableSpade
{
    [BepInPlugin("com.kuborro.plugins.fp2.playablespade", "PlayableSpade", "0.3.3")]
    public class Plugin : BaseUnityPlugin
    {
        public static AssetBundle moddedBundle;

        public static ConfigEntry<bool> configInfiniteDash;

        internal static ManualLogSource logSource;

        private void Awake()
        {
            logSource = Logger;

            string assetPath = Path.Combine(Path.GetFullPath("."), "mod_overrides");
            moddedBundle = AssetBundle.LoadFromFile(Path.Combine(assetPath, "playablespade.assets"));
            if (moddedBundle == null)
            {
                logSource.LogError("Failed to load AssetBundle! Mod cannot work without it, exiting. Please reinstall it.");
                return;
            }

            configInfiniteDash = Config.Bind("Experimental", "Infinite Air Dash", false, "Allows for infinite air dashes. Affects the balance of the mod.");

            var harmony = new Harmony("com.kuborro.plugins.fp2.playablespade");
            harmony.PatchAll(typeof(PatchFPPlayer));
            harmony.PatchAll(typeof(PatchItemFuel));
            harmony.PatchAll(typeof(PatchPlayerSpawnPoint));
            harmony.PatchAll(typeof(PatchFPStage));
            harmony.PatchAll(typeof(PatchMenuFile));
            harmony.PatchAll(typeof(PatchFPEventSequence));
            harmony.PatchAll(typeof(PatchParentActivator));
            harmony.PatchAll(typeof(PatchArenaRace));
            harmony.PatchAll(typeof(PatchMenuCharacterSelect));
            harmony.PatchAll(typeof(PatchMenuCharacterWheel));
            harmony.PatchAll(typeof(PatchMenuTutorialPrompt));
            harmony.PatchAll(typeof(PatchMenuGlobalPause));
            harmony.PatchAll(typeof(PatchMenuPhoto));
            harmony.PatchAll(typeof(PatchBFFCombiner));
            harmony.PatchAll(typeof(PatchFPHudDigit));
            harmony.PatchAll(typeof(PatchFPHudMaster));
            harmony.PatchAll(typeof(PatchSBBeaconCutscene));
            harmony.PatchAll(typeof(PatchBakunawaFusion));
            harmony.PatchAll(typeof(PatchBFFMicroMissile));
            harmony.PatchAll(typeof(PatchPlayerBFF));
            harmony.PatchAll(typeof(PatchGO3DColumn));
            harmony.PatchAll(typeof(PatchBFWallRunZone));
            harmony.PatchAll(typeof(PatchBSAutoscroller));
            harmony.PatchAll(typeof(PatchArenaCameraFlash));
            harmony.PatchAll(typeof(PatchMenuCredits));
            harmony.PatchAll(typeof(PatchLTNodePlayerBridge));
            harmony.PatchAll(typeof(PatchMenuWorldMap));
            harmony.PatchAll(typeof(PatchItemStarCard));
            harmony.PatchAll(typeof(PatchFPBossHud));
            harmony.PatchAll(typeof(PatchMenuWorldMapConfirm));
        }
    }
}
