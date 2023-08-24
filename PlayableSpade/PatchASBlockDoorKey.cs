using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayableSpade
{
    internal class PatchASBlockDoorKey
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ASBlockDoorKey), "InteractWithObjects", MethodType.Normal)]
        static void PatchASBDKey(ASBlockDoorKey __instance)
        {
            int num = __instance.DamageCheck();
            if (num != 0)
                Plugin.logSource.LogDebug("ASBlockDoorKey received damage of type:" + num);

            if (num != 1)
            {
                if (num != 2)
                {
                    if (num != 4)
                    {
                    }
                }
                else
                {
                    Plugin.logSource.LogDebug("Hit?");

                    if (__instance.direction == FPDirection.FACING_RIGHT)
                    {
                        if (__instance.velocity.x >= 0f && __instance.velocity.x < 7f)
                        {
                            __instance.velocity.x = 7f;
                        }
                    }
                    else if (__instance.velocity.x < -0f && __instance.velocity.x > -7f)
                    {
                        __instance.velocity.x = -7f;
                    }
                    __instance.health = 1f;
                    __instance.activationMode = FPActivationMode.XY_RANGE;
                }
            }
        }
    }
}
