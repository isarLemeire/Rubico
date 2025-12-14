using UnityEngine;
using System.Collections.Generic;

namespace PlayerController
{
    // NOTE: Assumes PlayerScriptableStats defines:
    //    public float wallLedgeCorrection; // Used for vertical scoot clearance (Wall) and lateral scoot clearance (Floor)
    //    public float ceilingLedgeCorrection; // Used for lateral scoot clearance (Ceiling)
    //    public float scootSpeed;
    //    public float WallAssistVerticalSpeedThreshold; (optional from previous code)
    //
    // The class focuses on incremental, velocity-driven scooting:
    // - Probe full correction distance to see if that direction can clear collision with the same collider.
    // - Compute minimum required scoot distance (binary search).
    // - Apply scoot = min(_stats.scootSpeed * deltaTime, requiredClearance).
    // - If scoot >= requiredClearance -> apply remaining axis movement this frame; otherwise cancel axis movement this frame and continue scooting upcoming frames.
    public class PlayerBoxCollisionHandler : MonoBehaviour
    {
        private const float _positionEpsilon = 0.001f;
        private const float _groundCheckDistance = 0.005f;
        private const float _skinWidth = 0.01f;

        [SerializeField] private PlayerScriptableStats _stats;
        [SerializeField] private bool _enableLedgeCorrection = true;

        [SerializeField] private LayerMask _collisionMask = ~0;
        [SerializeField] private LayerMask _triggerCollisionMask = ~0;

        private BoxCollider2D _box;

        public bool grounded { get; protected set; }
        public bool hitCeiling { get; protected set; }
        public bool hitWall { get; protected set; }

        public List<RaycastHit2D> Hits { get; protected set; } = new List<RaycastHit2D>();

        void Awake()
        {
            _box = GetComponent<BoxCollider2D>();
            if (Hits == null) Hits = new List<RaycastHit2D>();
        }

        /// <summary>
        /// Primary movement entrypoint. 'position' is expected to be the current transform.position
        /// passed by reference so the handler can update it directly.
        /// </summary>
        public void Move(ref Vector3 position, Vector3 velocity, float deltaTime)
        {
            grounded = hitCeiling = hitWall = false;
            Hits.Clear();

            Vector3 move = velocity * deltaTime;
            Vector3 originalMove = move;

            CheckAndResolveAxisCollision(ref position, ref move, Vector3.right, move.x, originalMove, deltaTime);
            CheckAndResolveAxisCollision(ref position, ref move, Vector3.up, move.y, originalMove, deltaTime);



            transform.position = position;
        }

        protected void CheckAndResolveAxisCollision(ref Vector3 position, ref Vector3 move, Vector3 axis, float axisMovement, Vector3 originalMove, float deltaTime)
        {
            if (Mathf.Abs(axisMovement) < _positionEpsilon)
            {
                if (axis == Vector3.up)
                {
                    PerformGroundedCheck(ref position, _groundCheckDistance);
                }
                return;
            }

            // Do minimal boxcasts and find the closest solid hit on this axis
            (RaycastHit2D closestSolidHit, LayerMask finalMask, Vector2 centerOffset, Vector2 boxSize, float angle) =
                PerformBoxCasts(position, axis, axisMovement);

            if (!closestSolidHit)
            {
                // No obstruction: move fully
                position += axis * axisMovement;
                return;
            }

            RaycastHit2D hit = closestSolidHit;
            float moveDistance = Mathf.Max(0f, hit.distance - _skinWidth); // distance we can move before collision
            Vector3 preCollisionPosition = position + (Vector3)(axis * Mathf.Sign(axisMovement)) * moveDistance;
            float distanceToResolve = Mathf.Abs(axisMovement) - moveDistance;


            // If scoot logic is enabled, attempt scoot
            if (_enableLedgeCorrection && distanceToResolve > 0.0001f)
            {
                bool resolved = false;
                if (axis == Vector3.up)
                {
                    if (Mathf.Abs(axisMovement) > 0.1)
                    {
                        // Upwards collision (ceiling) -> attempt horizontal scoots (right then left)
                        resolved = TryCeilingScoot(ref position, ref move, hit, preCollisionPosition, distanceToResolve, finalMask, centerOffset, boxSize, angle, deltaTime);
                    }
                    else // axisMovement < 0
                    {
                        // Downwards collision (floor/slope) -> attempt horizontal scoots (right then left)
                        resolved = TryFloorScoot(ref position, ref move, hit, preCollisionPosition, distanceToResolve, originalMove, finalMask, centerOffset, boxSize, angle, deltaTime);
                    }
                }
                else if (axis == Vector3.right)
                {
                    // Horizontal collision (wall) -> attempt vertical scoots (up then down)
                    resolved = TryWallScoot(ref position, ref move, hit, preCollisionPosition, distanceToResolve, originalMove, finalMask, centerOffset, boxSize, angle, deltaTime);
                }

                if (resolved)
                    return;
            }

            // Standard collision resolution: stop movement along this axis at collision point
            position = preCollisionPosition;
            if (axis == Vector3.right) move.x = 0;
            else if (axis == Vector3.up) move.y = 0;

            UpdateCollisionFlags(hit);
        }

