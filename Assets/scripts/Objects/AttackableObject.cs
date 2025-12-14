using UnityEngine;

// This component is placed on any object that can receive a hit.
// It acts as a universal interface for the AttackHitbox to communicate with.
public class AttackableObject : MonoBehaviour
{
    // A flag to indicate if this object was hit during the current physics step.
    public bool IsHitThisFrame { get; private set; } = false;
    public float Damage { get; private set; }

    /// <summary>
    /// Called by the AttackHitbox when a collision is detected.
    /// </summary>
    public void ReceiveHit(float damage)
    {
        IsHitThisFrame = true;
        this.Damage = damage;
    }

    /// <summary>
    /// Clears the hit flag. Called by the target's logic (e.g., BreakableObject) 
    /// after it consumes the damage, or by LateUpdate as a fallback.
    /// </summary>
    public void ClearHit()
    {
        IsHitThisFrame = false;
        Damage = 0;
    }
}
