using HarmonyLib;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchStingerBomb
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StingerBomb), "State_Explode", MethodType.Normal)]
        static void PatchStingerBombExplode(StingerBomb __instance)
        {
            if (__instance.type == (StingerBombType)2)
            {
                BFFProjectile explosion = (BFFProjectile)FPStage.CreateStageObject(BFFProjectile.classID , __instance.position.x , __instance.position.y);
                explosion.parentObject = __instance;
                explosion.faction = __instance.faction;
                explosion.scale = new Vector2(1.5f, 1.5f);
                explosion.velocity = Vector2.zero;
                explosion.explodeTimer = 50;
            }
        }
    }
}
