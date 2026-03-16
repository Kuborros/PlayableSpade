using UnityEngine;
using Random = UnityEngine.Random;

namespace PlayableSpade.Objects
{
    internal class SpadeCaptureCard : FPBaseObject
    {
        public struct TargetData
        {
            public FPBaseEnemy enemy;

            public Vector2 offset;
        }

        public static int classID = -1;

        public TargetData target;

        [Header("Projectile Settings")]
        public float explodeTimer = -1f;

        [HideInInspector]
        public bool collision;

        [HideInInspector]
        public bool enemyCollision;

        [HideInInspector]
        public bool animationFinished;

        [HideInInspector]
        public bool ignoreTerrain;

        public FPHitBox hbTouch = new FPHitBox { enabled = true, left = -10f, top = 10, right = 10, bottom = -10f };

        public float attackPower = 5f;

        public float speed = 15f;

        public float turnSpeed = 15f;

        public float trackTimer = 15f;

        private float jinkTimer = 0.5f;

        public float jinkPeriod = 0.2f;

        public string faction;

        public FPBaseObject owner;

        [HideInInspector]
        public Vector2 attackKnockback;

        private LineRenderer line;

        private int trailPoints;

        private float trailTimer = 0.2f;

        private Vector3 lastPosition;

        private Vector2[] tPoints;

        public Vector2 offset = Vector2.zero;

        public Color trailColor = Color.white;

        private Texture initTex;

        private float lifeTime;

        private float startWidth;

        private new void Start()
        {
            line = GetComponent<LineRenderer>();
            line.sortingOrder = 1;
            trailPoints = line.positionCount;
            tPoints = new Vector2[trailPoints];
            for (int i = 0; i < trailPoints; i++)
            {
                line.SetPosition(i, transform.position);
                ref Vector2 reference = ref tPoints[i];
                reference = transform.position;
            }
            lastPosition = transform.position;
            initTex = line.material.GetTexture("_MainTex");
            startWidth = line.widthMultiplier;
            line.useWorldSpace = true;
            jinkTimer = jinkPeriod;
            base.Start();
            classID = FPStage.RegisterObjectType(this, GetType(), 32);
            objectID = classID;
        }

        public override void ResetStaticVars()
        {
            base.ResetStaticVars();
            classID = -1;
        }

        public override void ObjectCreated()
        {
            activationMode = FPActivationMode.ALWAYS_ACTIVE;
            explodeTimer = 180f;
            animationFinished = false;
            collision = false;
            enemyCollision = false;
            ignoreTerrain = false;
            target.enemy = null;
            target.offset = Vector2.zero;
            trackTimer = 15f;
            jinkTimer = jinkPeriod;
            hbTouch.enabled = true;
            trailPoints = line.positionCount;
            tPoints = new Vector2[trailPoints];
            for (int i = 0; i < trailPoints; i++)
            {
                line.SetPosition(i, transform.position);
                ref Vector2 reference = ref tPoints[i];
                reference = transform.position;
            }
            lastPosition = transform.position;
            trailColor = Color.white;
            SetTexture(initTex);
            lifeTime = 0f;
        }

        private void SetTexture(Texture t)
        {
            line.material.SetTexture("_MainTex", t);
        }

        private void Update()
        {
            if (FPStage.objectsRegistered)
            {
                if (!collision)
                {
                    State_Default();
                }
                else
                {
                    State_Done();
                }
                line.startColor = trailColor;
            }
        }

