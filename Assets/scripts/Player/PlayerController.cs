using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerController
{
    [RequireComponent(typeof(PlayerBoxCollisionHandler))]
    public class PlayerController : MonoBehaviour
    {
        public PlayerStateContext ctx;
        private PlayerInputHandler _inputHandler;

        // Serialize fields
        [SerializeField] private PlayerScriptableStats _stats;
        [SerializeField] private RoomScriptableStats _roomStats;

        [SerializeField] public Transform _sprite;
        [SerializeField] public Transform _hair;
        [SerializeField] public Transform _HPSprite;
        [SerializeField] public GameObject attackHitBoxObject;
        [SerializeField] public GameObject engageHitBoxObject;
        [SerializeField] private GameObject shockWave;
        [SerializeField] private GameObject roomManagerObject;

        public AttackHitbox attackHitbox { get; private set; }
        public Animator animator { get; private set; }
        public Animator UIanimator { get; private set; }


        public ShockWaveController shockWaveController { get; private set; }
        public RoomManager roomManager { get; private set; }
        public CheckpointManager checkpoint { get; private set; }


        // movement
        private PlayerStateMachine _movementMachine;
        private IPlayerMovementStateFactory _movementStateFactory;
        private PlayerMovementStateType? _queuedMovementState = null;
        

        // attack
        private PlayerStateMachine _attackMachine;
        private IPlayerAttackStateFactory _attackStateFactory;
        private PlayerAttackStateType? _queuedAttackState = null;

        // damage
        private PlayerStateMachine _damageMachine;
        private IPlayerDamageStateFactory _damageStateFactory;
        private PlayerDamageStateType? _queuedDamageState = null;


        public bool HitTarget => attackHitbox.HitTarget;
        public bool HitNonTarget => attackHitbox.HitNonTarget;
        public bool faceRight;

        private void Awake()
        {
            ctx = new PlayerStateContext(_stats, GetComponent<PlayerBoxCollisionHandler>());
            _inputHandler = new PlayerInputHandler(ctx);

            _movementMachine = new PlayerStateMachine(ctx);
            _attackMachine = new PlayerStateMachine(ctx);
            _damageMachine = new PlayerStateMachine(ctx);

            _movementStateFactory = new PlayerMovementStateFactory(this, ctx, _inputHandler);
            _attackStateFactory = new PlayerAttackStateFactory(this, ctx, _inputHandler);
            _damageStateFactory = new PlayerDamageStateFactory(this, ctx, _inputHandler);


            checkpoint = new CheckpointManager(this, _roomStats, transform.position);

            attackHitbox = attackHitBoxObject.GetComponent<AttackHitbox>();
            animator = _sprite.GetComponent<Animator>();
            UIanimator = _HPSprite.GetComponent<Animator>();

            shockWaveController = shockWave.GetComponent<ShockWaveController>();
            roomManager = roomManagerObject.GetComponent<RoomManager>();
        }

        private void Start()
        {
            // starter states
            _movementMachine.ChangeState(new IdleState(this, ctx, _inputHandler));
            _attackMachine.ChangeState(new NonAttackState(this, ctx, _inputHandler));
            _damageMachine.ChangeState(new NeutralState(this, ctx, _inputHandler));
            faceRight = _inputHandler.FaceRight;
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

        private void FixedUpdate()
        {
            UpdateQueuedStates();

            _attackMachine.FixedUpdate();
            attackHitbox.CheckAttack();
            _movementMachine.FixedUpdate();
            ProcessMovementAndEnvironment();
            _damageMachine.FixedUpdate();

            // Late updates mainly handle collision
            _attackMachine.LateFixedUpdate();
            _movementMachine.LateFixedUpdate();
            _damageMachine.LateFixedUpdate();
            UpdateFacing();
            checkpoint.UpdateCheckPoint(transform.position, ctx.grounded.IsTrue);
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



        private void ProcessMovementAndEnvironment()
        {
            Vector3 position = transform.position;
            ctx.CollisionHandler.Move(ref position, ctx.speed, Time.deltaTime);
        }

        private void UpdateFacing()
        {
            if (faceRight != _inputHandler.FaceRight)
            {
                animator.SetTrigger("TurnAround");
                faceRight = _inputHandler.FaceRight;
            }
        }
    }
}
