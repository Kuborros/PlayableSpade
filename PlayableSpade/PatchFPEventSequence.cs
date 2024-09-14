using HarmonyLib;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchFPEventSequence
    {
        //Spade Anywhere System™️
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPEventSequence), "Start", MethodType.Normal)]
        static void PatchStateDefault(FPEventSequence __instance)
        {
            if (__instance != null && FPSaveManager.character == (FPCharacterID)5)
            {
                //Prevent the Triple Spade Incident
                if (__instance.transform.parent != null && FPStage.stageNameString != "Nalao Lake")
                {
                    Transform cutsceneCarol = __instance.transform.parent.gameObject.transform.Find("Cutscene_Carol");
                    if (cutsceneCarol != null)
                    {
                        if (cutsceneCarol.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Spade Animator Player")
                        {
                            cutsceneCarol.gameObject.GetComponent<Animator>().runtimeAnimatorController = Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("Spade Animator Player");
                            cutsceneCarol.Find("tail").gameObject.SetActive(false);
                        }
                    }
                }

                //Post-Merga fight special case
                if (__instance.transform.parent != null && FPStage.stageNameString == "Merga")
                {
                    Transform eventSequence = __instance.transform.parent.gameObject.transform;
                    if (eventSequence != null)
                    {
                        Transform cutsceneCarol = eventSequence.parent.gameObject.transform.Find("Cutscene_Carol");
                        if (cutsceneCarol != null)
                        {
                            if (cutsceneCarol.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Spade Animator Player")
                            {
                                cutsceneCarol.gameObject.GetComponent<Animator>().runtimeAnimatorController = Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("Spade Animator Player");
                                cutsceneCarol.Find("tail").gameObject.SetActive(false);
                            }
                        }
                    }
                }

                //Snowfields magic
                if (__instance.transform.Find("Cutscene_Carol_Classic") != null)
                {
                    Transform cutsceneCarolClassic = __instance.transform.Find("Cutscene_Carol_Classic");
                    if (cutsceneCarolClassic != null)
                    {
                        if (cutsceneCarolClassic.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Spade Animator Player")
                        {
                            cutsceneCarolClassic.gameObject.GetComponent<Animator>().runtimeAnimatorController = Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("Spade Animator Player");
                            cutsceneCarolClassic.Find("tail").gameObject.SetActive(false);
                        }
                    }
                }

                //For Sigwada, we need to use Lilac's cutscene instead - Carol lacks the needed ones, since she skips the Cory fight
                if (__instance.transform.parent != null && FPStage.stageNameString == "Airship Sigwada")
                {
                    if (__instance.lilac) __instance.carol = true;

                    Transform eventSequence = __instance.gameObject.transform;
                    if (eventSequence != null)
                    {
                        Transform cutsceneLilac = eventSequence.Find("Cutscene_Lilac_Classic");
                        if (cutsceneLilac != null)
                        {
                            if (cutsceneLilac.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Spade Animator Player")
                            {
                                cutsceneLilac.gameObject.GetComponent<Animator>().runtimeAnimatorController = Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("Spade Animator Player");
                                cutsceneLilac.Find("tail").gameObject.SetActive(false);
                            }
                        }
                    }

                }
            }
        }


        //Special ending cutscene skipping code. These cutscenes do not have classic equivalents and still include dialogue - thus would mess immersion up.
        //Consider asking Spade's VA to voice a line for this. To anyone reading this - NO, AI Voice will *not* be even considered.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPEventSequence), "State_Event", MethodType.Normal)]
        static void PatchStateEvent(FPEventSequence __instance)
        {
            if (__instance != null && FPSaveManager.character == (FPCharacterID)5)
            {
                if (__instance.transform.parent != null && (FPStage.stageNameString == "Merga"))
                {
                    Transform eventSequence = __instance.transform.parent.gameObject.transform;
                    if (eventSequence != null)
                    {
                        Transform cutsceneCarol = eventSequence.parent.gameObject.transform.Find("Cutscene_Carol");
                        if (cutsceneCarol != null)
                        {
                            __instance.Action_SkipScene();
                        }
                    }
                }
            }
            if (__instance != null)
            {
                if (__instance.name == "Event Activator (Classic)" && __instance.transform.parent != null)
                {
                    if (__instance.transform.parent.gameObject.name == "Ending")
                        __instance.Action_SkipScene();
                }
            }
        }
    }
}
