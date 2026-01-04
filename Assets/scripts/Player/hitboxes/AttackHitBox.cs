using UnityEngine;
using System.Collections.Generic;

namespace Player
{
    [RequireComponent(typeof(Collider2D))]
    public class AttackHitbox : MonoBehaviour
    {
        public PlayerScriptableStats Stats;

        public HashSet<GameObject> HitTargets { get; private set; }

        private Collider2D _selfCollider;

        private Vector2 attackDir;
        private float _damage;

        // Detection control
        private bool _isDetectionActive = false;
        private bool _isBuffering = false;

        // Buffered collisions
        private readonly HashSet<Collider2D> _bufferedColliders = new();

        // Public results
        public bool HitTarget { get; private set; }
        public bool HitNonTarget { get; private set; }

        private void Awake()
        {
            _selfCollider = GetComponent<Collider2D>();
            HitTargets = new HashSet<GameObject>();
        }

        /* =========================
         * BUFFER CONTROL
         * ========================= */

        public void StartBuffer(Vector2 attackDirection)
        {
            _bufferedColliders.Clear();
            attackDir = attackDirection;
            _isBuffering = true;
            _isDetectionActive = false;
        }

        public void StopBuffer()
        {
            _isBuffering = false;
            _bufferedColliders.Clear();
        }

        /* =========================
         * ATTACK FLOW
         * ========================= */

        public void StartAttackDetection(float damage, Vector2 attackDirection)
        {
            _damage = damage;
            attackDir = attackDirection;

            HitTargets.Clear();
            HitTarget = false;
            HitNonTarget = false;

            // Flush buffered collisions FIRST
            foreach (var col in _bufferedColliders)
            {
                if (col != null)
                    HandleCollision(col);
            }

            _bufferedColliders.Clear();
            _isBuffering = false;
            _isDetectionActive = true;
        }

        public void StopAttackDetection()
        {
            _isDetectionActive = false;
            HitTargets.Clear();
            HitTarget = false;
            HitNonTarget = false;
        }

        /* =========================
         * COLLISION SAMPLING
         * ========================= */

        public void CheckAttack(Vector2 attackDelta)
        {
            if (!_isDetectionActive && !_isBuffering)
                return;

            var col = _selfCollider as BoxCollider2D;
            if (col == null) return;

            ContactFilter2D filter = new()
            {
                useLayerMask = true,
                layerMask = Stats.AttackTargetMask | Stats.AttackNonTargetMask,
                useTriggers = true
            };

            Collider2D[] overlap = new Collider2D[10];
            int overlapCount = col.Overlap(filter, overlap);

            for (int i = 0; i < overlapCount; i++)
                RegisterCollision(overlap[i]);

            /*
            float dist = Mathf.Max(attackDelta.magnitude, 0.001f);
            Vector2 dir = attackDelta.normalized;

            RaycastHit2D[] hits = new RaycastHit2D[10];
            int hitCount = col.Cast(dir, filter, hits, dist);

            for (int i = 0; i < hitCount; i++)
                RegisterCollision(hits[i].collider);
                */
        }

        private void RegisterCollision(Collider2D other)
        {
            if (other == null)
                return;

            if (_isBuffering && !_isDetectionActive)
            {
                _bufferedColliders.Add(other);
                return;
            }

            HandleCollision(other);
        }

        /* =========================
         * HIT RESOLUTION
         * ========================= */

        private void HandleCollision(Collider2D other)
        {
            if (other.gameObject == gameObject ||
                other.transform == _selfCollider.transform.parent)
                return;

            if (IsInLayerMask(other.gameObject.layer, Stats.AttackTargetMask))
            {
                if (!HitTargets.Contains(other.gameObject))
                {
                    var attackable = other.GetComponent<AttackableObject>();
                    if (attackable != null)
                    {
                        attackable.ReceiveHit(_damage, attackDir);
                        HitTargets.Add(other.gameObject);
                    }
                    HitTarget = true;
                }
                return;
            }

            if (IsInLayerMask(other.gameObject.layer, Stats.AttackNonTargetMask))
            {
                HitNonTarget = true;
            }
        }

        private bool IsInLayerMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }
    }
}
