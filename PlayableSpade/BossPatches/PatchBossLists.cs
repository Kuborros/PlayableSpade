using FP2Lib.Player;
using HarmonyLib;
using System;
using UnityEngine;

namespace PlayableSpade.BossPatches
{
    internal class PatchBossLists
    {

        internal static GameObject spadeBoss = null;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ArenaSpawner), "Start", MethodType.Normal)]
        static void PatchArenaSpawnerStart(ArenaSpawner __instance)
        {
            spadeBoss = null;

            if (FPStage.stageNameString == "Training" && (FPSaveManager.currentArenaChallenge == 30 || FPSaveManager.currentArenaChallenge == 0)) FPAudio.StopMusic();
            if (FPStage.stageNameString == "Training" && spadeBoss == null)
            {
                spadeBoss = GameObject.Instantiate(PlayerHandler.PlayableChars["com.kuborro.spade"].playerBoss.gameObject);
                spadeBoss.SetActive(false);
                spadeBoss.name = "Boss Spade";

                if (spadeBoss != null && (FPSaveManager.currentArenaChallenge == 30 || FPSaveManager.currentArenaChallenge == 0))
                {
                    __instance.syncChallengeID = false;

                    ArenaRoundSpawnList spadeList = new()
                    {
                        bossBattle = true,
                        waitForObjectDestruction = false,
                        objectList = new FPBaseObject[] { spadeBoss.GetComponent<PlayerBossSpade>() }
                    };
                    ArenaSpawnList spawnList = new()
                    {
                        name = "SpadeBoss",
                        challengeID = 30,
                        rewardCrystals = 1000,
                        rewardTimeCapsule = false,
                        timeCapsuleID = 0,
                        spawnAllies = false,
                        alliesAreHostile = false,
                        disableCorePickups = false,
                        spawnAtStart = new FPBaseObject[] { spadeBoss.GetComponent<PlayerBossSpade>() },
                        roundObjectList = new ArenaRoundSpawnList[] { spadeList },
                        spawnDelay = new float[] { 0 },
                        endCutscene = "",
                        victoryDelayOffset = 0
                    };

                    __instance.challenges = __instance.challenges.AddToArray(spawnList);
                    __instance.currentChallenge = 6;
                    FPSaveManager.currentArenaChallenge = 6;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuArenaBossSelect), "Start", MethodType.Normal)]
        static void PatchArenaBossSelectPre(MenuArenaBossSelect __instance) 
        {
            if (FPStage.stageNameString == "Royal Palace")
            {
                //Append Spade
                __instance.bossScenes = __instance.bossScenes.AddToArray("RoyalPalace_Sparring");
                __instance.bossUnlockRequirement = __instance.bossUnlockRequirement.AddToArray(-1);
                __instance.bossSpawnID = __instance.bossSpawnID.AddToArray(30);
            }
        }

        //Patch to replace Askal with Spade in Shang Tu Dojo
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuArenaBossSelect), "Start", MethodType.Normal)]
        static void PatchArenaBossSelect(MenuArenaBossSelect __instance)
        {
            if (FPStage.stageNameString == "Royal Palace")
            { 
                //Make sure we dont edit the BattleSphere
                GameObject icon = __instance.transform.GetChild(1).GetChild(6).gameObject;
                if (icon != null)
                {
                    icon.GetComponent<SpriteRenderer>().sprite = PlayableSpade.moddedBundle.LoadAsset<Sprite>("Spade_Profile_Boss");
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuText), "Start", MethodType.Normal)]
        static void Postfix(ref string[] ___paragraph, MenuText __instance)
        {
            if (___paragraph != null && FPStage.stageNameString == "Royal Palace" && __instance.name == "Name") //Same deal as above, we also check if its the MenuText we want
            {
                if (___paragraph.Length > 2) //One other MenuText matches, but it has lenght of 1. We make sure we arent trying to manipulate that one
                    ___paragraph[Array.IndexOf(___paragraph, "Proto Pincer")] = "Spade";
            }

        }
    }
}
