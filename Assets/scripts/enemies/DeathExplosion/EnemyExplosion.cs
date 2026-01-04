using UnityEngine;
using System.Collections;

public class EnemyExplosion : MonoBehaviour
{
    private Coroutine destroyRoutine;

    public void Explode(Vector2 hitDir, float speed, float spread, float lifetime)
    {
        hitDir = hitDir.normalized;

        foreach (var rb in GetComponentsInChildren<Rigidbody2D>())
        {
            Vector2 dir = ApplySpread(hitDir, spread);
            rb.linearVelocity = dir * speed;

            var frag = rb.GetComponent<ExplosionFragment>();
            if (frag != null)
                frag.Init(lifetime);
        }

        // Ensure we only start this once
        if (destroyRoutine != null)
            StopCoroutine(destroyRoutine);

        destroyRoutine = StartCoroutine(DestroyAfter(lifetime));
    }

    private IEnumerator DestroyAfter(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    private Vector2 ApplySpread(Vector2 direction, float spreadDegrees)
    {
        float angle = Random.Range(-spreadDegrees, spreadDegrees) * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        );
    }
}
