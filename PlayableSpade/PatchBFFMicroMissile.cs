using HarmonyLib;

namespace PlayableSpade
{
    internal class PatchBFFMicroMissile
    {
        public static bool BFFActive = false;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BFFMicroMissile), "State_Default", MethodType.Normal)]
        static void PatchBFFMicroMissileDefault(ref float ___speed, float ___explodeTimer)
        {
            if (!BFFActive)
            {
                if ((___explodeTimer > 150f && ___explodeTimer < 175f) || (___explodeTimer > 120f && ___explodeTimer < 140f) || (___explodeTimer > 90f && ___explodeTimer < 110f) || (___explodeTimer > 60f && ___explodeTimer < 80f))
                {
                    ___speed = 0;
                }
                else
                {
                    ___speed = 20;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BFFMicroMissile), "State_Done", MethodType.Normal)]
        static void PatchBFFMicroMissileDone(BFFMicroMissile __instance)
        {
            __instance.activationMode = FPActivationMode.NEVER_ACTIVE;
            __instance.gameObject.SetActive(false);

        }

    }
}
