using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchFPBreakable
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPBreakable), "CollisionCheck", MethodType.Normal)]
        static void PatchCollision(FPBreakable __instance)
        {
            List<FPPlayer> players = FPStage.FindFPPlayers(false);
            if (__instance == null || players.Count == 0) return;
            foreach (FPPlayer fpplayer in players)
            {
                if (FPCollision.CheckOOBB(__instance, __instance.hbSolid, fpplayer, fpplayer.hbTouch, false, false, false) && (fpplayer.state == new FPObjectState(fpplayer.State_Lilac_DragonBoostPt2) || (fpplayer.currentAnimation == "Rolling" && fpplayer.state != new FPObjectState(fpplayer.State_Carol_Roll))))
                {
                    FPStage.CreateStageObject(HitSpark.classID, (fpplayer.position.x + __instance.position.x * 2f) * 0.33333334f + UnityEngine.Random.Range(-20f, 20f), (fpplayer.position.y + __instance.position.y * 2f) * 0.33333334f + UnityEngine.Random.Range(-20f, 20f));
                    __instance.Hit(0f);
                    fpplayer.hitStun = fpplayer.attackHitstun;
                }
            }
        }
    }
}
