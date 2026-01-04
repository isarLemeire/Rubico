using UnityEngine;
using UnityEngine.InputSystem;

namespace Player 
{
    // Single container for all data accessible to states.
    public class PlayerStateContext : StateContext
    {
        public PlayerScriptableStats Stats { get; private set; }
        public JuiceStats JuiceStats { get; private set; }

        public PlayerBoxCollisionHandler CollisionHandler;

        // persistent runtime state
        public BufferedBool grounded;
        public BufferedBool dashBuffer;
        public bool canDash;
        public bool canAttack;
        public bool canKnockBack;
        public float dashCoolDownTimer;
        public bool canEngage;

        public bool invulnerable;
        public bool movement_invulnerable;
        public bool DeathInputBlock;
        public bool attackInputBlock;

        public int HP;
        public int heal;

        public Vector2 attackAim;
        public Vector2 dashAim;
        public int attackCombo;

        public GameObject lastCheckpoint;
        public RaycastHit2D lastHazardHit;

        public PlayerStateContext(PlayerScriptableStats stats, JuiceStats juiceStats, PlayerBoxCollisionHandler collisionHandler)
        {
            this.JuiceStats = juiceStats;
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
            DeathInputBlock = false;
            attackInputBlock = false;
            canEngage = true;
            invulnerable = false;
            movement_invulnerable = false;
            HP = stats.MaxHP;
            faceRight = true;
        }

        new public void Update(float deltaTime)
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