        private void State_Default()
        {
            trackTimer -= FPStage.deltaTime;
            if (target.enemy != null && target.enemy.isActiveAndEnabled && trackTimer <= 0f)
            {
                Vector2 vector = target.enemy.position + target.offset - position;
                Quaternion b = Quaternion.AngleAxis(Mathf.Atan2(vector.y, vector.x) * 57.29578f, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, b, turnSpeed / 60f * FPStage.deltaTime);
            }
            trailColor = Color.white;
            jinkTimer -= FPStage.deltaTime / 60f;
            if (jinkTimer <= 0f)
            {
                if (target.enemy != null)
                {
                    if (Vector2.Distance(transform.position, target.enemy.position + target.offset) > 64f)
                    {
                        Quaternion b2 = Quaternion.AngleAxis(transform.eulerAngles.z + (float)Random.Range(0, 120) - 60f, Vector3.forward);
                        transform.rotation = Quaternion.Slerp(transform.rotation, b2, 1f);
                        jinkTimer = jinkPeriod;
                        target.offset = GetTargetOffset(target.enemy);
                    }
                    if (target.enemy.health <= 0f)
                    {
                        target.enemy = null;
                        target.offset = Vector2.zero;
                        DropTarget();
                    }
                }
                if (target.enemy == null)
                {
                    Quaternion b3 = Quaternion.AngleAxis(transform.eulerAngles.z + (float)Random.Range(0, 120) - 60f, Vector3.forward);
                    transform.rotation = Quaternion.Slerp(transform.rotation, b3, 1f);
                    jinkTimer = jinkPeriod;
                }
            }
            velocity = new Vector2(transform.right.x * speed, transform.right.y * speed);
            attackKnockback = new Vector2(velocity.x * 0.5f, velocity.y * 0.5f);
            velocity *= FPStage.deltaTime;
            position += velocity;
            if (explodeTimer > -1f)
            {
                explodeTimer -= FPStage.deltaTime;
                if (explodeTimer < 0f)
                {
                    Collide();
                    return;
                }
            }
            if (enemyCollision)
            {
                Collide();
            }
            if (!ignoreTerrain && FPCollision.CheckTerrainCircleThroughPlatforms(this, 10f))
            {
                Collide();
            }
            trailTimer -= FPStage.deltaTime / 60f;
            if (trailTimer <= 0f)
            {
                AddPosition();
                UpdateTrail();
                trailTimer = 0.01f;
            }
            lifeTime += FPStage.deltaTime / 30f;
            line.widthMultiplier = Mathf.Min(1f, lifeTime) * startWidth;
            offset.x -= Time.deltaTime * speed;
            line.material.SetTextureOffset("_MainTex", offset);
            if (lifeTime < 0.15f)
            {
                return;
            }
            for (int num = trailPoints - 1; num > 0; num--)
            {
                Vector2 vector2 = Vector2.zero;
                if (num != 0)
                {
                    vector2 = tPoints[num] - tPoints[0];
                    vector2.Normalize();
                }
                tPoints[num] += vector2 * Time.deltaTime * speed * 60f;
            }

            //Stop effect like Dail's FP1 cards
            if (explodeTimer % 30 > 10f)
            {
                speed = 0;
                turnSpeed = 150f;
            }
            else
            {
                speed = 20;
                turnSpeed = 15f;
            }

            //Armored enemies (aka bosses) take reduced damage
            if (target.enemy != null)
            {
                if (target.enemy.cannotBeFrozen == true)
                {
                    attackPower = 1;
                }
                else attackPower = 3;
            }

        }

        private void AddPosition()
        {
            for (int num = trailPoints - 1; num > 0; num--)
            {
                if (num > 0)
                {
                    ref Vector2 reference = ref tPoints[num];
                    reference = tPoints[num - 1];
                }
            }
            ref Vector2 reference2 = ref tPoints[0];
            reference2 = transform.position;
        }

        private void UpdateTrail()
        {
            for (int i = 0; i < trailPoints; i++)
            {
                line.SetPosition(i, tPoints[i]);
            }
            lastPosition = transform.position;
        }

        public void Collide()
        {
            collision = true;
            FPAudio.PlaySfx(25);
            FPStage.CreateStageObject(Explosion.classID, position.x, position.y);
        }

        private void State_Done()
        {
            FPStage.DestroyStageObject(this);
        }

        public void AssignTarget(FPBaseEnemy enemy, Vector2 offset)
        {
            target.enemy = enemy;
            target.offset = offset;
        }

        public void DropTarget()
        {
            target.enemy = null;
            target.offset = Vector2.zero;
        }

        private static Vector2 GetTargetOffset(FPBaseEnemy enemy)
        {
            FPHitBox hbWeakpoint = enemy.hbWeakpoint;
            if (enemy.GetComponent<MonsterCube>() != null)
            {
                hbWeakpoint = enemy.GetComponent<MonsterCube>().childWeakpoint.hbWeakpoint;
            }
            return new Vector2(Random.Range(hbWeakpoint.left, hbWeakpoint.right), Random.Range(hbWeakpoint.bottom, hbWeakpoint.top));
        }
    }
}
