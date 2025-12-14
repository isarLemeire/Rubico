using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerController
{
    // Single container for all data accessible to states.
    public class PlayerStateContext
    {
        public PlayerScriptableStats Stats { get; private set; }
        public PlayerBoxCollisionHandler CollisionHandler;

        // persistent runtime state
        public BufferedBool grounded;
        public BufferedBool dashBuffer;
        public bool canDash;
        public bool canAttack;
        public bool canKnockBack;
        public float dashCoolDownTimer;
        public bool canEngage;

        public bool Invulnerable;

        public bool inputBlock;

        public int HP;
        public int heal;



        public Vector3 speed;
        public Vector2 attackAim;
        public Vector2 dashAim;
        public int attackCombo;

        public GameObject lastCheckpoint;
        public RaycastHit2D lastHazardHit;

        public PlayerStateContext(PlayerScriptableStats stats, PlayerBoxCollisionHandler collisionHandler)
        {
            this.Stats = stats;
            this.CollisionHandler = collisionHandler;
            this.speed = speed = Vector3.zero;
            canDash = true;
            dashCoolDownTimer = -1f;
            grounded = new BufferedBool(stats.ForgivenessTime);
            dashBuffer = new BufferedBool(stats.ForgivenessTime);
            canKnockBack = true;
            attackCombo = 0;
            canAttack = true;
            inputBlock = false;
            canEngage = true;
            Invulnerable = false;
            HP = stats.MaxHP;
        }

        public void Update(float deltaTime)
        {
            dashBuffer.Update(deltaTime);
            grounded.Update(deltaTime);
            dashCoolDownTimer -= deltaTime;
        }

        // Buffered helpers
        public bool BufferedAction(System.Func<bool> condition, System.Action action, ref float bufferTimer, float timeout, bool triggered)
        {
            if (triggered) bufferTimer = timeout;

            bool executed = false;
            if (bufferTimer > 0f)
            {
                bufferTimer -= Time.deltaTime;
                if (condition())
                {
                    action?.Invoke();
                    bufferTimer = 0f;
                    executed = true;
                }
            }
            return executed;
        }


    }
}
