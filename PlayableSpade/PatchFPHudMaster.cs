using HarmonyLib;

namespace PlayableSpade
{
    internal class PatchFPHudMaster
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPHudMaster), "GuideUpdate", MethodType.Normal)]
        static void PatchGuideUpdate(FPPlayer player, FPHudMaster __instance)
        {
            string text = "Jump";
            string text2 = "Card Throw";
            string text3 = "<c=energy>Special</c>";
            string text4 = "Guard";

            if (player == null || player.characterID != Plugin.spadeCharID)
            {
                return;
            }
            if (player.IsKOd(false))
            {
                text = "-";
                text2 = "-";
                text3 = "-";
                text4 = "-";
            }

            //Mid-air
            if (!player.onGround && player.state != new FPObjectState(player.State_LadderClimb))
            {
                //No double-jump for Spade
                text = "-";
                if (player.state != new FPObjectState(player.State_Ball) && player.state != new FPObjectState(player.State_Ball_Physics) && player.state != new FPObjectState(player.State_Ball_Vulnerable))
                {
                    text3 = "<c=energy>Power Cards</c>";
                }
            }

            //On the ground, excluding funky states
            if (player.state != new FPObjectState(player.State_LadderClimb) && player.state != new FPObjectState(player.State_Ball) && player.state != new FPObjectState(player.State_Ball_Physics) && player.state != new FPObjectState(player.State_Ball_Vulnerable))
            {
                if (player.input.down && !player.input.up)
                {
                    if (player.state == new FPObjectState(player.State_Crouching) || (player.onGround && player.groundVel == 0f))
                    {
                        text2 = "Sneak Throw";
                    }
                    else
                    {
                        text2 = "Downwards Throw";
                    }
                }
                if (player.input.up && !player.input.down)
                {
                    text2 = "Upwards Throw";
                }
                if (player.onGround)
                {
                    text3 = "<c=energy>Capture Cards</c>";
                }
            }
            if (PatchFPPlayer.upDash && !Plugin.configDashOnDoubleJump.Value)
            {
                if (player.input.down || player.input.downPress)
                {
                    text4 = "Ground Pound";
                } 
                else
                    text4 = "Dodge Dash";
            }

            if (player.displayMoveJump != string.Empty)
            {
                text = player.displayMoveJump;
            }
            if (player.displayMoveAttack != string.Empty)
            {
                text2 = player.displayMoveAttack;
            }
            if (player.displayMoveSpecial != string.Empty)
            {
                text3 = player.displayMoveSpecial;
            }
            if (player.displayMoveGuard != string.Empty)
            {
                text4 = player.displayMoveGuard;
            }
            __instance.hudGuide.text = string.Concat(new string[]
            {
            text,
            "\n",
            text2,
            "\n",
            text3,
            "\n",
            text4,
            "\n "
            });
        }
    }
}
