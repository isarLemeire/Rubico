using UnityEngine;

public class ExplosionFragment : MonoBehaviour
{
    private float lifetime;
    private float timer;
    private Vector3 initialScale;

    public void Init(float lifetime)
    {
        this.lifetime = lifetime;
        timer = lifetime;
        initialScale = transform.localScale;
    }

    private void FixedUpdate()
    {
        timer -= Time.deltaTime;
        float t = Mathf.SmoothStep(0f, 1f, timer / lifetime);
        transform.localScale = initialScale * t;

        if (timer <= 0f)
            gameObject.SetActive(false);
    }
}