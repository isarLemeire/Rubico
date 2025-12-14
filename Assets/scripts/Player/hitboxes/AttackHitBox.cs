using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class AttackHitbox : MonoBehaviour
{
    [Header("Layer Masks")]
    [SerializeField] public LayerMask TargetMask;      // Things that should take damage
    [SerializeField] private LayerMask _nonTargetMask;   // Things that block or react but take no damage

    public HashSet<GameObject> HitTargets { get; private set; }
    private Collider2D _selfCollider;

    // Controlled externally by PlayerController
    private bool _isDetectionActive = false;

    // Public flags to check from PlayerController AFTER hit check
    public bool HitTarget { get; private set; }
    public bool HitNonTarget { get; private set; }

    private float _damage;

    private void Awake()
    {
        _selfCollider = GetComponent<Collider2D>();
        HitTargets = new HashSet<GameObject>();

        if (!_selfCollider.isTrigger)
        {
            Debug.LogWarning("AttackHitbox collider should be set to 'Is Trigger'. Enabling it now.");
            _selfCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// Returns the projected AABB size for a box of given size rotated by angle.
    /// Use this when passing size to Physics2D.OverlapBoxAll to compensate for angle.
    /// </summary>
    public static Vector2 GetOrientedSize(Vector2 originalSize, float angleDeg)
    {
        float angleRad = angleDeg * Mathf.Deg2Rad;

        float cos = Mathf.Abs(Mathf.Cos(angleRad));
        float sin = Mathf.Abs(Mathf.Sin(angleRad));

        // Projected (axis-aligned) bounding box after rotation
        float width = originalSize.x * cos + originalSize.y * sin;
        float height = originalSize.x * sin + originalSize.y * cos;

        return new Vector2(width, height);
    }

    public void CheckAttack()
    {
        if (!_isDetectionActive)
            return;

        // Get the collider of this AttackHitbox
        // Assuming _selfCollider is the BoxCollider2D on the AttackHitbox object
        var col = _selfCollider.GetComponent<BoxCollider2D>();
        if (col == null) return;

        // Build contact filter (similar to checkStop)
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        // Assuming TargetMask is a public property of this script
        filter.layerMask = TargetMask | _nonTargetMask;
        filter.useTriggers = true;

        // Use the maximum possible array size you might need (e.g., 10)
        Collider2D[] hitColliders = new Collider2D[10];

        // --- Perform the Overlap check using the actual collider ---
        // This respects the collider's actual rotation, size, and offset.
        int hitCount = col.Overlap(filter, hitColliders);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D other = hitColliders[i];

            if (other.gameObject == gameObject || other.gameObject == _selfCollider.transform.parent.gameObject)
                continue;

            // --- TARGET LAYER HIT ---
            // Using the same IsInLayerMask helper
            if (IsInLayerMask(other.gameObject.layer, TargetMask))
            {
                if (!HitTargets.Contains(other.gameObject))
                {
                    AttackableObject targetAttackable = other.GetComponent<AttackableObject>();
                    if (targetAttackable != null)
                    {
                        targetAttackable.ReceiveHit(_damage);
                        HitTargets.Add(other.gameObject);
                    }
                    HitTarget = true;
                }
                continue;
            }

            // --- NON-TARGET HIT ---
            if (IsInLayerMask(other.gameObject.layer, _nonTargetMask))
            {
                HitNonTarget = true;
            }
        }
    }

    public void StartAttackDetection(float damage)
    {
        this._damage = damage;
        HitTargets.Clear();
        HitTarget = false;
        HitNonTarget = false;
        _isDetectionActive = true;
    }

    public void StopAttackDetection()
    {
        _isDetectionActive = false;
        HitTargets.Clear();
        HitTarget = false;
        HitNonTarget = false;
    }

    /// <summary>
    /// Helper: Checks if a layer is inside a given LayerMask.
    /// </summary>
    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}

