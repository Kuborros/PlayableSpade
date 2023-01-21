﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayableSpade
{
    internal class PatchGO3DColumn
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GO3DColumn), "Action_Dismount", MethodType.Normal)]
        static void Patch3DColumnDismount()
        {
            PatchFPPlayer.upDash = true;
        }
    }
}
