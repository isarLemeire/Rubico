using UnityEngine;
using System;
using System.Collections;


[RequireComponent(typeof(HitFlashObject))]
public class CrabController : EnemyController<CrabStateType>
{

    [SerializeField] public CrabStats stats;
    public Transform trackedPlayer { get; private set; }
    public Collider2D hitboxCollider { get; private set; }
    private bool _angry;

    public Player.PlayerController playerController { get; private set; }

    private AttackableObject attackable;

    [SerializeField] private GameObject DamageBoxObject;
    [SerializeField] public GameObject HitBoxObject;
    private HitFlashObject hitFlash;

    public Vector2 startingLocation { get; private set; }

    private ContactFilter2D dangerFilter;
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[8];

    protected override void Awake()
    {
        base.Awake();
        statePriority = CrabStatePriority.priority;
        _angry = false;

        attackable = DamageBoxObject.GetComponent<AttackableObject>();
        hitFlash  = GetComponent<HitFlashObject>();
        hitboxCollider = GetComponentInChildren<Collider2D>();

        enemyCtx.health = stats.health;
    }

    public void Start()
    {
        startingLocation = transform.position;
        trackedPlayer = FindFirstObjectByType<Player.PlayerController>()?.transform;
        playerController = trackedPlayer?.GetComponent<Player.PlayerController>();

        stateFactory = new CrabStateFactory(this);
        stateMachine.ChangeState(stateFactory.GetState(CrabStateType.Idle));
        
        Debug.Log("Starting location: " + startingLocation);

        dangerFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = stats.dangerMask,
            useTriggers = true
        };
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();


        CheckHit();
        if(!_angry)
        {
            CheckPlayerDetection();
        }
        if(_angry)
        {
            checkFacing();
        }

        if(IsInDanger())
        {
            Die();
        }
    }

    private bool IsInDanger()
    {
        var collider = Rigidbody.GetComponent<Collider2D>();

        int count = Rigidbody.Cast(
            Vector2.zero,
            dangerFilter,
            _hits,
            0f
        );
        if (count > 0)
        {
            enemyCtx.ReceiveHitAim = _hits[0].normal;
            return true;
        }
        return false;
    }

    private void CheckPlayerDetection()
    {
        Vector2 origin = ctx.position;

            Vector2[] directions = { Vector2.left, Vector2.right };
            foreach (var dir in directions)
            {
                RaycastHit2D hit = Physics2D.Raycast(origin, dir, stats.detectRange, stats.playerMask | stats.groundMask);
                if (hit.collider != null && hit.collider.GetComponent<Player.PlayerController>() != null )
                {
                    QueueState(CrabStateType.Startled);
                    _angry = true;
                    break;
                }
            }
    }

    private void checkFacing(){
        if (trackedPlayer == null)
            return;
        Vector2 toPlayer = trackedPlayer.position - transform.position;
        if(ctx.faceRight != (toPlayer.x > 0f)){
            ctx.faceRight = (toPlayer.x > 0f);
        }
    }

    private void CheckHit()
    {
        if (!attackable.IsHitThisFrame)
            return;

        float damage = attackable.Damage;

        // Forward damage
        hitFlash.FlashHit();

        // State transition
        QueueState(CrabStateType.Hurt);


        enemyCtx.ReceiveHitAim = attackable.AttackDirection;
        attackable.ClearHit();
        _angry = true;

        enemyCtx.health -= (int)damage;

        if (enemyCtx.health <= 0)
        {
            StartCoroutine(DieDelayed(0.05f));
        }
    }

    public void Die()
    {
        Instantiate(stats.explosionPrefab, transform.position, Quaternion.identity)
        .GetComponent<EnemyExplosion>()
        .Explode(enemyCtx.ReceiveHitAim, stats.explosionSpeed, stats.explosionSpread, stats.explosionLifetime);

        gameObject.SetActive(false);
    }

    private IEnumerator DieDelayed(float delay)
    {
        
        yield return new WaitForSeconds(delay);
        Instantiate(stats.explosionPrefab, transform.position, Quaternion.identity)
        .GetComponent<EnemyExplosion>()
        .Explode(enemyCtx.speed, stats.explosionSpeed, stats.explosionSpread, stats.explosionLifetime);


        gameObject.SetActive(false);
    }

    public void ResetAnger()
    {
        _angry = false;
    }
}


