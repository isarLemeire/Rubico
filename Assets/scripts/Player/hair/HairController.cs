using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HairController : MonoBehaviour
{
    [Header("Simulation Settings")]
    public int segmentCount = 5;
    public float segmentLength = 0.1f;
    [Range(0f, 1f)]
    public float damping = 0.9f;

    [Header("Gravity Settings")]
    public float gravityScale = 0.3f;

    [Header("Elasticity")]
    [Range(0f, 1f)]
    public float stretchiness = 0f;
    // 0 = never stretch, 1 = fully stretchable up to +100% length

    [Header("References")]
    public Transform headTarget;

    [Header("Visuals")]
    public Color hairColor = new Color(0.9f, 0.3f, 0.3f);

    private Vector3[] positions;
    private Vector3[] prevPositions;
    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = segmentCount;
        line.useWorldSpace = true;

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = hairColor;
        line.endColor = hairColor;

        positions = new Vector3[segmentCount];
        prevPositions = new Vector3[segmentCount];

        Vector3 start = headTarget.position;
        for (int i = 0; i < segmentCount; i++)
        {
            positions[i] = start - Vector3.up * (segmentLength * i);
            prevPositions[i] = positions[i];
        }
    }

    void LateUpdate()
    {
        // keep root fixed
        positions[0] = headTarget.position;
        prevPositions[0] = headTarget.position;

        Simulate();
        ApplyConstraints();
        RenderHair();
    }

    void Simulate()
    {
        Vector3 g = Vector3.down * gravityScale * (Time.deltaTime * Time.deltaTime);

        for (int i = 1; i < segmentCount; i++)
        {
            Vector3 velocity = (positions[i] - prevPositions[i]) * damping;
            prevPositions[i] = positions[i];
            positions[i] += velocity + g;
        }
    }

    void ApplyConstraints()
    {
        float maxStretchMultiplier = 1f + stretchiness; // e.g. 0 => 1x, 1 => 2x length

        for (int iteration = 0; iteration < 4; iteration++)
        {
            // re-anchor root
            positions[0] = headTarget.position;

            for (int i = 1; i < segmentCount; i++)
            {
                Vector3 dir = positions[i] - positions[i - 1];
                float dist = dir.magnitude;
                if (dist == 0f) continue;

                // clamp: cannot exceed restLength * (1 + stretchiness)
                float maxDist = segmentLength * maxStretchMultiplier;
                if (dist > maxDist)
                {
                    dir = dir.normalized * maxDist;
                    positions[i] = positions[i - 1] + dir;
                    dist = maxDist;
                }

                // normal distance correction (keeps it around rest length)
                float diff = (dist - segmentLength) / dist;
                positions[i] -= dir * diff * 0.5f;
                positions[i - 1] += dir * diff * 0.5f;
            }

            positions[0] = headTarget.position; // lock again
        }
    }

    void RenderHair()
    {
        for (int i = 0; i < segmentCount; i++)
            line.SetPosition(i, positions[i]);
    }
}
