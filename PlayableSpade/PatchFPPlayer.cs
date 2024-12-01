using HarmonyLib;
using Rewired;
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
        public static RuntimeAnimatorController[] cardAnimator;
        public static RuntimeAnimatorController[] ironCardAnimator;
        public static RuntimeAnimatorController shadowCardAnimator;
        public static RuntimeAnimatorController dualCardAnimator;
        public static RuntimeAnimatorController shadowDualCardAnimator;
        public static RuntimeAnimatorController ironDualCardAnimator;
        public static RuntimeAnimatorController captureCardAnimator;
        public static AudioClip sfxThrowCard;
        public static AudioClip sfxThrowDualCard;
        public static FPPlayer player;
        public static PlayerShadow playerShadow;
        public static bool upDash;

        internal static readonly MethodInfo m_AirMoves = SymbolExtensions.GetMethodInfo(() => Action_Spade_AirMoves());
        internal static readonly MethodInfo m_FuelPickup = SymbolExtensions.GetMethodInfo(() => Action_Spade_FuelPickup());
        internal static readonly MethodInfo m_Jump = SymbolExtensions.GetMethodInfo(() => Action_Jump());
        internal static readonly MethodInfo m_GroundMoves = SymbolExtensions.GetMethodInfo(() => Action_Spade_GroundMoves());

        protected static float cardTimer;
        protected static float crashTimer;
        protected static float speedMultiplier;
        protected static float guardBuffer;
        protected static bool autoGuard;
        protected static float ghostTimer = 0f;
        protected static float shadowTimer = 0f;
        protected static float dashTime = 0f;

        private static List<FPBaseEnemy> cardTargetedEnemies;
        private static int captureCardCount = 3;
        private static float captureCardRange = 512f;
        private static float captureCardDamage = 4f;
        private static int cardAngle;

        private static readonly FPHitBox cardHitbox = new FPHitBox { left = -16, right = 16, top = 16, bottom = -16, enabled = true }; 


        public static void Action_ResetCardAngle()
        {
            cardAngle = 0;
        }

        private static void UpdateCardTargetedEnemies()
        {
            List<FPBaseEnemy> enemyListInMissileRange = GetEnemyListInCardRange();
            enemyListInMissileRange.Sort(new Comparison<FPBaseEnemy>(CompareCardTargets));
            int i = 0;
            while (i < enemyListInMissileRange.Count - 1)
            {
                if (ReferenceEquals(enemyListInMissileRange[i], enemyListInMissileRange[i + 1]))
                {
                    enemyListInMissileRange.RemoveAt(i + 1);
                }
                else
                {
                    i++;
                }
            }
            cardTargetedEnemies = new List<FPBaseEnemy>(Mathf.Min(enemyListInMissileRange.Count, captureCardCount));
            i = 0;
            while (i < enemyListInMissileRange.Count && cardTargetedEnemies.Count < captureCardCount)
            {
                if (!(enemyListInMissileRange[i] == null))
                {
                    if (!cardTargetedEnemies.Contains(enemyListInMissileRange[i]))
                    {
                        cardTargetedEnemies.Add(enemyListInMissileRange[i]);
                    }
                }
                i++;
            }
        }

        private static int CompareCardTargets(FPBaseEnemy enemy1, FPBaseEnemy enemy2)
        {
            if (ReferenceEquals(enemy1, enemy2))
            {
                return 0;
            }
            if (enemy1 == null)
            {
                return 1;
            }
            if (enemy2 == null)
            {
                return -1;
            }
            float num = Vector2.SqrMagnitude(player.position - enemy1.position);
            float num2 = Vector2.SqrMagnitude(player.position - enemy2.position);
            if (num < num2)
            {
                return -1;
            }
            if (num > num2)
            {
                return 1;
            }
            if (enemy1.stageListPos < enemy2.stageListPos)
            {
                return -1;
            }
            if (enemy1.stageListPos > enemy2.stageListPos)
            {
                return 1;
            }
            return 0;
        }


        private static List<FPBaseEnemy> GetEnemyListInCardRange()
        {
            List<FPBaseEnemy> list = new List<FPBaseEnemy>();
            float num = captureCardRange * captureCardRange;
            foreach (FPBaseEnemy fpbaseEnemy in FPStage.GetActiveEnemies(false, false))
            {
                if (fpbaseEnemy.health > 0f && fpbaseEnemy.CanBeTargeted() && (player == null || (player != null && fpbaseEnemy.faction != player.faction)) && Vector2.SqrMagnitude(player.position - fpbaseEnemy.position) <= num)
                {
                    list.Add(fpbaseEnemy);
                }
            }
            return list;
        }

        private static Vector2 GetTargetOffset(FPBaseEnemy enemy)
        {
            FPHitBox hbWeakpoint = enemy.hbWeakpoint;
            if (enemy.GetComponent<MonsterCube>() != null)
            {
                hbWeakpoint = enemy.GetComponent<MonsterCube>().childWeakpoint.hbWeakpoint;
            }
            float x = UnityEngine.Random.Range(hbWeakpoint.left, hbWeakpoint.right);
            float y = UnityEngine.Random.Range(hbWeakpoint.bottom, hbWeakpoint.top);
            return new Vector2(x, y);
        }

        private static void State_ThrowCards()
        {
            player.genericTimer += FPStage.deltaTime;
            if (cardTimer > 10)
            {
                if (player.onGround && player.velocity == Vector2.zero)
                {
                    player.SetPlayerAnimation("Throw");
                }
                else if (player.onGround)
                {
                    player.SetPlayerAnimation("RunningThrow");
                }
                else if (!player.onGround)
                {
                    player.SetPlayerAnimation("AirThrow");
                }
                Action_SpadeThrowCard();
                cardTimer = 0;
            }
            else
            {
                if (player.onGround)
                {
                    ApplyGroundForces(player, false);
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
            if (!player.onGround)
            {
                if (player.targetWaterSurface != null)
                {
                    ApplyWaterForces(player);
                }
                else ApplyGravityForce();
            }
        }

        private static void State_DualCrash()
        {
            if (!player.onGround)
            {
                player.SetPlayerAnimation("AirSpecial");
                player.genericTimer += FPStage.deltaTime;
                player.energy -= 1.5f * FPStage.deltaTime;
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
                if (player.energy <= 1f || !player.input.specialHold)
                {
                    player.genericTimer = 0f;
                    player.SetPlayerAnimation("Jumping");
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

        private static void State_Spade_AirDash()
        {
            player.SetPlayerAnimation("AirDash");
            player.genericTimer += FPStage.deltaTime;
            player.superArmor = true;
            ghostTimer += FPStage.deltaTime;
            player.attackStats = new FPObjectState(AttackStats_Dash);

            if (ghostTimer >= 2f)
            {
                Ghost();
                ghostTimer = 0f;
            }

            if (player.genericTimer >= 10f)
            {
                player.genericTimer = 0f;
                player.hbAttack.enabled = false;
                player.superArmor = false;
                if (player.onGround)
                {
                    player.state = new FPObjectState(player.State_Ground);
                }
                else
                {
                    player.state = new FPObjectState(player.State_InAir);
                }
                return;
            }
        }

        private static void State_Spade_GroundPound()
        {
            bool shockwave = false;
            player.genericTimer += FPStage.deltaTime;
            player.SetPlayerAnimation("GroundPoundAir");
            player.superArmor = true;
            ghostTimer += FPStage.deltaTime;
            player.attackStats = new FPObjectState(AttackStats_Dash);

            if (ghostTimer >= 1f)
            {
                Ghost();
                ghostTimer = 0f;
            }

            player.velocity.y = -20f;
            player.velocity.x = 0f;
            player.Process360Movement();

            if (player.onGround || player.onGrindRail || player.genericTimer >= 100f)
            {
                player.hbAttack.enabled = false;
                player.superArmor = false;
                if (!shockwave && player.onGround)
                {
                    StingerBomb stingerBomb = (StingerBomb)FPStage.CreateStageObject(StingerBomb.classID, player.position.x, player.position.y - player.halfHeight);
                    stingerBomb.explodeTimer = 999f;
                    stingerBomb.faction = player.faction;
                    shockwave = true;
                }

                dashTime += 10f;

                if (player.onGround)
                {
                    player.SetPlayerAnimation("Crouching");
                    player.state = new FPObjectState(player.State_Crouching);
                }
                else
                {
                    player.state = new FPObjectState(player.State_InAir);
                }
                return;
            }
        }

        private static void State_Spade_CaptureCard()
        {
            if (player.velocity == Vector2.zero)
            {
                player.SetPlayerAnimation("Throw");
            }
            else
            {
                player.SetPlayerAnimation("RunningThrow");
            }

            player.genericTimer += FPStage.deltaTime;
            if (player.genericTimer >= 10f)
            {
                player.genericTimer = 0f;
                if (player.onGround)
                {
                    player.state = new FPObjectState(player.State_Ground);
                }
                else
                {
                    player.state = new FPObjectState(player.State_InAir);
                }
                return;
            }
            Action_ResetCardAngle();
        }

        private static void Action_SpadeThrowCaptureCard()
        {
            UpdateCardTargetedEnemies();
            List<FPBaseEnemy> list = cardTargetedEnemies;
            Vector3 localScale = player.transform.localScale;
            int num = 0;
            FPAudio.PlaySfx(sfxThrowCard);
            if (player.direction == FPDirection.FACING_LEFT) cardAngle += 2;
            for (int i = 0; i < captureCardCount; i++)
            {
                BFFMicroMissile bffmicroMissile;
                if (player.direction == FPDirection.FACING_LEFT)
                {
                    bffmicroMissile = (BFFMicroMissile)FPStage.CreateStageObject(BFFMicroMissile.classID, player.position.x - Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * 10, player.position.y + Mathf.Cos(0.017453292f * player.angle) * 10 - Mathf.Sin(0.017453292f * player.angle) * 32f);
                }
                else
                {
                    bffmicroMissile = (BFFMicroMissile)FPStage.CreateStageObject(BFFMicroMissile.classID, player.position.x + Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * 10, player.position.y + Mathf.Cos(0.017453292f * player.angle) * 10 + Mathf.Sin(0.017453292f * player.angle) * 32f);
                }
                bffmicroMissile.transform.rotation = Quaternion.Euler(0f, 0f, player.angle + 20f - cardAngle * 10f + ((localScale.x < 0f) ? 180 : 0));
                if (list.Count > 0)
                {
                    bffmicroMissile.AssignTarget(list[num], GetTargetOffset(list[num]));
                    num++;
                    if (num >= list.Count)
                    {
                        num = 0;
                    }
                }
                cardAngle++;
                bffmicroMissile.attackPower = captureCardDamage * player.GetAttackModifier();
                bffmicroMissile.turnSpeed = 50;
                if (FPStage.stageNameString == "Nalao Lake")
                {
                    bffmicroMissile.gameObject.GetComponent<Animator>().runtimeAnimatorController = captureCardAnimator;
                    bffmicroMissile.gameObject.GetComponent<LineRenderer>().enabled = false;
                }
                bffmicroMissile.ignoreTerrain = true;
                bffmicroMissile.faction = player.faction;
            }

        }

        private static void Action_SpadeThrowCard()
        {
            if (player.direction == FPDirection.FACING_LEFT)
            {
                if (player.onGround) cardAngle += 2;
                if (player.input.up)
                {
                    cardAngle += 4;
                }
                if (player.input.down && !player.onGround)
                {
                    cardAngle -= 6;
                }
            }
            else
            {
                if (player.input.up)
                {
                    cardAngle -= 4;
                }
                if (player.input.down && !player.onGround)
                {
                    cardAngle += 4;
                }
            }
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
                }
                else
                {
                    projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x + Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * num, player.position.y + Mathf.Cos(0.017453292f * player.angle) * num + Mathf.Sin(0.017453292f * player.angle) * 32f);
                    projectileBasic.velocity.x = Mathf.Cos(0.017453292f * num2) * 16f;
                    projectileBasic.velocity.y = Mathf.Sin(0.017453292f * num2) * 16f;
                }
                projectileBasic.animatorController = cardAnimator[UnityEngine.Random.RandomRangeInt(0, 3)];
                projectileBasic.attackPower = 2.5f * player.GetAttackModifier();
                projectileBasic.animator = projectileBasic.GetComponent<Animator>();
                projectileBasic.animator.runtimeAnimatorController = projectileBasic.animatorController;
                projectileBasic.direction = player.direction;
                projectileBasic.damageElementType = -1;
                projectileBasic.scale = new Vector2(0.8f, 0.8f);
                projectileBasic.angle = num2;
                projectileBasic.ignoreTerrain = false;
                projectileBasic.explodeType = FPExplodeType.WHITEBURST;
                projectileBasic.explodeTimer = 50f;
                projectileBasic.terminalVelocity = 0f;
                projectileBasic.gravityStrength = 0;
                projectileBasic.sfxExplode = null;
                projectileBasic.parentObject = player;
                projectileBasic.faction = player.faction;
                projectileBasic.timeBeforeCollisions = 0f;
                projectileBasic.hbTouch = cardHitbox;
                if (player.powerupTimer > 0)
                {
                    projectileBasic.attackPower = 5f * player.GetAttackModifier();
                    projectileBasic.damageElementType = 4;
                    projectileBasic.explodeType = FPExplodeType.METALBURST;
                    projectileBasic.animatorController = ironCardAnimator[UnityEngine.Random.RandomRangeInt(0, 3)];
                    projectileBasic.animator = projectileBasic.GetComponent<Animator>();
                    projectileBasic.animator.runtimeAnimatorController = projectileBasic.animatorController;
                }

                if (shadowTimer > 0)
                {
                    ProjectileBasic shadowCard;
                    if (player.direction == FPDirection.FACING_LEFT)
                    {
                        shadowCard = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x - Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * num, player.position.y + Mathf.Cos(0.017453292f * player.angle) * num - Mathf.Sin(0.017453292f * player.angle) * 32f);
                        shadowCard.velocity.x = Mathf.Cos(0.017453292f * num2) * -10f;
                        shadowCard.velocity.y = Mathf.Sin(0.017453292f * num2) * -16f;
                    }
                    else
                    {
                        shadowCard = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x + Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * num, player.position.y + Mathf.Cos(0.017453292f * player.angle) * num + Mathf.Sin(0.017453292f * player.angle) * 32f);
                        shadowCard.velocity.x = Mathf.Cos(0.017453292f * num2) * 10f;
                        shadowCard.velocity.y = Mathf.Sin(0.017453292f * num2) * 16f;
                    }
                    shadowCard.animatorController = shadowCardAnimator;
                    shadowCard.attackPower = 1.75f * player.GetAttackModifier();
                    shadowCard.animator = shadowCard.GetComponent<Animator>();
                    shadowCard.animator.runtimeAnimatorController = shadowCard.animatorController;
                    shadowCard.direction = player.direction;
                    shadowCard.damageElementType = -1;
                    shadowCard.scale = new Vector2(0.8f, 0.8f);
                    shadowCard.angle = num2;
                    shadowCard.ignoreTerrain = false;
                    shadowCard.explodeType = FPExplodeType.WHITEBURST;
                    shadowCard.explodeTimer = 50f;
                    shadowCard.terminalVelocity = 0f;
                    shadowCard.gravityStrength = 0;
                    shadowCard.sfxExplode = null;
                    shadowCard.parentObject = player;
                    shadowCard.faction = player.faction;
                    shadowCard.timeBeforeCollisions = 0f;
                    shadowCard.hbTouch = cardHitbox;
                    if (player.powerupTimer > 0)
                    {
                        shadowCard.attackPower = 2.5f * player.GetAttackModifier();
                        shadowCard.damageElementType = 4;
                        shadowCard.explodeType = FPExplodeType.METALBURST;
                    }
                } 

                cardAngle++;
                Action_PlaySound(sfxThrowCard, 0.3f);

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
                projectileBasic.attackPower = 5f * player.GetAttackModifier();
                projectileBasic.animator = projectileBasic.GetComponent<Animator>();
                projectileBasic.animator.runtimeAnimatorController = projectileBasic.animatorController;
                projectileBasic.direction = FPDirection.FACING_RIGHT;
                projectileBasic.damageElementType = -1;
                projectileBasic.explodeType = FPExplodeType.WHITEBURST;
                projectileBasic.sfxExplode = null;
                projectileBasic.gravityStrength = 0;
                projectileBasic.terminalVelocity = 0f;
                projectileBasic.explodeTimer = 50f;
                projectileBasic.parentObject = player;
                projectileBasic.faction = player.faction;
                projectileBasic.hbTouch = cardHitbox;
                if (player.powerupTimer > 0)
                {
                    projectileBasic.attackPower = 10f * player.GetAttackModifier();
                    projectileBasic.damageElementType = 4;
                    projectileBasic.explodeType = FPExplodeType.METALBURST;
                    projectileBasic.animatorController = ironDualCardAnimator;
                    projectileBasic.animator = projectileBasic.GetComponent<Animator>();
                    projectileBasic.animator.runtimeAnimatorController = projectileBasic.animatorController;
                }
                if (shadowTimer > 0)
                {
                    ProjectileBasic shadowCard;
                    if (player.direction == FPDirection.FACING_LEFT)
                    {
                        shadowCard = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x - Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * num, player.position.y + Mathf.Cos(0.017453292f * player.angle) * num - Mathf.Sin(0.017453292f * player.angle) * 32f);
                        shadowCard.velocity.x = Mathf.Cos(0.017453292f * num2) * -25f;
                        shadowCard.velocity.y = Mathf.Sin(0.017453292f * num2) * 20f;
                        shadowCard.angle = 180f - num2;
                    }
                    else
                    {
                        shadowCard = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x + Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * num, player.position.y + Mathf.Cos(0.017453292f * player.angle) * num + Mathf.Sin(0.017453292f * player.angle) * 32f);
                        shadowCard.velocity.x = Mathf.Cos(0.017453292f * num2) * 25f;
                        shadowCard.velocity.y = Mathf.Sin(0.017453292f * num2) * 20f;
                        shadowCard.angle = num2;
                    }
                    shadowCard.animatorController = shadowDualCardAnimator;
                    shadowCard.attackPower = 5f * player.GetAttackModifier();
                    shadowCard.animator = shadowCard.GetComponent<Animator>();
                    shadowCard.animator.runtimeAnimatorController = shadowCard.animatorController;
                    shadowCard.direction = FPDirection.FACING_RIGHT;
                    shadowCard.damageElementType = -1;
                    shadowCard.explodeType = FPExplodeType.WHITEBURST;
                    shadowCard.sfxExplode = null;
                    shadowCard.gravityStrength = 0;
                    shadowCard.terminalVelocity = 0f;
                    shadowCard.explodeTimer = 50f;
                    shadowCard.parentObject = player;
                    shadowCard.faction = player.faction;
                    shadowCard.hbTouch = cardHitbox;
                    if (player.powerupTimer > 0)
                    {
                        shadowCard.attackPower = 10f * player.GetAttackModifier();
                        shadowCard.damageElementType = 4;
                        shadowCard.explodeType = FPExplodeType.METALBURST;
                    }
                }
                Action_PlaySound(sfxThrowDualCard, 0.7f);
                crashTimer = 0;   
            }
            player.Process360Movement();
        }

        public static void Action_Spade_ShadowGuard()
        {
            if (player.IsPowerupActive(FPPowerup.SHADOW_GUARD))
            {
                shadowTimer = 120f;
            }
        }

        internal static void Action_Spade_FuelPickup()
        {
            player.powerupTimer = Mathf.Max(player.powerupTimer, 600f);
            player.flashTime = Mathf.Max(player.flashTime, 600f);
            FPAudio.PlaySfx(sfxThrowCard);
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
            if (player.input.up || player.input.upPress)
            {
                player.velocity.y = Mathf.Max(Mathf.Min(player.velocity.y + dashSpeed, 12f), player.velocity.y);
                upDash = false;
                player.genericTimer = 0;
                ghostTimer = 0;
                player.state = new FPObjectState(State_Spade_AirDash);
            }
            else if ((player.input.down || player.input.downPress) && !(player.onGrindRail || player.onGround))
            {
                upDash = false;
                player.genericTimer = 0;
                ghostTimer = 0;
                player.state = new FPObjectState(State_Spade_GroundPound);
            }
            else if (player.direction == FPDirection.FACING_RIGHT)
            {
                if (player.onGround)
                {
                    player.groundVel = Mathf.Max(Mathf.Min(player.groundVel + dashSpeed, 18f), player.groundVel);
                }
                else
                {
                    player.velocity.x = Mathf.Max(Mathf.Min(player.velocity.x + dashSpeed, 18f), player.velocity.x);
                    upDash = false;
                    player.genericTimer = 0;
                    ghostTimer = 0;
                    player.state = new FPObjectState(State_Spade_AirDash);
                }
            }
            else if (player.onGround)
            {
                player.groundVel = Mathf.Min(Mathf.Max(player.groundVel - dashSpeed, -18f), player.groundVel);
            }
            else
            {
                player.velocity.x = Mathf.Min(Mathf.Max(player.velocity.x - dashSpeed, -18f), player.velocity.x);
                upDash = false;
                player.genericTimer = 0;
                ghostTimer = 0;
                player.state = new FPObjectState(State_Spade_AirDash);
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
            else if (player.input.jumpPress && !player.onGround && upDash && Plugin.configDashOnDoubleJump.Value && player.energy > 25 && dashTime <= 0f)
            {
                if (!player.input.down || !player.input.downPress)
                {
                    FPAudio.PlaySfx(15);
                    dashTime = 40f;
                    Action_Spade_Dash(45f);
                    player.energy -= 25f;
                } 
            }
            else if ((player.guardTime <= 0f || player.cancellableGuard) && (player.input.guardPress || (guardBuffer > 0f && player.input.guardHold)))
            {
                player.Action_Guard(0f,false);
                Action_Spade_ShadowGuard();
                if (player.energy > 25 && !autoGuard && dashTime <= 0f && upDash && (player.input.left || player.input.right || player.input.up || player.input.down) && !Plugin.configDashOnDoubleJump.Value)
                {
                    FPAudio.PlaySfx(15);
                    GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                    guardFlash.parentObject = player;
                    guardFlash.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
                    dashTime = 40f;
                    Action_Spade_Dash(45f);
                    player.energy -= 25f;
                }
                else if (!player.IsPowerupActive(FPPowerup.NO_GUARDING))
                {
                    FPAudio.PlaySfx(15);
                    player.SetPlayerAnimation("GuardAir");
                    player.animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(player.velocity.x * 0.05f)));
                    GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                    guardFlash.parentObject = player;
                    guardFlash.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                    dashTime = 40f;
                }
            }
        }

        public static void Action_Spade_GroundMoves()
        {
            upDash = true;
            if ((player.input.attackPress || player.input.attackHold && player.state != State_ThrowCards)) //Base Card Throw
            {
                player.idleTimer = -player.fightStanceTime;
                player.state = new FPObjectState(State_ThrowCards);
            }
            else if ((player.input.specialPress || player.input.specialHold) && !(player.input.up || player.input.down) && player.state != State_Spade_CaptureCard)
            {
                player.idleTimer = -player.fightStanceTime;
                if (player.energy > 25)
                {
                    player.energy -= 25f;
                    player.genericTimer = 0f;
                    Action_SpadeThrowCaptureCard();
                    player.state = new FPObjectState(State_Spade_CaptureCard);
                }
            }
            else if ((player.guardTime <= 0f || player.cancellableGuard) && (player.input.guardPress || (guardBuffer > 0f && player.input.guardHold)))
            {                
                player.Action_Guard(0f, false);
                Action_Spade_ShadowGuard();
                if (player.energy > 25 && !autoGuard && dashTime <= 0f && upDash && (player.input.left || player.input.right || player.input.up || player.input.down))
                {
                    FPAudio.PlaySfx(15);
                    GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                    guardFlash.parentObject = player;
                    guardFlash.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
                    dashTime = 40f;
                    Action_Spade_Dash(90f);
                    player.energy -= 25f;
                }
                else if (!player.IsPowerupActive(FPPowerup.NO_GUARDING)) {
                    FPAudio.PlaySfx(15);
                    GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                    guardFlash.parentObject = player;
                    guardFlash.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                    dashTime = 40f;
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

        public static void AttackStats_Dash()
        {
            AttackStats_Blink(player);
        }

        private static void Ghost()
        {
            Color start = new Color(0f, 1f, 0f, 0.5f);
            Color end = new Color(0f, 1f, 0f, 0f);
            SpriteGhost spriteGhost = (SpriteGhost)FPStage.CreateStageObject(SpriteGhost.classID, player.transform.position.x, player.transform.position.y);
            spriteGhost.sprite.material = Plugin.moddedBundle.LoadAsset<Material>("SpadeTrail");
            spriteGhost.transform.rotation = player.transform.rotation;
            spriteGhost.SetUp(player.gameObject.GetComponent<SpriteRenderer>().sprite, start, end, 0.5f, 3f);
            spriteGhost.transform.localScale = player.transform.localScale;
            spriteGhost.maxLifeTime = 0.5f;
            spriteGhost.growSpeed = 0f;
            spriteGhost.activationMode = FPActivationMode.ALWAYS_ACTIVE;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FPPlayer), "AttackStats_Blink", MethodType.Normal)]
        public static void AttackStats_Blink(FPPlayer instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FPPlayer), "ApplyGroundForces", MethodType.Normal)]
        public static void ApplyGroundForces(FPPlayer instance, bool ignoreDirectionalInput)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FPPlayer), "ApplyWaterForces", MethodType.Normal)]
        public static void ApplyWaterForces(FPPlayer instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Update", MethodType.Normal)]
        static void PatchPlayerUpdate(FPPlayer __instance, float ___speedMultiplier)
        {
            cardTimer += FPStage.deltaTime;
            crashTimer += FPStage.deltaTime;
            player = __instance;
            speedMultiplier = ___speedMultiplier;
            if (player.onGround) upDash = true;
            if (player.guardTime <= 0f) autoGuard = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "LateUpdate", MethodType.Normal)]
        static void PatchPlayerLateUpdate(float ___guardBuffer)
        {
            guardBuffer = ___guardBuffer;

            if (shadowTimer > 0f)
            {
                shadowTimer -= FPStage.deltaTime;
            }

            if (dashTime > 0f)
            {
                dashTime -= FPStage.deltaTime;
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "Start", MethodType.Normal)]
        static void PatchPlayerStart(FPPlayer __instance)
        {

            cardAnimator = cardAnimator.AddItem(Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("ThrowingCard0")).ToArray();
            cardAnimator = cardAnimator.AddItem(Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("ThrowingCard1")).ToArray();
            cardAnimator = cardAnimator.AddItem(Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("ThrowingCard2")).ToArray();
            cardAnimator = cardAnimator.AddItem(Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("ThrowingCard3")).ToArray();

            ironCardAnimator = ironCardAnimator.AddItem(Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("IronCard1")).ToArray();
            ironCardAnimator = ironCardAnimator.AddItem(Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("IronCard2")).ToArray();
            ironCardAnimator = ironCardAnimator.AddItem(Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("IronCard3")).ToArray();
            ironCardAnimator = ironCardAnimator.AddItem(Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("IronCard4")).ToArray();

            shadowCardAnimator = Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("ShadowCard");
            shadowCardAnimator.hideFlags = HideFlags.DontUnloadUnusedAsset;

            dualCardAnimator = Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("PowerCard");
            dualCardAnimator.hideFlags = HideFlags.DontUnloadUnusedAsset;

            ironDualCardAnimator = Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("IronPowerCard");
            ironDualCardAnimator.hideFlags = HideFlags.DontUnloadUnusedAsset;

            shadowDualCardAnimator = Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("ShadowPowerCard");
            shadowDualCardAnimator.hideFlags = HideFlags.DontUnloadUnusedAsset;

            captureCardAnimator = Plugin.moddedBundle.LoadAsset<RuntimeAnimatorController>("ThrowingCardFP1");
            captureCardAnimator.hideFlags = HideFlags.DontUnloadUnusedAsset;

            sfxThrowCard = Plugin.moddedBundle.LoadAsset<AudioClip>("spade_card_toss");
            sfxThrowDualCard = Plugin.moddedBundle.LoadAsset<AudioClip>("DiscThrow");

            GameObject.Instantiate(Plugin.moddedBundle.LoadAsset<GameObject>("DashGhost"));

            GameObject.Instantiate(Plugin.moddedBundle.LoadAsset<GameObject>("SpadeCaptureCard"));

            player = __instance;
            upDash = true;

            if (FPStage.stageNameString == "Lunar Cannon")
            {
                player.blueFlashMat = FPStage.player[0].blueFlashMat;
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "AutoGuard", MethodType.Normal)]
        static void PatchPlayerHurt(FPPlayer __instance)
        {
            if (FPSaveManager.assistGuard == 1 && !__instance.IsPowerupActive(FPPowerup.NO_GUARDING) && __instance.guardTime <= 0f && (__instance.state == new FPObjectState(__instance.State_Ground) || __instance.state == new FPObjectState(__instance.State_InAir) || __instance.state == new FPObjectState(__instance.State_LookUp) || __instance.state == new FPObjectState(__instance.State_Crouching) || __instance.state == new FPObjectState(__instance.State_Swimming)))
            {
                autoGuard = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "State_InAir", MethodType.Normal)]
        static void PatchPlayerInAir()
        {
            if (player.currentAnimation == "AirThrow" && player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.8f)
            {
                player.SetPlayerAnimation("Jumping");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "State_Hanging", MethodType.Normal)]
        static void PatchPlayerHang()
        {
            upDash = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "State_Swimming", MethodType.Normal)]
        static void PatchPlayerSwim()
        {
            upDash = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "State_GrindRail", MethodType.Normal)]
        [HarmonyPatch(typeof(FPPlayer), "PseudoGrindRail", MethodType.Normal)]
        static void PatchPlayerGrind()
        {
            upDash = true;
        }
    }
}
