using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Target & Offset Settings")]
    [Tooltip("The player object the camera should follow.")]
    [SerializeField] private Transform playerTarget;

    [SerializeField] private RoomScriptableStats _stats;
    [Tooltip("Half-width of horizontal deadzone.")]
    [SerializeField] private float xOffset;
    [SerializeField] private float yOffset;

    [SerializeField] private Transform cameraTransform;  // Optional, defaults to main camera

    private Vector3 DesiredPosition;
    private Vector3 shakeOffset;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        Instance = this;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        if (playerTarget == null)
        {
            Debug.LogError("CameraManager requires a Player Target assigned in the inspector.");
            enabled = false;
        }

        DesiredPosition = cameraTransform.position;

        //xOffset = _stats.RoomWidth / 2f;
        //yOffset = _stats.RoomHeight / 2f;
    }

    private void Update()
    {
        //UpdateDesiredPosition();
        UpdateSmoothDesiredPosition();
        //DesiredPosition = new Vector3(playerTarget.position.x, playerTarget.position.y, transform.position.z);
    }

    private void LateUpdate()
    {
        cameraTransform.position = DesiredPosition + shakeOffset;
    }

    private void UpdateDesiredPosition()
    {
        if (playerTarget == null) return;

        Vector3 pos = DesiredPosition; // start from previous
        float playerX = playerTarget.position.x;
        float playerY = playerTarget.position.y;

        float rightBound = pos.x + xOffset;
        float leftBound = pos.x - xOffset;
        float upperBound = pos.y + yOffset;
        float lowerBound = pos.y - yOffset;

        if (playerX > rightBound) pos.x += 2 * xOffset;
        else if (playerX < leftBound) pos.x -= 2 * xOffset;
        if (playerY > upperBound) pos.y += 2 * yOffset;
        else if (playerY < lowerBound) pos.y -= 2 * yOffset;

        DesiredPosition = pos;
    }


    private void UpdateSmoothDesiredPosition()
    {
        if (playerTarget == null) return;

        Vector3 camPos = DesiredPosition;
        Vector3 playerPos = playerTarget.position;

        // Horizontal deadzone
        if (playerPos.x > camPos.x + xOffset) camPos.x = playerPos.x - xOffset;
        else if (playerPos.x < camPos.x - xOffset) camPos.x = playerPos.x + xOffset;

        // Vertical deadzone
        //if (playerPos.y > camPos.y + yOffset) camPos.y = playerPos.y - yOffset;
        //else if (playerPos.y < camPos.y - yOffset) camPos.y = playerPos.y + yOffset;

        DesiredPosition = camPos;
    }

    // Public methods to start shakes
    public void Shake(float intensity, float duration, float shakeFrequency) => StartShake(Vector2.zero, intensity, duration, shakeFrequency);
    public void ShakeDirectional(Vector2 direction, float intensity, float duration, float shakeFrequency) => StartShake(direction.normalized, intensity, duration, shakeFrequency);

    private void StartShake(Vector2 direction, float intensity, float duration, float shakeFrequency)
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(DoShake(direction, intensity, duration, shakeFrequency));
    }

    private IEnumerator DoShake(Vector2 direction, float intensity, float duration, float shakeFrequency)
    {
        float timer = 0f;
        shakeOffset = Vector3.zero;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float decay = 1f - (timer / duration);

            Vector2 offset;
            if (direction == Vector2.zero)
            {
                // Isotropic shake
                offset = Random.insideUnitCircle * intensity * decay;
            }
            else
            {
                // Directional shake with slight jitter
                Vector2 rand = Random.insideUnitCircle * 0.3f;
                offset = (direction + rand).normalized * intensity * decay;
            }

            shakeOffset = (Vector3)offset;

            yield return new WaitForSecondsRealtime(1f / shakeFrequency);
        }

        shakeOffset = Vector3.zero;
        shakeRoutine = null;
    }
}
