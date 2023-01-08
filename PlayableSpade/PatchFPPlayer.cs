using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace PlayableSpade
{
    internal class PatchFPPlayer
    {
        private static int cardAngle;
        public static RuntimeAnimatorController cardAnimator;
        public static RuntimeAnimatorController dualCardAnimator;
        public static RuntimeAnimatorController spadeAnimator;
        public static AudioClip sfxThrowCard;
        public static AudioClip sfxThrowDualCard;
        public static AudioClip sfxRolling;
        public static FPPlayer player;
        public static float guardBuffer;

        static readonly MethodInfo m_AirMoves = SymbolExtensions.GetMethodInfo(() => Action_Spade_AirMoves());
        static readonly MethodInfo m_Jump = SymbolExtensions.GetMethodInfo(() => Action_Jump());
        static readonly MethodInfo m_GroundMoves = SymbolExtensions.GetMethodInfo(() => Action_Spade_GroundMoves());

        protected static float cardTimer;
        protected static float crashTimer;
        protected static float speedMultiplier;
        protected static bool upDash;

        public static readonly float SINGLE_SpadeBlinkDash_Velocity = 32f;
        protected static readonly int SINGLE_Spade_CardAmount_BasicThrow = 4;
        protected static readonly int SINGLE_Spade_CardAmount_DoubleThrow = 4;
        protected static readonly float SINGLE_Spade_EnergyRecoverRateMultiplier_LowPenaltyThreshold = 1f;
        protected static readonly float SINGLE_Spade_EnergyRecoverRateMultiplier_HighPenaltyThreshold = 0.7f;
        protected static readonly float SINGLE_Spade_EnergyRecoverPenaltyScore_LowThreshold = 6f;
        protected static readonly float SINGLE_Spade_EnergyRecoverPenaltyScore_HighThreshold = 40f;
        protected static readonly float SINGLE_SpadeAirPounce_VelocityXBoost_NominalValue = 8f;
        protected static readonly float SINGLE_SpadeAirPounce_VelocityXBoost_IncreasedBoostThreshold = 0f;
        protected static readonly float SINGLE_SpadeAirPounce_VelocityXBoost_IncreasedBoostMultiplier = 0.5f;
        protected static readonly float SINGLE_SpadeAirPounce_VelocityXBoost_BoostDecayStart = 10f;
        protected static readonly float SINGLE_SpadeAirPounce_VelocityXBoost_BoostDecayMax = 16f;
        protected static readonly float SINGLE_SpadeAirPounce_VelocityXBoost_BoostDecayMultiplier = 0.5f;
        protected static readonly float SINGLE_SpadeAirDownAttack_VelocityXBoost_NominalValue = 4f;
        protected static readonly float SINGLE_SpadeAirDownAttack_VelocityXBoost_IncreasedBoostThreshold = 4f;
        protected static readonly float SINGLE_SpadeAirDownAttack_VelocityXBoost_IncreasedBoostMultiplier = 1.5f;
        protected static readonly float SINGLE_SpadeAirDownAttack_VelocityXBoost_MaxBoostThreshold = 16f;
        protected static readonly float SINGLE_SpadeUpAttack_VelocityYBoost_InitialValue = 7f;
        protected static readonly float SINGLE_SpadeUpAttack_VelocityYBoost_DecayInitialValue = 3.5f;
        protected static readonly float SINGLE_SpadeUpAttack_VelocityYBoost_DecayDecrement = 1.5f;
        protected static readonly float SINGLE_SpadeUpAttack_VelocityYBoost_DecayFinalValue = 1.5f;
        protected static readonly float SINGLE_SpadeUpAttack_VelocityYBoost_FinalValue = 0f;
        protected static readonly float SINGLE_SpadeUpAttack_VelocityYBoost_MaxBoostThreshold = 9f;
        protected static readonly float SINGLE_SpadeUpAttack_VelocityYBoost_IncreasedBoostThreshold = 0f;
        protected static readonly float SINGLE_SpadeUpAttack_VelocityYBoost_IncreasedBoostMultiplier = 2f;
        

        public static void Action_ResetCardAngle()
        {
            cardAngle = 0;
        }

        private static void AttackStats_Spade_Blink()
        {
            FPPlayer player = FPStage.player[0];
            player.attackPower = 8f;
            player.attackHitstun = 0f;
            player.attackEnemyInvTime = 10f;
            player.attackKnockback.x = Mathf.Max(Mathf.Abs(player.prevVelocity.x * 0.375f), 4.5f);
            if (player.direction == FPDirection.FACING_LEFT)
            {
                player.attackKnockback.x = -player.attackKnockback.x;
            }
            player.attackKnockback.y = player.prevVelocity.y * 0.5f;
            player.attackSfx = 5;
            player.attackPower *= player.GetAttackModifier();
        }

        private static void State_ThrowCards()
        {
            if (cardTimer > 10)
            {
                if (player.onGround && player.velocity == Vector2.zero)
                {
                    player.SetPlayerAnimation("Throw", 0f, 0f);
                }
                else if (player.onGround)
                {
                    player.SetPlayerAnimation("Throw", 0f, 0f); //Replace with RunningThrow when added
                }
                else if (!player.onGround)
                {
                    player.SetPlayerAnimation("AirThrow", 0f, 0f);
                }
                Action_SpadeThrowCard();
                cardTimer = 0;                
            }
            else
            {
                if (player.onGround)
                {
                    ApplyGroundForces(player,false);
                    if (player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.8f || (player.input.jumpPress || player.input.jumpHold))
                    {
                        player.state = new FPObjectState(player.State_Ground);
                    }
                }
                else
                {
                    player.state = new FPObjectState(player.State_InAir);
                }
            }
            if (!player.onGround)ApplyGravityForce();
        }

        private static void State_DualCrash()
        {
            if (!player.onGround)
            {
                player.SetPlayerAnimation("AirSpecial", 0f, 0f);
                player.genericTimer += FPStage.deltaTime;
                player.energy -= 0.5f;
                player.velocity.y = 0f;
                player.angle = 0f;
                if (player.direction == FPDirection.FACING_LEFT)
                {
                    player.velocity.x = Mathf.Min(player.velocity.x + player.acceleration * FPStage.deltaTime, 7f);
                }
                else
                {
                    player.velocity.x = Mathf.Max(player.velocity.x - player.acceleration * FPStage.deltaTime, -7f);
                }
                if (player.energy <= 1f)
                {
                    player.genericTimer = 0f;
                    player.SetPlayerAnimation("Jumping", 0f, 0f);
                    player.state = new FPObjectState(player.State_InAir);
                    return;
                }
                Action_SpadeThrowDualCard();
            } 
            else
            {
                player.state = new FPObjectState(player.State_Ground);
            }
        }

        private static void Action_SpadeThrowCard()
        {
            for (int i = 1; i <= 3; i++)
            {
                float num = 10f;
                float num2 = player.angle + 20f - cardAngle * 10f;
                if (player.currentAnimation == "AirThrow")
                {
                    num2 = player.angle - cardAngle * 10f;
                }
                ProjectileBasic projectileBasic;
                if (player.direction == FPDirection.FACING_LEFT)
                {
                    projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x - Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * num, player.position.y + Mathf.Cos(0.017453292f * player.angle) * num - Mathf.Sin(0.017453292f * player.angle) * 32f);
                    projectileBasic.velocity.x = Mathf.Cos(0.017453292f * num2) * -16f;
                    projectileBasic.velocity.y = Mathf.Sin(0.017453292f * num2) * -16f;
                    projectileBasic.angle *= -1;
                }
                else
                {
                    projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x + Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * num, player.position.y + Mathf.Cos(0.017453292f * player.angle) * num + Mathf.Sin(0.017453292f * player.angle) * 32f);
                    projectileBasic.velocity.x = Mathf.Cos(0.017453292f * num2) * 16f;
                    projectileBasic.velocity.y = Mathf.Sin(0.017453292f * num2) * 16f;
                }
                projectileBasic.animatorController = cardAnimator;
                projectileBasic.attackPower = 2.5f;
                projectileBasic.animator = projectileBasic.GetComponent<Animator>();
                projectileBasic.animator.runtimeAnimatorController = projectileBasic.animatorController;
                projectileBasic.direction = player.direction;
                projectileBasic.angle = num2;              
                projectileBasic.explodeType = FPExplodeType.WHITEBURST;
                projectileBasic.sfxExplode = null;
                projectileBasic.parentObject = player;
                projectileBasic.faction = player.faction;
                cardAngle++;
                Action_PlaySound(sfxThrowCard,0.3f);

            }
            player.Process360Movement();
            Action_ResetCardAngle();
        }
        private static void Action_SpadeThrowDualCard()
        {
            if (crashTimer > 4f)
            {
                float num = 10f;
                float num2 = player.angle - 45f;
                ProjectileBasic projectileBasic;
                if (player.direction == FPDirection.FACING_LEFT)
                {
                    projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x - Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * num, player.position.y + Mathf.Cos(0.017453292f * player.angle) * num - Mathf.Sin(0.017453292f * player.angle) * 32f);
                    projectileBasic.velocity.x = Mathf.Cos(0.017453292f * num2) * -20f;
                    projectileBasic.velocity.y = Mathf.Sin(0.017453292f * num2) * 20f;
                    projectileBasic.angle = 180f - num2;
                }
                else
                {
                    projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x + Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * num, player.position.y + Mathf.Cos(0.017453292f * player.angle) * num + Mathf.Sin(0.017453292f * player.angle) * 32f);
                    projectileBasic.velocity.x = Mathf.Cos(0.017453292f * num2) * 20f;
                    projectileBasic.velocity.y = Mathf.Sin(0.017453292f * num2) * 20f;
                    projectileBasic.angle = num2;
                }
                projectileBasic.animatorController = dualCardAnimator;
                projectileBasic.animator = projectileBasic.GetComponent<Animator>();
                projectileBasic.animator.runtimeAnimatorController = projectileBasic.animatorController;
                projectileBasic.direction = FPDirection.FACING_RIGHT;
                projectileBasic.explodeType = FPExplodeType.WHITEBURST;
                projectileBasic.sfxExplode = null;
                projectileBasic.parentObject = player;
                projectileBasic.faction = player.faction;
                Action_PlaySound(sfxThrowDualCard,0.7f);
                crashTimer = 0;
            }
            player.Process360Movement();
        }
        public static void Action_PlaySound(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                return;
            }
            if (player == null)
            {
                return;
            }
            player.audioChannel[1].PlayOneShot(clip, volume);
        }

        public static void Action_Spade_Dash(float dashSpeed)
        {
            if (dashSpeed == 0f)
            {
                player.groundVel *= 0.5f;
                player.velocity.x = player.velocity.x * 0.5f;
            }
            else if ((player.input.up || player.input.upPress) && upDash)
            {
                player.velocity.y = Mathf.Max(Mathf.Min(player.velocity.y + dashSpeed, 12f), player.velocity.y);
                upDash = false;
            }
            else if (player.direction == FPDirection.FACING_RIGHT)
            {
                if (player.onGround)
                {
                    player.groundVel = Mathf.Max(Mathf.Min(player.groundVel + dashSpeed, 18f), player.groundVel);
                }
                else if (upDash)
                {
                    player.velocity.x = Mathf.Max(Mathf.Min(player.velocity.x + dashSpeed, 18f), player.velocity.x);
                    upDash= false;
                }
            }
            else if (player.onGround)
            {
                player.groundVel = Mathf.Min(Mathf.Max(player.groundVel - dashSpeed, -18f), player.groundVel);
            }
            else if (upDash)
            {
                player.velocity.x = Mathf.Min(Mathf.Max(player.velocity.x - dashSpeed, -18f), player.velocity.x);
                upDash = false;
            }
        }

        public static void Action_Spade_AirMoves()
        {
            if ((player.input.attackPress || player.input.attackHold && !(player.state == State_ThrowCards))) //Base Card Throw
            {
                player.idleTimer = -player.fightStanceTime;
                player.state = new FPObjectState(State_ThrowCards);                
            }
            else if ((player.input.specialPress || player.input.specialHold) && !(player.state == State_DualCrash))
            {
                player.idleTimer = -player.fightStanceTime;
                if (player.energy > 10)
                {
                    player.state = new FPObjectState(State_DualCrash);
                }
            }
            else if (player.guardTime <= 0f && (player.input.guardHold || (guardBuffer > 0f && player.input.guardHold)))
            {
                FPAudio.PlaySfx(15);
                player.Action_Guard(0f);
                player.Action_ShadowGuard();
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                guardFlash.parentObject = player;
                player.guardTime = 25f;
                if (player.energy > 25)
                {
                    Action_Spade_Dash(45f);
                    player.energy -= 25;
                }
            }
        }

        public static void Action_Spade_GroundMoves()
        {
            upDash = true;

            if ((player.input.attackPress || (player.input.attackHold && !player.input.up && !player.input.down)) && !(player.state == State_ThrowCards)) //Base Card Throw
            {
                player.idleTimer = -player.fightStanceTime;
                player.state = new FPObjectState(State_ThrowCards);
            }
            if (player.guardTime <= 0f && (player.input.guardPress || (guardBuffer > 0f && player.input.guardHold)))
            {
                FPAudio.PlaySfx(15);
                player.Action_Guard(0f);
                player.Action_ShadowGuard();
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                guardFlash.parentObject = player;
                player.guardTime = 25f;
                if (player.energy > 25)
                {
                    Action_Spade_Dash(90f);
                    player.energy -= 25;
                }
            }
        }

        private static void ApplyGravityForce()
        {
            if (player.hitStun <= 0f)
            {
                player.velocity.y = player.velocity.y + player.gravityStrength * FPStage.deltaTime;
            }
            if (player.velocity.y < -24f)
            {
                player.velocity.y = -24f;
            }
            player.quadrant = 0;
            RotatePlayerUpright();
        }

        private static void RotatePlayerUpright()
        {
            if (player.angle < 180f)
            {
                if (player.angle > 0f)
                {
                    player.angle -= 6f * FPStage.deltaTime;
                }
                if (player.angle < 0f)
                {
                    player.angle = 0f;
                }
            }
            else
            {
                if (player.angle < 360f)
                {
                    player.angle += 6f * FPStage.deltaTime;
                }
                if (player.angle > 360f)
                {
                    player.angle = 360f;
                }
            }
        }
        public static void Action_Jump()
        {
            player.Action_Jump();
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FPPlayer), "ApplyGroundForces", MethodType.Normal)]
        public static void ApplyGroundForces(FPPlayer instance, bool ignoreDirectionalInput)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Update", MethodType.Normal)]
        static void PatchPlayerUpdate(FPPlayer __instance, float ___speedMultiplier)
        {
            cardTimer += FPStage.deltaTime;
            crashTimer+= FPStage.deltaTime;
            player = __instance;
            speedMultiplier = ___speedMultiplier;
            if (player.onGround) upDash = true;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "Start", MethodType.Normal)]
        static void PatchPlayerStart(FPPlayer __instance)
        {
            UnityEngine.Object[] assets = Plugin.moddedBundle.LoadAllAssets();

            foreach (var asset in assets)
            {
                if (asset.GetType() == typeof(RuntimeAnimatorController))
                {
                    if (asset.name == "Spade Animator Player")
                    {
                        spadeAnimator = (RuntimeAnimatorController)asset;
                        spadeAnimator.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    }
                    if (asset.name == "ThrowingCard")
                    {
                        cardAnimator = (RuntimeAnimatorController)asset;
                        cardAnimator.hideFlags = HideFlags.DontUnloadUnusedAsset;

                        dualCardAnimator = (RuntimeAnimatorController)asset;
                        dualCardAnimator.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    }
                }
                if (asset.GetType() == typeof(AudioClip))
                {
                    if (asset.name == "DiscThrow")
                    {
                        sfxThrowCard= (AudioClip)asset;
                        sfxThrowDualCard = (AudioClip)asset;
                    }
                }

            }
            player = __instance;
            upDash = true;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FPPlayer), "Action_Hurt", MethodType.Normal)]
        static IEnumerable<CodeInstruction> PlayerHurtTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label airStart = il.DefineLabel();
            Label airEnd = il.DefineLabel();
            Label groundStart = il.DefineLabel();
            Label groundEnd = il.DefineLabel();

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_1)
                {
                    Label[] targets = (Label[])codes[i].operand;                 
                    targets = targets.AddItem(airStart).ToArray();
                    codes[i].operand = targets;
                    airEnd = (Label)codes[i + 1].operand;
                }
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_2)
                {
                    Label[] targets = (Label[])codes[i].operand;
                    targets = targets.AddItem(groundStart).ToArray();
                    codes[i].operand = targets;
                    groundEnd = (Label)codes[i + 1].operand;
                    break;
                }
            }
            CodeInstruction airCodeStart = new CodeInstruction(OpCodes.Ldarg_0);
            airCodeStart.labels.Add(airStart);

            codes.Add(airCodeStart);
            codes.Add(new CodeInstruction(OpCodes.Call, m_AirMoves));
            codes.Add(new CodeInstruction(OpCodes.Br, airEnd));

            CodeInstruction groundCodeStart = new CodeInstruction(OpCodes.Ldarg_0);
            groundCodeStart.labels.Add(groundStart);

            codes.Add(groundCodeStart);
            codes.Add(new CodeInstruction(OpCodes.Call, m_GroundMoves));
            codes.Add(new CodeInstruction(OpCodes.Br, groundEnd));


            return codes;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FPPlayer), "State_Crouching", MethodType.Normal)]
        static IEnumerable<CodeInstruction> PlayerCrouchTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label groundStart = il.DefineLabel();
            Label groundEnd = il.DefineLabel();

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_0)
                {
                    Label[] targets = (Label[])codes[i].operand;
                    targets = targets.AddItem(groundStart).ToArray();
                    codes[i].operand = targets;
                    groundEnd = (Label)codes[i + 1].operand;
                    break;
                }
            }

            CodeInstruction groundCodeStart = new CodeInstruction(OpCodes.Ldarg_0);
            groundCodeStart.labels.Add(groundStart);

            codes.Add(groundCodeStart);
            codes.Add(new CodeInstruction(OpCodes.Call, m_GroundMoves));
            codes.Add(new CodeInstruction(OpCodes.Br, groundEnd));


            return codes;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FPPlayer), "State_Ground", MethodType.Normal)]
        static IEnumerable<CodeInstruction> PlayerGroundTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label groundStart = il.DefineLabel();
            Label groundEnd = il.DefineLabel();

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_S)
                {
                    Label[] targets = (Label[])codes[i].operand;
                    targets = targets.AddItem(groundStart).ToArray();
                    codes[i].operand = targets;
                    groundEnd = (Label)codes[i + 1].operand;
                    break;
                }
            }

            CodeInstruction groundCodeStart = new CodeInstruction(OpCodes.Ldarg_0);
            groundCodeStart.labels.Add(groundStart);

            codes.Add(groundCodeStart);
            codes.Add(new CodeInstruction(OpCodes.Call, m_GroundMoves));
            codes.Add(new CodeInstruction(OpCodes.Br, groundEnd));


            return codes;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "State_InAir", MethodType.Normal)]
        static void PatchPlayerInAir()
        {
            if (player.currentAnimation == "AirThrow" && player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.8f)
            {
                player.SetPlayerAnimation("Jumping", 0f, 0f);
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FPPlayer), "State_InAir", MethodType.Normal)]
        static IEnumerable<CodeInstruction> PlayerAirTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label airStart = il.DefineLabel();
            Label airEnd = il.DefineLabel();

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_2)
                {
                    Label[] targets = (Label[])codes[i].operand;
                    targets = targets.AddItem(airStart).ToArray();
                    codes[i].operand = targets;
                    airEnd = (Label)codes[i + 1].operand;
                }

            }
            CodeInstruction airCodeStart = new CodeInstruction(OpCodes.Ldarg_0);
            airCodeStart.labels.Add(airStart);

            codes.Add(airCodeStart);
            codes.Add(new CodeInstruction(OpCodes.Call, m_AirMoves));
            codes.Add(new CodeInstruction(OpCodes.Br, airEnd));

            return codes;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "State_Hanging", MethodType.Normal)]
        static void PatchPlayerHang()
        {
            upDash = true;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FPPlayer), "State_Hanging", MethodType.Normal)]
        static IEnumerable<CodeInstruction> PlayerHangTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label airStart = il.DefineLabel();
            Label airEnd = il.DefineLabel();

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_3)
                {
                    Label[] targets = (Label[])codes[i].operand;
                    targets = targets.AddItem(airStart).ToArray();
                    codes[i].operand = targets;
                    airEnd = (Label)codes[i + 1].operand;
                }

            }
            CodeInstruction airCodeStart = new CodeInstruction(OpCodes.Ldarg_0);
            airCodeStart.labels.Add(airStart);

            codes.Add(airCodeStart);
            codes.Add(new CodeInstruction(OpCodes.Call, m_AirMoves));
            codes.Add(new CodeInstruction(OpCodes.Br, airEnd));

            return codes;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FPPlayer), "State_Swimming", MethodType.Normal)]
        static IEnumerable<CodeInstruction> PlayerSwimTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label airStart = il.DefineLabel();
            Label airEnd = il.DefineLabel();

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_0)
                {
                    Label[] targets = (Label[])codes[i].operand;
                    targets = targets.AddItem(airStart).ToArray();
                    codes[i].operand = targets;
                    airEnd = (Label)codes[i + 1].operand;
                }

            }
            CodeInstruction airCodeStart = new CodeInstruction(OpCodes.Ldarg_0);
            airCodeStart.labels.Add(airStart);

            codes.Add(airCodeStart);
            codes.Add(new CodeInstruction(OpCodes.Call, m_AirMoves));
            codes.Add(new CodeInstruction(OpCodes.Br, airEnd));

            return codes;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "State_GrindRail", MethodType.Normal)]
        static void PatchPlayerGrind()
        {
            upDash = true;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FPPlayer), "State_GrindRail", MethodType.Normal)]
        static IEnumerable<CodeInstruction> PlayerGrindTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label airStart = il.DefineLabel();
            Label airEnd = il.DefineLabel();

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Switch && codes[i - 1].opcode == OpCodes.Ldloc_0)
                {
                    Label[] targets = (Label[])codes[i].operand;
                    targets = targets.AddItem(airStart).ToArray();
                    codes[i].operand = targets;
                    airEnd = (Label)codes[i + 1].operand;
                }

            }
            CodeInstruction airCodeStart = new CodeInstruction(OpCodes.Ldarg_0);
            airCodeStart.labels.Add(airStart);

            codes.Add(airCodeStart);
            codes.Add(new CodeInstruction(OpCodes.Call, m_Jump));
            codes.Add(new CodeInstruction(OpCodes.Call, m_AirMoves));
            codes.Add(new CodeInstruction(OpCodes.Br, airEnd));

            return codes;
        }

    }
}
