using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using FP2Lib.Badge;
using FP2Lib.Vinyl;
using UnityEngine;
using FP2Lib.Player;
using System;
using UnityEngine.SceneManagement;

namespace PlayableSpade
{
    [BepInPlugin("com.kuborro.plugins.fp2.playablespade", "PlayableSpade", "0.5.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static AssetBundle moddedBundle;
        public static AssetBundle tutorialScene;

        public static ConfigEntry<bool> configInfiniteDash;

        internal static ManualLogSource logSource;

        private void Awake()
        {
            logSource = Logger;

            string assetPath = Path.Combine(Path.GetFullPath("."), "mod_overrides");
            moddedBundle = AssetBundle.LoadFromFile(Path.Combine(assetPath, "playablespade.assets"));
            tutorialScene = AssetBundle.LoadFromFile(Path.Combine(assetPath, "playablespade.scene"));

            if (moddedBundle == null || tutorialScene == null)
            {
                logSource.LogError("Failed to load AssetBundle! Mod cannot work without it, exiting. Please reinstall it.");
                return;
            }

            configInfiniteDash = Config.Bind("Experimental", "FP1 Air Dash", false, "Switches dash to FP1-style one.");

            //Initialise music
            AudioClip spadeClear = moddedBundle.LoadAsset<AudioClip>("M_Clear_Spade");
            AudioClip spadeTheme = moddedBundle.LoadAsset<AudioClip>("M_Theme_Spade");

            //Add Vinyls
            VinylHandler.RegisterVinyl("kubo.m_clear_spade","Results - Spade",spadeClear,VAddToShop.Naomi);
            VinylHandler.RegisterVinyl("kubo.m_theme_spade", "Spade's Theme",spadeTheme, VAddToShop.Fawnstar);

            //Add Badges
            BadgeHandler.RegisterBadge("kubo.spaderunner","Red Scarf Runner", "Beat any stage's par time as Spade.", moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Badges")[0], FPBadgeType.SILVER);
            BadgeHandler.RegisterBadge("kubo.spadespeedrunner", "Red Scarf Speedrunner", "Beat any stage as Spade in less than half of the par time.", moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Badges")[1], FPBadgeType.SILVER);
            BadgeHandler.RegisterBadge("kubo.spademaster", "Red Scarf Master", "Beat the par times in all stages as Spade.", moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Badges")[2], FPBadgeType.GOLD);
            BadgeHandler.RegisterBadge("kubo.spadecomplete", "The House Always Wins", "Finish the game as Spade.", moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Badges")[3], FPBadgeType.GOLD);

            //Init character select sprite
            GameObject spadeWheel = moddedBundle.LoadAsset<GameObject>("Menu CS Character Spade");

            //Initialise map sprites
            Sprite[] mapsprites = moddedBundle.LoadAssetWithSubAssets<Sprite>("AdventureMap_Spade");

            Sprite[] spadeIdle;
            Sprite[] spadeWalk;

            spadeIdle = new Sprite[7];

            spadeIdle[0] = mapsprites[0];
            spadeIdle[1] = mapsprites[1];
            spadeIdle[2] = mapsprites[2];
            spadeIdle[3] = mapsprites[3];
            spadeIdle[4] = mapsprites[2];
            spadeIdle[5] = mapsprites[1];
            spadeIdle[6] = mapsprites[0];

            spadeWalk = new Sprite[4];

            spadeWalk[0] = mapsprites[4];
            spadeWalk[1] = mapsprites[5];
            spadeWalk[2] = mapsprites[6];
            spadeWalk[3] = mapsprites[5];

            PlayableChara spadechar = new PlayableChara()
            {
                uid = "com.kuborro.spade",
                Name = "Spade",
                TutorialScene = "Tutorial1Spade",
                characterType = "RANGED Type",
                skill1 = "Card Special",
                skill2 = "Jump",
                skill3 = "Card Throw",
                skill4 = "Dodge Dash",
                airshipSprite = 1,
                useOwnCutsceneActivators = false,
                enabledInAventure = false,
                AirMoves = PatchFPPlayer.Action_Spade_AirMoves,
                GroundMoves = PatchFPPlayer.Action_Spade_GroundMoves,
                ItemFuelPickup = PatchFPPlayer.Action_Spade_FuelPickup,
                eventActivatorCharacter = FPCharacterID.CAROL,
                Gender = CharacterGender.MALE,
                profilePic = moddedBundle.LoadAsset<Sprite>("spade_portrait"),
                keyArtSprite = moddedBundle.LoadAsset<Sprite>("Spade_KeyArt"),
                endingKeyArtSprite = moddedBundle.LoadAsset<Sprite>("Spade_KeyArt"),
                charSelectName = moddedBundle.LoadAsset<Sprite>("Spade_Name"),
                piedSprite = (Sprite)moddedBundle.LoadAssetWithSubAssets("Spade_Pie")[1],
                piedHurtSprite = (Sprite)moddedBundle.LoadAssetWithSubAssets("Spade_Pie")[2],
                itemFuel = moddedBundle.LoadAsset<Sprite>("ItemFuelCards"),
                worldMapPauseSprite = moddedBundle.LoadAsset<Sprite>("spade_pause"),
                zaoBaseballSprite = moddedBundle.LoadAsset<Sprite>("SpadeZLBall"),
                livesIconAnim = moddedBundle.LoadAssetWithSubAssets<Sprite>("Spade_Stock"),
                worldMapIdle = spadeIdle,
                worldMapWalk = spadeWalk,
                sagaBlock = moddedBundle.LoadAsset<RuntimeAnimatorController>("SagaSpade"),
                sagaBlockSyntax = moddedBundle.LoadAsset<RuntimeAnimatorController>("Saga2Spade"),
                resultsTrack = spadeClear,
                endingTrack = spadeTheme,
                menuPhotoPose = new MenuPhotoPose(),
                characterSelectPrefab = spadeWheel,
                prefab = moddedBundle.LoadAsset<GameObject>("Player Spade"),
                dataBundle = moddedBundle
            };

            PlayerHandler.RegisterPlayableCharacterDirect(spadechar);

            var harmony = new Harmony("com.kuborro.plugins.fp2.playablespade");
            harmony.PatchAll(typeof(PatchFPPlayer));
            harmony.PatchAll(typeof(PatchFPStage));
            harmony.PatchAll(typeof(PatchFPEventSequence));
            harmony.PatchAll(typeof(PatchBFFMicroMissile));
            harmony.PatchAll(typeof(PatchPlayerBFF));
            harmony.PatchAll(typeof(PatchGO3DColumn));
            harmony.PatchAll(typeof(PatchLTNodePlayerBridge));
            harmony.PatchAll(typeof(PatchMenuWorldMap));
            harmony.PatchAll(typeof(PatchItemStarCard));
            harmony.PatchAll(typeof(PatchFPHubNPC));
            harmony.PatchAll(typeof(PatchAnimatorPreInitializer));
            harmony.PatchAll(typeof(PatchFPSaveManager));
            harmony.PatchAll(typeof(PatchFPResultsMenu));
        }
    }
}
