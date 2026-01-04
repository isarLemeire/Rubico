using System.Collections;
using UnityEngine;

namespace Player
{
    public class ShockWaveController : MonoBehaviour
    {
        [Header("Effect Settings")]
        [SerializeField] private float _shockWaveTime = 0.75f;
        [Header("Visual References")]
        // The visual object whose 'left' edge aligns with the position
        [SerializeField] private Transform _leftVisual;
        // The visual object whose 'right' edge aligns with the position
        [SerializeField] private Transform _rightVisual;

        // Store the materials for independent animation
        private Material _leftMaterial;
        private Material _rightMaterial;
        private Coroutine _leftShockCoroutine;
        private Coroutine _rightShockCoroutine;

        // Shader Property ID (Used by both materials)
        private static int _waveDistanceFromCenter = Shader.PropertyToID("_WaveDistance");

        void Awake()
        {
            // Get the unique material instance from the SpriteRenderer of each visual object
            // Assumes _leftVisual and _rightVisual have a SpriteRenderer component.
            _leftMaterial = _leftVisual.GetComponent<SpriteRenderer>().material;
            _rightMaterial = _rightVisual.GetComponent<SpriteRenderer>().material;
        }

        /// <summary>
        /// Triggers the dual shockwave effect, positioning one at the 'left' and one at the 'right' of the given position.
        /// </summary>
        /// <param name="position">The world position to anchor the effect.</param>
        /// <param name="direction">The Vector2 direction the wave is facing.</param>
        public void callShockWave(Vector3 position, Vector2 direction)
        {
            // --- 1. Calculate Rotation ---
            // +90f assumes the visual component's start/end aligns along its local Y-axis.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            transform.rotation = rotation;
            transform.position = position;

            // --- 4. Start Animations ---
            _leftShockCoroutine = StartCoroutine(ShockWaveAction(_leftMaterial));
            _rightShockCoroutine = StartCoroutine(ShockWaveAction(_rightMaterial));
        }

        private IEnumerator ShockWaveAction(Material material)
        {

            material.SetFloat(_waveDistanceFromCenter, 0);


            float LerpedAmount = 0f;

            float elapsedTime = 0f;


            while (elapsedTime < _shockWaveTime)

            {

                elapsedTime += Time.deltaTime;

                LerpedAmount = Mathf.Lerp(0, 1, elapsedTime / _shockWaveTime);

                material.SetFloat(_waveDistanceFromCenter, LerpedAmount);

                yield return null;

            }
        }
    }
}

