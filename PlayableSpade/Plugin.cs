using BepInEx;
using HarmonyLib;
using System.IO;
using UnityEngine;

namespace PlayableSpade
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
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
        }
    }
}