        private void PerformGroundedCheck(ref Vector3 position, float checkDistance)
        {
            (RaycastHit2D closestSolidHit, LayerMask finalMask, Vector2 centerOffset, Vector2 boxSize, float angle) =
                PerformBoxCasts(position, Vector3.down, checkDistance);

            if (closestSolidHit)
            {
                RaycastHit2D hit = closestSolidHit;
                UpdateCollisionFlags(hit);
            }
        }

        /// <summary>
        /// Run boxcasts for the axis and return the closest solid hit plus commonly used geometry values.
        /// </summary>
        private (RaycastHit2D closestSolidHit, LayerMask finalMask, Vector2 centerOffset, Vector2 boxSize, float angle)
        PerformBoxCasts(Vector3 position, Vector3 axis, float axisMovement)
        {
            Vector2 boxSize = Vector2.Scale(_box.size, transform.localScale);
            Vector2 centerOffset = Vector2.Scale(_box.offset, transform.localScale);
            Vector2 origin = (Vector2)position + centerOffset;
            float angle = transform.eulerAngles.z;
            Vector2 direction = (Vector2)axis * Mathf.Sign(axisMovement);
            float distance = Mathf.Abs(axisMovement) + _skinWidth;

            int selfLayer = gameObject.layer;
            LayerMask finalMask = _collisionMask & ~(1 << selfLayer);
            LayerMask triggerMask = _triggerCollisionMask & ~(1 << selfLayer);

            // Note: Incomplete trigger gathering is intentionally omitted for brevity here.
            RaycastHit2D[] allHits = Physics2D.BoxCastAll(origin, boxSize, angle, direction, distance, finalMask);
            RaycastHit2D[] triggerHits = Physics2D.BoxCastAll(origin, boxSize, angle, direction, distance, triggerMask);

            RaycastHit2D closestSolidHit = default;
            float closestDistance = float.MaxValue;

            foreach (var h in triggerHits)
            {
                if (h.collider == null) continue;
                if (h.collider.gameObject == gameObject) continue;

                Hits.Add(h);
            }

            foreach (var h in allHits)
            {
                if (h.collider == null) continue;
                if (h.collider.gameObject == gameObject) continue;

                Hits.Add(h);

                if (!h.collider.isTrigger && h.distance < closestDistance)
                {
                    closestDistance = h.distance;
                    closestSolidHit = h;
                }
            }

            return (closestSolidHit, finalMask, centerOffset, boxSize, angle);
        }

