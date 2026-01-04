using UnityEngine;

[CreateAssetMenu]
public class CrabStats : ScriptableObject
{

    public int health;
    [Header("MOVEMENT")]
    public float acceleration;
    public float speed;
    public float gravity;

    [Header("AWARENESS")]
    public LayerMask groundMask;
    public LayerMask playerMask;
    public LayerMask dangerMask;
    public float detectRange;
    public float detectMinimumRange;
    public float detectAngle;
    public float forgetDuration;

    public float forwardGroundCheckDistance;
    public float downwardGroundCheckDistance;
    
    public float wallCheckDistance;

    [Header("ATTACK AWARENESS")]
    public float downwardGroundCheckDistanceAttack;
    public float holdDetectionRange;

    [Header("PACING")]
    public float paceMinPaceDistance;
    public float paceMaxPaceDistance;
    public float maxPaceDistance;

    public float PaceMinDuration 
    {
        get { return paceMinPaceDistance / speed; }
    }

    public float PaceMaxDuration
    {
        get { return paceMaxPaceDistance / speed; }
    }

    [Header("TAUNT")]
    public float tauntOdds;
    public float tauntTime;

    [Header("IDLE")]
    public float MinIdleDuration;
    public float MaxIdleDuration;

    [Header("STARTLED")]
    public float StartledTime;
    

    [Header("HURT")]
    public float initialVelocity;
    public float hurtDuration;
    public float bounceReduction;
    public float hitAngle;
    public int maxBounces;
    public float recoverTime;

    [Header("DEATH EXPLOSION")]
    public GameObject explosionPrefab;
    public float explosionSpeed;
    public float explosionSpread;
    public float explosionLifetime;

    [Header("KNOCKBACK")]
    public float knockbackDuration;
    public float KnockbackDistance;

    public float KnockbackSpeed
    {
        get { return 2 * KnockbackDistance / knockbackDuration; }
    }
    public float KnockbackDeceleration
    {
        get { return KnockbackSpeed / knockbackDuration; }
    }   
}
