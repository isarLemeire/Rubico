using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Player
{
    [RequireComponent(typeof(PlayerBoxCollisionHandler))]
    public class PlayerController : Controller
    {
        new public PlayerStateContext ctx;
        private PlayerInputHandler _inputHandler;

        // Serialize fields
        [SerializeField] private PlayerScriptableStats _stats;
        [SerializeField] private JuiceStats _juiceStats;
        [SerializeField] private RoomScriptableStats _roomStats;

        public SpriteRenderer Sprite { get; private set; }
        public LineRenderer HairRenderer { get; private set; }

        public GameObject engageHitBoxObject { get; private set; }
        public GameObject attackHitBoxObject { get; private set; }
        public AttackHitbox attackHitbox { get; private set; }
        public BoxCollider2D playerHitbox { get; private set; }

        [SerializeField] public Transform _HPSprite;
        [SerializeField] private GameObject shockWave;
        [SerializeField] private GameObject roomManagerObject;

        public Animator animator { get; private set; }
        public Animator UIanimator { get; private set; }

        public ShockWaveController shockWaveController { get; private set; }
        public RoomManager roomManager { get; private set; }
        public CheckpointManager checkpoint { get; private set; }

        // movement
        private StateMachine _movementMachine;
        private PlayerMovementStateFactory _movementStateFactory;
        private PlayerMovementStateType? _queuedMovementState = null;
        
        // attack
        private StateMachine _attackMachine;
        private PlayerAttackStateFactory _attackStateFactory;
        private PlayerAttackStateType? _queuedAttackState = null;

        // damage
        private StateMachine _damageMachine;
        private PlayerDamageStateFactory _damageStateFactory;
        private PlayerDamageStateType? _queuedDamageState = null;


        public bool HitTarget => attackHitbox.HitTarget;
        public bool HitNonTarget => attackHitbox.HitNonTarget;

        new private void Awake()
        {
            ctx = new PlayerStateContext(_stats, _juiceStats, GetComponent<PlayerBoxCollisionHandler>());
            _inputHandler = new PlayerInputHandler(ctx);

            _movementMachine = new StateMachine(ctx);
            _attackMachine = new StateMachine(ctx);
            _damageMachine = new StateMachine(ctx);

            _movementStateFactory = new PlayerMovementStateFactory(this, ctx, _inputHandler);
            _attackStateFactory = new PlayerAttackStateFactory(this, ctx, _inputHandler);
            _damageStateFactory = new PlayerDamageStateFactory(this, ctx, _inputHandler);


            checkpoint = new CheckpointManager(this, _roomStats, _stats, transform.position);

            animator = GetComponentInChildren<Animator>(true);
            Sprite = GetComponentInChildren<SpriteRenderer>(true);
            HairRenderer = GetComponentInChildren<LineRenderer>(true);
            UIanimator = _HPSprite.GetComponent<Animator>();

            CreateEngageHitbox();
            CreatAttackHitbox();
            playerHitbox = transform.GetComponent<BoxCollider2D>();

            shockWaveController = shockWave.GetComponent<ShockWaveController>();
            roomManager = roomManagerObject.GetComponent<RoomManager>();
        }

        private void Start()
        {
            // starter states
            _movementMachine.ChangeState(new IdleState(this, ctx, _inputHandler));
            _attackMachine.ChangeState(new NonAttackState(this, ctx, _inputHandler));
            _damageMachine.ChangeState(new NeutralState(this, ctx, _inputHandler));
            ctx.faceRight = _inputHandler.FaceRight;
        }

        private void Update()
        {
            _inputHandler.Update(Time.deltaTime); // Increment input timers before potentially resetting them
            ctx.Update(Time.deltaTime);
            _inputHandler.Read();
            _attackMachine.Update();
            _movementMachine.Update();
            _damageMachine.Update();
        }

        new private void FixedUpdate()
        {
            UpdateQueuedStates();

            _attackMachine.FixedUpdate();
            _movementMachine.FixedUpdate();
            ProcessMovement();
            _damageMachine.FixedUpdate();

            // Late updates mainly handle collision
            attackHitbox.CheckAttack(ctx.speed * Time.deltaTime);
            _attackMachine.LateFixedUpdate();
            _movementMachine.LateFixedUpdate();
            _damageMachine.LateFixedUpdate();
            UpdateFacing();
            checkpoint.UpdateCheckPoint(transform.position, playerHitbox, ctx.grounded.IsTrue);
        }

        public void QueueMovementState(PlayerMovementStateType newState)
        {
            if (_queuedMovementState == null || PlayerStatePriority.MovementPriority[newState] > PlayerStatePriority.MovementPriority[_queuedMovementState.Value])
            {
                _queuedMovementState = newState;
            }
        }

        public void QueueAttackState(PlayerAttackStateType newState)
        {
            if (_queuedAttackState == null || PlayerStatePriority.AttackPriority[newState] > PlayerStatePriority.AttackPriority[_queuedAttackState.Value])
            {
                _queuedAttackState = newState;
            }
        }

        public void QueueDamageState(PlayerDamageStateType newState)
        {
            if (_queuedDamageState == null || PlayerStatePriority.DamagePriority[newState] > PlayerStatePriority.DamagePriority[_queuedDamageState.Value])
            {
                _queuedDamageState = newState;
            }
        }

        private void UpdateQueuedStates()
        {
            if (_queuedAttackState.HasValue)
            {
                _attackMachine.ChangeState(_attackStateFactory.GetState(_queuedAttackState.Value));
                _queuedAttackState = null;
            }
            if (_queuedMovementState.HasValue)
            {
                _movementMachine.ChangeState(_movementStateFactory.GetState(_queuedMovementState.Value));
                _queuedMovementState = null;
            }
            if (_queuedDamageState.HasValue)
            {
                _damageMachine.ChangeState(_damageStateFactory.GetState(_queuedDamageState.Value));
                _queuedDamageState = null;
            }
        }



        protected override void ProcessMovement()
        {
            Vector3 position = transform.position;
            ctx.CollisionHandler.Move(ref position, ctx.speed, Time.deltaTime);
        }

        private void UpdateFacing()
        {
            if (ctx.faceRight != _inputHandler.FaceRight)
            {
                animator.SetTrigger("TurnAround");
                ctx.faceRight = _inputHandler.FaceRight;
            }
        }

        private void CreateEngageHitbox()
        {
            engageHitBoxObject = createHitbox("EngageHitbox");
            engageHitBoxObject.layer = LayerMask.NameToLayer("Triggers");
        }

        private void CreatAttackHitbox()
        {
            attackHitBoxObject = createHitbox("AttackHitbox");
            attackHitBoxObject.layer = LayerMask.NameToLayer("AttackHitbox");
            attackHitbox = attackHitBoxObject.AddComponent<AttackHitbox>();
            attackHitbox.Stats = _stats;
        }

        private GameObject createHitbox(String name)
        {
            GameObject newObject = new GameObject(name);
            newObject.transform.SetParent(transform, false);
            BoxCollider2D hitBox = newObject.AddComponent<BoxCollider2D>();
            hitBox.isTrigger = true;

            newObject.SetActive(true);
            
            return newObject;
        }
        
    }
}
