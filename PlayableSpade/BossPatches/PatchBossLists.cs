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
        static void Prefix(ArenaSpawner __instance)
        {
            spadeBoss = null;

            if (FPStage.stageNameString == "Training" && (FPSaveManager.currentArenaChallenge == 36 || FPSaveManager.currentArenaChallenge == 6)) FPAudio.StopMusic();
            if (FPStage.stageNameString == "Training" && spadeBoss == null)
            {
                spadeBoss = GameObject.Instantiate(PlayerHandler.PlayableChars["com.kuborro.spade"].playerBoss.gameObject);
                spadeBoss.name = "Boss Spade";

                if (spadeBoss != null && (FPSaveManager.currentArenaChallenge == 36 || FPSaveManager.currentArenaChallenge == 6))
                {
                    __instance.syncChallengeID = false;
                    PatchPlayerBossSpade.FightStarted = false;

                    ArenaRoundSpawnList spadeList = new()
                    {
                        bossBattle = true,
                        waitForObjectDestruction = false,
                        objectList = new FPBaseObject[] { spadeBoss.GetComponent<PlayerBossSpade>() }
                    };
                    ArenaSpawnList spawnList = new()
                    {
                        name = "SpadeBoss",
                        challengeID = 36,
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

        //Patch to replace Askal with Spade in Shang Mu Dojo
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuArenaBossSelect), "Start", MethodType.Normal)]
        static void Postfix(MenuArenaBossSelect __instance)
        {
            if (FPStage.stageNameString == "Royal Palace")
            { //Make sure we dont edit the BattleSphere
                SpriteRenderer[] components = __instance.GetComponentsInChildren<SpriteRenderer>();

                foreach (SpriteRenderer component in components)
                {
                    if (component.sprite.name == "arena_bosses_6")
                    {
                        component.sprite = PlayableSpade.moddedBundle.LoadAsset<Sprite>("Spade_Profile_Boss");
                    }
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
                    ___paragraph[Array.IndexOf(___paragraph, "Askal")] = "Spade";
            }

        }
    }
}