        // --------------------------
        // Wall scoot (horizontal collision -> vertical scoot candidates)
        // --------------------------
        private bool TryWallScoot(ref Vector3 position, ref Vector3 move, RaycastHit2D hit,
                                     Vector3 preCollisionPosition, float distanceToResolve, Vector3 originalMove,
                                     LayerMask finalMask, Vector2 centerOffset, Vector2 boxSize, float angle, float deltaTime)
        {
            if (_stats == null || _stats.WallLedgeCorrection <= 0f) return false;

            // Optionally gate by vertical velocity threshold (keeps previous behavior)
            float verticalVelocity = originalMove.y / Mathf.Max(deltaTime, 1e-8f);
            // Check if the wall is mostly vertical (normal.y is small) AND if vertical speed is low enough
            if (Mathf.Abs(hit.normal.y) > 0.1f)
            {
                return false;
            }

            // Try both up and down
            Vector2[] candidates = new Vector2[] { Vector2.up, Vector2.down };

            foreach (var dir in candidates)
            {
                // 1) See if scooting in 'dir' by up to wallLedgeCorrection can clear the *same* collider for the remaining horizontal move.
                if (TryDirectionalScoot(preCollisionPosition, dir, _stats.WallLedgeCorrection, hit.collider,
                                             finalMask, centerOffset, boxSize, angle,
                                             Vector2.right * Mathf.Sign(originalMove.x), distanceToResolve,
                                             out float requiredClearance))
                {
                    // We can scoot in this dir; apply incremental scoot for this frame
                    float scootStep = Mathf.Min(_stats.ScootSpeed * deltaTime, requiredClearance);
                    if (dir == Vector2.up)
                        scootStep -= originalMove.y;

                    // Apply scoot
                    position = preCollisionPosition + (Vector3)(dir * scootStep);

                    // If scoot fully resolves the collision this frame, also apply the remaining horizontal move now
                    if (scootStep >= requiredClearance - _positionEpsilon)
                    {
                        position += (Vector3)(Vector2.right * Mathf.Sign(originalMove.x) * distanceToResolve);
                    }

                    // Block horizontal movement for this frame if collision still remains (scoot was partial)
                    move.x = 0;
                    Debug.Log($"[Wall Scoot] dir={dir} required={requiredClearance:F4} step={scootStep:F4} distToResolve={distanceToResolve:F4}");
                    return true;
                }
            }
            return false;
        }

        // --------------------------
        // Ceiling scoot (vertical collision -> horizontal scoot candidates)
        // --------------------------
        private bool TryCeilingScoot(ref Vector3 position, ref Vector3 move, RaycastHit2D hit,
                  Vector3 preCollisionPosition, float verticalDistanceToResolve,
                  LayerMask finalMask, Vector2 centerOffset, Vector2 boxSize, float angle, float deltaTime)
        {
            if (_stats == null || _stats.CeilingLedgeCorrection <= 0f) return false;


            if (hit.normal.y > 0.9f) return false;

            // Try both right and left
            Vector2[] candidates = new Vector2[] { Vector2.right, Vector2.left };

            foreach (var dir in candidates)
            {
                // For ceiling scoot, we need lateral clearance first (max ceilingLedgeCorrection),
                // then ensure vertical path is clear from the scooted position for verticalDistanceToResolve.
                if (TryDirectionalScoot(preCollisionPosition, dir, _stats.CeilingLedgeCorrection, hit.collider,
                    finalMask, centerOffset, boxSize, angle,
                    Vector2.up, verticalDistanceToResolve,
                    out float requiredClearance))
                {
                    // We can scoot in this dir; apply incremental scoot for this frame
                    float scootStep = Mathf.Min(_stats.ScootSpeed * deltaTime, requiredClearance);

                    // Apply scoot sideways
                    position = preCollisionPosition + (Vector3)(dir * scootStep);

                    // If scoot fully resolves the collision this frame, also apply the remaining upward move now
                    if (scootStep >= requiredClearance - _positionEpsilon)
                    {
                        position += Vector3.up * verticalDistanceToResolve;
                    }

                    // Block vertical movement for this frame if collision still remains (scoot was partial)
                    move.y = 0;
                    Debug.Log($"[Ceiling Scoot] dir={dir} required={requiredClearance:F4} step={scootStep:F4} verticalResolve={verticalDistanceToResolve:F4}");
                    return true;
                }
            }

            return false;
        }


