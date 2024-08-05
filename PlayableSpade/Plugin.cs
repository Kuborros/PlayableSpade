using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using FP2Lib.Badge;
using FP2Lib.Vinyl;
using UnityEngine;

namespace PlayableSpade
{
    [BepInPlugin("com.kuborro.plugins.fp2.playablespade", "PlayableSpade", "0.4.2")]
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

            configInfiniteDash = Config.Bind("Experimental", "FP1 Air Dash", false, "Switches dash to FP1-style one.");


            VinylHandler.RegisterVinyl("kubo.m_clear_spade","Results - Spade",moddedBundle.LoadAsset<AudioClip>("M_Clear_Spade"),VAddToShop.Naomi);
            VinylHandler.RegisterVinyl("kubo.m_theme_spade", "Spade's Theme", moddedBundle.LoadAsset<AudioClip>("M_Theme_Spade"), VAddToShop.Fawnstar);

            BadgeHandler.RegisterBadge("kubo.spaderunner","Red Scarf Runner", "Beat any stage's par time as Spade.", moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Badges")[0], FPBadgeType.SILVER);
            BadgeHandler.RegisterBadge("kubo.spadespeedrunner", "Red Scarf Speedrunner", "Beat any stage as Spade in less than half of the par time.", moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Badges")[1], FPBadgeType.SILVER);
            BadgeHandler.RegisterBadge("kubo.spademaster", "Red Scarf Master", "Beat the par times in all stages as Spade.", moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Badges")[2], FPBadgeType.GOLD);
            BadgeHandler.RegisterBadge("kubo.spadecomplete", "The House Always Wins", "Finish the game as Spade.", moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Badges")[3], FPBadgeType.GOLD);


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
            harmony.PatchAll(typeof(PatchFPHubNPC));
            harmony.PatchAll(typeof(PatchSaga));
            harmony.PatchAll(typeof(PatchAnimatorPreInitializer));
            harmony.PatchAll(typeof(PatchAcrabellePieTrap));
            harmony.PatchAll(typeof(PatchFPSaveManager));
            harmony.PatchAll(typeof(PatchFPResultsMenu));
            harmony.PatchAll(typeof(PatchMenuShop));
        }
    }
}
