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

            if (FPStage.stageNameString == "Training" && FPSaveManager.currentArenaChallenge == PlayableSpade.spadeBossID) FPAudio.StopMusic();
            if (FPStage.stageNameString == "Training" && spadeBoss == null)
            {
                spadeBoss = GameObject.Instantiate(PlayerHandler.PlayableChars["com.kuborro.spade"].playerBoss.gameObject);
                spadeBoss.SetActive(false);
                spadeBoss.name = "Boss Spade";

                if (spadeBoss != null && FPSaveManager.currentArenaChallenge == PlayableSpade.spadeBossID)
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
                        challengeID = PlayableSpade.spadeBossID,
                        rewardCrystals = 200,
                        rewardTimeCapsule = false,
                        timeCapsuleID = -1,
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
                    __instance.currentChallenge = PlayableSpade.spadeBossID;
                    FPSaveManager.currentArenaChallenge = PlayableSpade.spadeBossID;
                }
            }
        }
    }
}