        // --------------------------
        // Floor scoot (vertical collision downwards -> horizontal scoot candidates)
        // --------------------------

        // --------------------------
        // Floor scoot (vertical collision downwards -> horizontal scoot candidates)
        // --------------------------
        private bool TryFloorScoot(ref Vector3 position, ref Vector3 move, RaycastHit2D hit,
                                     Vector3 preCollisionPosition, float verticalDistanceToResolve, Vector3 originalMove,
                                     LayerMask finalMask, Vector2 centerOffset, Vector2 boxSize, float angle, float deltaTime)
        {
            // Using WallLedgeCorrection for the lateral scoot distance, similar to how Wall Scoot uses it for vertical
            if (_stats == null || _stats.FloorLedgeCorrection <= 0f) return false;

            // NEW CHECK: Only attempt floor scoot if the player is not trying to move horizontally this frame.
            if (Mathf.Abs(originalMove.x) > 0.1f) return false;

            // Only attempt scoot if the normal is somewhat sloped/angled (i.e., not a perfectly flat floor)
            if (hit.normal.y < 0.9f) return false;

            // Try both right and left (lateral candidates)
            Vector2[] candidates = new Vector2[] { Vector2.right, Vector2.left };

            foreach (var dir in candidates)
            {
                // For floor scoot, we need lateral clearance (max WallLedgeCorrection),
                // then ensure vertical path is clear from the scooted position for verticalDistanceToResolve.
                // NOTE: Using WallLedgeCorrection as defined in comments for lateral clearance.
                if (TryDirectionalScoot(preCollisionPosition, dir, _stats.FloorLedgeCorrection, hit.collider,
                                             finalMask, centerOffset, boxSize, angle,
                                             Vector2.down, verticalDistanceToResolve,
                                             out float requiredClearance))
                {
                    // We can scoot in this dir; apply incremental scoot for this frame
                    float scootStep = Mathf.Min(_stats.ScootSpeed * deltaTime, _stats.FloorLedgeCorrection - requiredClearance);

                    // Apply scoot sideways
                    position = preCollisionPosition + (Vector3)(-dir * scootStep);

                    // If scoot fully resolves the collision this frame, also apply the remaining downward move now
                    move.y = 0;

                    
                    Debug.Log($"[Floor Scoot] dir={dir} required={requiredClearance:F4} step={scootStep:F4} verticalResolve={verticalDistanceToResolve:F4}");
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Generic directional scoot tester that:
        ///  - Probes the environment by attempting to move 'maxTestDistance' in 'scootDir' from 'preCollisionPosition'.
        ///  - If that probe finds any clearance, computes the minimum scoot distance required to allow the remainingMovementDir
        ///     to pass (i.e., a BoxCast in remainingMovementDir with distance 'remainingMovementDistance' should not hit the originalCollider).
        ///  - Uses a small binary search to compute a tight 'requiredClearance'.
        ///  - Returns true and the required clearance when possible, otherwise false.
        ///
        /// Parameters:
        ///  - preCollisionPosition : the position at which the object contacts the blocker before scoot
        ///  - scootDir : unit vector direction to scoot (up/down/left/right)
        ///  - maxTestDistance : maximum distance to probe for scoot clearance (wall or ceiling correction)
        ///  - originalCollider : the collider we want to avoid colliding with after scoot
        ///  - finalMask, centerOffset, boxSize, angle : geometry used for BoxCasts
        ///  - remainingMovementDir : direction of the axis we still need to move through (unit vector; e.g., right)
        ///  - remainingMovementDistance : how far along remainingMovementDir we need to move to finish the attempted axis movement
        ///  - requiredClearance (out) : minimal scoot distance to clear the original collider for the remaining movement
        /// </summary>
        private bool TryDirectionalScoot(Vector3 preCollisionPosition,
                                         Vector2 scootDir,
                                         float maxTestDistance,
                                         Collider2D originalCollider,
                                         LayerMask finalMask,
                                         Vector2 centerOffset,
                                         Vector2 boxSize,
                                         float angle,
                                         Vector2 remainingMovementDir,
                                         float remainingMovementDistance,
                                         out float requiredClearance)
        {
            requiredClearance = 0f;

            if (maxTestDistance <= 0f) return false;

            // 1) Find the actual available scoot distance in 'scootDir'
            RaycastHit2D scootProbe = Physics2D.BoxCast((Vector2)preCollisionPosition + centerOffset, boxSize, angle, scootDir, maxTestDistance + _skinWidth, finalMask);

            float available = maxTestDistance;
            if (scootProbe)
            {
                available = Mathf.Max(0f, scootProbe.distance - _skinWidth);
            }

            if (available < _positionEpsilon) return false; // no available scoot

            // 2) Binary search to find minimal required clearance s in [0, available]
            float lo = 0f;
            float hi = available;
            bool anySuccess = false;

            // First check the hi endpoint: if the path is still BLOCKED even at max available scoot, return false.
            if (RemainingMovementHitsOriginal(preCollisionPosition + (Vector3)(scootDir * hi), remainingMovementDir, remainingMovementDistance, centerOffset, boxSize, angle, finalMask, originalCollider))
            {
                // Path blocked by original collider OR another collider behind it. Cannot scoot.
                return false;
            }
            else
            {
                // At max scoot distance (hi), the path is clear
                anySuccess = true;
            }

            // Binary search (8 iterations for precision)
            for (int i = 0; i < 8; ++i)
            {
                float mid = (lo + hi) * 0.5f;

                // midBlocked is TRUE if the remaining path is obstructed at position 'mid'
                bool midBlocked = RemainingMovementHitsOriginal(preCollisionPosition + (Vector3)(scootDir * mid), remainingMovementDir, remainingMovementDistance, centerOffset, boxSize, angle, finalMask, originalCollider);

                if (midBlocked)
                {
                    lo = mid; // Still blocked, need more scoot (minimum is higher)
                }
                else
                {
                    hi = mid; // Clear, try less scoot (maximum is lower)
                    anySuccess = true;
                }
            }

            if (!anySuccess) return false;

            requiredClearance = hi; // hi holds the minimal required scoot distance that clears the path

            // Clamp tiny values to zero
            if (requiredClearance < _positionEpsilon) requiredClearance = 0f;

            // ADD SAFETY MARGIN
            if (requiredClearance > 0f)
            {
                requiredClearance += _positionEpsilon * 2f;
                requiredClearance = Mathf.Min(requiredClearance, available);
            }

            return requiredClearance > 0f;
        }

        /// <summary>
        /// Returns true if a BoxCast from 'testPosition' along 'remainingDir' for distance (remainingDistance + skinWidth)
        /// hits the originalCollider as the first non-trigger solid. This function respects finalMask.
        /// </summary>
        private bool RemainingMovementHitsOriginal(Vector3 testPosition,
                                                 Vector2 remainingDir,
                                                 float remainingDistance,
                                                 Vector2 centerOffset,
                                                 Vector2 boxSize,
                                                 float angle,
                                                 LayerMask finalMask,
                                                 Collider2D originalCollider) // originalCollider is kept for context but unused in logic
        {
            Vector2 origin = (Vector2)testPosition + centerOffset;
            float distance = Mathf.Max(0f, remainingDistance) + _skinWidth;

            // Use BoxCast, which is more efficient than BoxCastAll if you only need the closest hit.
            RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, angle, remainingDir, distance, finalMask);

            // If 'hit' is true, it means a solid collider was found within the search distance.
            return hit.collider != null;
        }

        protected void UpdateCollisionFlags(RaycastHit2D hit)
        {

            // --- Normal Vector Analysis ---
            if (!hit.collider.CompareTag("Level"))
            {
                return;
            }

            Vector3 normal = hit.normal;
            if (normal.y > 0.5f)
            {
                grounded = true;
            }
            else if (normal.y < -0.5f)
            {
                hitCeiling = true;
            }
            else if (Mathf.Abs(normal.x) > 0.5f)
            {
                hitWall = true;
            }
        }
    }
}