using HarmonyLib;

namespace PlayableSpade
{
    internal class PatchMenuPhoto
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuPhoto), "Start", MethodType.Normal)]
        static void PatchMenuPhotoStart(ref MenuPhotoPose[] ___poseList)
        {
            MenuPhotoPose spadePoses = new MenuPhotoPose();
         ___poseList = ___poseList.AddToArray(spadePoses);
        }
    }
}
