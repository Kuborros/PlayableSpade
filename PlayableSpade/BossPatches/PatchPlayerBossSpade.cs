using HarmonyLib;
using System;
using UnityEngine;
using static UnityEngine.Random;

namespace PlayableSpade.BossPatches
{
    internal class PatchPlayerBossSpade
    {
        internal static PlayerBossSpade instance;

        protected static float cardTimer;
        protected static float crashTimer;
        protected static float wallCheckTimer;
        protected static float dashTime = 0f;
        protected static float ghostTimer = 0f;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerBossSpade), "Start", MethodType.Normal)]
        static void PatchPlayerBossSpadeStart(ref PlayerBossSpade __instance, ref Vector2 ___start)
        {
            //Mirror Match
            if (FPSaveManager.character == PlayableSpade.spadeCharID)
            {
                SpriteRenderer playerSprite = __instance.GetComponent<SpriteRenderer>();
                if (playerSprite != null)
                {
                    playerSprite.color = new Color(0f, 1f, 1f);
                }
                SpriteOutline outline = __instance.GetComponent<SpriteOutline>();
                if (outline != null)
                {
                    outline.enabled = true;
                }
            }

            if (FPStage.stageNameString == "Training")
            {
                ___start = new Vector2(514, -336);
            }

            instance = __instance;
            GameObject.Instantiate(PlayableSpade.moddedBundle.LoadAsset<GameObject>("DashGhost"));
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerBossSpade), "State_Default", MethodType.Normal)]
        static bool PatchPlayerBossSpadeDefault(PlayerBossSpade __instance)
        {
            __instance.SetPlayerAnimation("FightStance");
            __instance.spriteRenderer.sprite = null;
            if (!__instance.bossActivated && FPStage.timeEnabled && __instance.targetPlayer != null
                && __instance.targetPlayer.position.x > __instance.position.x - __instance.bossActivation.x && __instance.targetPlayer.position.x < __instance.position.x + __instance.bossActivation.x 
                && __instance.targetPlayer.position.y > __instance.position.y - __instance.bossActivation.y && __instance.targetPlayer.position.y < __instance.position.y + __instance.bossActivation.y)
            {
                if (__instance.bgmBoss != null)
                {
                    FPAudio.PlayMusic(__instance.bgmBoss);
                }
                __instance.Action_PlayVoice(__instance.vaStart[Range(0, __instance.vaStart.Length - 1)]);
                __instance.isTalking = true;
                __instance.voiceTimer = 240f;
                __instance.bossActivated = true;
                FPBossHud component = __instance.GetComponent<FPBossHud>();
                if (component != null)
                {
                    component.MoveIn();
                }
                __instance.genericTimer = -30f;
                __instance.state = State_Running;
            }
            /*
            if (!FPStage.ConfirmClassWithPoolTypeID(typeof(FPPlayer), FPPlayer.classID))
            {
                __instance.bossActivated = true;
                FPBossHud component2 = __instance.GetComponent<FPBossHud>();
                if (component2 != null)
                {
                    component2.MoveIn();
                }
                __instance.state = State_Running;
            }
            */
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerBossSpade), "State_Running", MethodType.Normal)]
        static void PatchPlayerBossSpadeRun(PlayerBossSpade __instance)
        {
            if (__instance.targetToPursue == null)
            {
                //Set a target
                if (__instance.faction != "Player")
                    __instance.targetToPursue = FPStage.FindNearestPlayer(__instance, __instance.pursuitRange);
                else
                    __instance.targetToPursue = FPStage.FindNearestEnemy(__instance, __instance.pursuitRange);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerBossSpade), "State_ThrowCards", MethodType.Normal)]
        static bool PatchPlayerBossSpadeCardThrow(PlayerBossSpade __instance, ref int ___nextMotion, ref int ___cardAngle)
        {
            if (__instance.faction != "Player")
            {
                __instance.Action_FacePlayer();
                CheckBoundaries(__instance);
            }

            if (cardTimer > 20)
            {
                if (__instance.currentAnimation == "Crouching" || (__instance.input.down && __instance.currentAnimation == "CrouchThrow"))
                {
                    __instance.SetPlayerAnimation("CrouchThrow");
                }
                else if (__instance.onGround)
                {
                    __instance.SetPlayerAnimation("Throw");
                }
                else if (!__instance.onGround)
                {
                    __instance.SetPlayerAnimation("AirThrow");
                }
                for (int i = 1; i <= 3; i++)
                    Action_ThrowCard(__instance);
                cardTimer = 0;
                ___cardAngle = 0;

            }
            __instance.genericTimer += FPStage.deltaTime;
            if (__instance.genericTimer > 70f && __instance.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.8f)
            {
                if (__instance.onGround)
                {
                    ___nextMotion = Range(0, 3);
                    if (___nextMotion == 0)
                    {
                        __instance.state = State_Idle;
                        __instance.genericTimer = 0f;
                    }
                    else
                    {
                        __instance.state = State_Running;
                        __instance.genericTimer = Range(-20f, 40f);
                    }
                }
                else
                {
                    __instance.SetPlayerAnimation("Jumping", 0.5f, 0.5f);
                    ___nextMotion = Range(0, 3);
                    if (___nextMotion == 0)
                    {
                        __instance.state = State_Idle;
                        __instance.genericTimer = 0f;
                    }
                    else
                    {
                        __instance.state = State_Running;
                        __instance.genericTimer = 0f;
                    }
                }
            }
            CheckBoundaries(__instance);
            InteractWithObjects(__instance);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerBossSpade), "State_DualCrash", MethodType.Normal)]
        static bool PatchPlayerBossSpadeCardCrash(PlayerBossSpade __instance, ref int ___targetDir)
        {
            __instance.genericTimer += FPStage.deltaTime;
            __instance.velocity.y = 0f;
            __instance.angle = 0f;
            if (___targetDir == 1)
            {
                __instance.velocity.x = Mathf.Min(__instance.velocity.x + __instance.acceleration * FPStage.deltaTime, 10f);
            }
            else
            {
                __instance.velocity.x = Mathf.Max(__instance.velocity.x - __instance.acceleration * FPStage.deltaTime, -10f);
            }
            if (__instance.position.x < __instance.start.x + __instance.walkRange.left)
            {
                ___targetDir = 1;
            }
            else if (__instance.position.x > __instance.start.x + __instance.walkRange.right)
            {
                ___targetDir = -1;
            }
            if (__instance.genericTimer > 120f)
            {
                __instance.genericTimer = -60f;
                if (Range(0, 100) > 33)
                {
                    __instance.state = State_Jumping;
                }
                else
                {
                    __instance.state = State_Spade_GroundPound;
                }
            }
            InteractWithObjects(__instance);
            CheckBoundaries(__instance);
            __instance.Process360Movement();

            if (crashTimer > 3.5f)
            {
                Action_ThrowDualCard(__instance);
                crashTimer = 0;
            }

            return false;
        }

        private static void State_Spade_GroundPound()
        {
            bool shockwave = false;
            instance.genericTimer += FPStage.deltaTime;
            instance.SetPlayerAnimation("GroundPoundAir");
            instance.superArmor = true;
            ghostTimer += FPStage.deltaTime;
            instance.attackStats = new FPObjectState(AttackStats_Dash);

            if (ghostTimer >= 1f)
            {
                Ghost();
                ghostTimer = 0f;
            }

            instance.velocity.y = -20f;
            instance.velocity.x = 0f;
            instance.Process360Movement();

            if (instance.onGround || instance.genericTimer >= 100f)
            {
                instance.hbAttack.enabled = false;
                instance.superArmor = false;
                if (!shockwave && instance.onGround)
                {
                    StingerBomb stingerBomb = (StingerBomb)FPStage.CreateStageObject(StingerBomb.classID, instance.position.x, (instance.position.y - instance.halfHeight) - 5);
                    stingerBomb.explodeTimer = 999f;
                    stingerBomb.faction = instance.faction;
                    shockwave = true;
                }

                dashTime += 10f;

                if (instance.onGround)
                {
                    instance.state = new FPObjectState(State_Rolling);
                }
                else
                {
                    instance.state = new FPObjectState(State_Jumping);
                }
                return;
            }
        }

        private static void Ghost()
        {
            Color start = new Color(0f, 1f, 0f, 0.5f);
            Color end = new Color(0f, 1f, 0f, 0f);
            SpriteGhost spriteGhost = (SpriteGhost)FPStage.CreateStageObject(SpriteGhost.classID, instance.transform.position.x, instance.transform.position.y);
            spriteGhost.sprite.material = PlayableSpade.moddedBundle.LoadAsset<Material>("SpadeTrail");
            spriteGhost.transform.rotation = instance.transform.rotation;
            spriteGhost.SetUp(instance.gameObject.GetComponent<SpriteRenderer>().sprite, start, end, 0.5f, 3f);
            spriteGhost.transform.localScale = instance.transform.localScale;
            spriteGhost.maxLifeTime = 0.5f;
            spriteGhost.growSpeed = 0f;
            spriteGhost.activationMode = FPActivationMode.ALWAYS_ACTIVE;
        }

        private static void CheckBoundaries(PlayerBossSpade instance)
        {
            if (instance.faction != "Player")
            {
                FPPlayer fPPlayer = FPStage.FindNearestPlayer(instance, 640f);
                if (fPPlayer != null)
                {
                    if (instance.invincibility > 0f)
                    {
                        if (instance.position.x > fPPlayer.position.x + 10f)
                        {
                            instance.direction = FPDirection.FACING_LEFT;
                        }
                        else if (instance.position.x < fPPlayer.position.x - 10f)
                        {
                            instance.direction = FPDirection.FACING_RIGHT;
                        }
                    }
                    else if (instance.state == new FPObjectState(State_Running))
                    {
                        if (instance.position.x > fPPlayer.position.x + instance.pursuitRange)
                        {
                            instance.direction = FPDirection.FACING_LEFT;
                        }
                        else if (instance.position.x < fPPlayer.position.x - instance.pursuitRange)
                        {
                            instance.direction = FPDirection.FACING_RIGHT;
                        }
                    }
                }
            }
            else if (instance.targetToPursue != null)
            {
                if (instance.invincibility > 0f)
                {
                    if (instance.position.x > instance.targetToPursue.position.x + 10f)
                    {
                        instance.direction = FPDirection.FACING_LEFT;
                    }
                    else if (instance.position.x < instance.targetToPursue.position.x - 10f)
                    {
                        instance.direction = FPDirection.FACING_RIGHT;
                    }
                }
                else if (instance.state == new FPObjectState(State_Running))
                {
                    if (instance.position.x > instance.targetToPursue.position.x + instance.pursuitRange)
                    {
                        instance.direction = FPDirection.FACING_LEFT;
                    }
                    else if (instance.position.x < instance.targetToPursue.position.x - instance.pursuitRange)
                    {
                        instance.direction = FPDirection.FACING_RIGHT;
                    }
                }
            }
            if (instance.walkRange.enabled && instance.position.x > instance.start.x + instance.walkRange.right)
            {
                instance.direction = FPDirection.FACING_LEFT;
            }
            else if (instance.walkRange.enabled && instance.position.x < instance.start.x + instance.walkRange.left)
            {
                instance.direction = FPDirection.FACING_RIGHT;
            }
            if (instance.colliderWall != null && wallCheckTimer > 20f)
            {
                if (instance.onGround)
                {
                    instance.groundVel = 0f - instance.groundVel;
                }
                else
                {
                    instance.velocity.x = 0f - instance.prevVelocity.x;
                }
                instance.direction ^= FPDirection.FACING_RIGHT;
                wallCheckTimer = 0f;
            }
            if (instance.direction == FPDirection.FACING_RIGHT)
            {
                instance.input.right = true;
                instance.input.left = false;
            }
            else
            {
                instance.input.left = true;
                instance.input.right = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerBossSpade), "LateUpdate", MethodType.Normal)]
        static void PatchBossSpadeLateUpdate(PlayerBossSpade __instance)
        {
            if (__instance.energy < 100f)
            {
                __instance.energy += 0.6f * FPStage.deltaTime;
            }

            cardTimer += FPStage.deltaTime;
            crashTimer += FPStage.deltaTime;
            wallCheckTimer += FPStage.deltaTime;

            if (dashTime > 0f)
            {
                dashTime -= FPStage.deltaTime;
            }
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PlayerBossSpade), "InteractWithObjects", MethodType.Normal)]
        public static void InteractWithObjects(PlayerBossSpade instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        public static void State_Idle() => State_Idle(instance);
        public static void State_Running() => State_Running(instance);
        public static void State_Jumping() => State_Jumping(instance);
        public static void State_Rolling() => State_Rolling(instance);

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PlayerBossSpade), "State_Idle", MethodType.Normal)]
        public static void State_Idle(PlayerBossSpade instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PlayerBossSpade), "State_Running", MethodType.Normal)]
        public static void State_Running(PlayerBossSpade instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PlayerBossSpade), "State_Jumping", MethodType.Normal)]
        public static void State_Jumping(PlayerBossSpade instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PlayerBossSpade), "State_Rolling", MethodType.Normal)]
        public static void State_Rolling(PlayerBossSpade instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PlayerBossSpade), "Action_ThrowCard", MethodType.Normal)]
        public static void Action_ThrowCard(PlayerBossSpade instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PlayerBossSpade), "Action_ThrowDualCard", MethodType.Normal)]
        public static void Action_ThrowDualCard(PlayerBossSpade instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        public static void AttackStats_Dash() => AttackStats_Blink(instance);

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PlayerBoss), "AttackStats_Blink", MethodType.Normal)]
        public static void AttackStats_Blink(PlayerBoss instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }
    }
}
