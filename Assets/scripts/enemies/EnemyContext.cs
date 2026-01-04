using UnityEngine;
public class EnemyContext : StateContext
{
    public Vector2 ReceiveHitAim;

    public bool AnimationBlock;
    public int health;
    public bool moveRight;

    public bool IsGrounded(Rigidbody2D rb, ContactFilter2D groundFilter)
    {
        ContactPoint2D[] contacts = new ContactPoint2D[8];
        int count = rb.GetContacts(groundFilter, contacts);
        for (int i = 0; i < count; i++)
        {
            if (contacts[i].normal.y > 0.5f)
                return true;
        }
        return false;
    }

    public bool CanMove(Vector2 origin, float dir, float forwardGroundCheckDistanceDistance, float downwardGroundCheckDistance, float wallCheckDistance, LayerMask groundMask)
    {
        // 1) Check for ground ahead
        Vector2 groundProbe = origin + Vector2.right * dir * forwardGroundCheckDistanceDistance;
        bool hasGround = Physics2D.Raycast(
            groundProbe,
            Vector2.down,
            downwardGroundCheckDistance,
            groundMask
        );

        // 2) Check for wall directly in front
        Vector2 wallProbe = origin;
        bool hasWall = Physics2D.Raycast(
            wallProbe,
            Vector2.right * dir,
            wallCheckDistance,
            groundMask
        );

        return hasGround && !hasWall;
    }

    public bool CanSafeMove(
        Vector2 origin,
        float dir,
        float forwardGroundCheckDistance,
        float downwardGroundCheckDistance,
        float wallCheckDistance,
        LayerMask groundMask,
        LayerMask dangerMask
    )
    {
        // ---- 1) Ground ahead probe (downward) ----
        Vector2 groundProbeOrigin = origin + Vector2.right * dir * forwardGroundCheckDistance;

        RaycastHit2D groundHit = Physics2D.Raycast(
            groundProbeOrigin,
            Vector2.down,
            downwardGroundCheckDistance,
            groundMask | dangerMask
        );

        // If nothing hit → no ground
        if (!groundHit)
            return false;

        // If danger is hit before ground → blocked
        if (((1 << groundHit.collider.gameObject.layer) & dangerMask) != 0)
            return false;

        // ---- 2) Forward wall probe ----
        RaycastHit2D wallHit = Physics2D.Raycast(
            origin,
            Vector2.right * dir,
            wallCheckDistance,
            groundMask | dangerMask
        );

        if (wallHit)
        {
            // If danger is closer than wall → blocked
            if (((1 << wallHit.collider.gameObject.layer) & dangerMask) != 0)
                return false;

            // Wall blocks movement
            return false;
        }

        return true;
    }

}