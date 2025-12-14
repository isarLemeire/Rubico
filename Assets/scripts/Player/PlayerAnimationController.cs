using UnityEngine;

namespace PlayerController
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform spriteRoot;    // The transform you flip (your sprite)
        [SerializeField] private Transform playerController;
        private PlayerController controller;


        private void Awake()
        {
            controller = playerController.GetComponent<PlayerController>();
        }

        public void FlipCharacter()
        {
            bool faceRight = controller.faceRight;    // invert sign
            spriteRoot.localScale = new Vector3(faceRight ? 1f : -1f, 1f, 1f); ;

        }


        [SerializeField] private Material rippleMaterial;
        [SerializeField] private float rippleDuration = 0.35f;
        [SerializeField] private float rippleStrength = 0.25f;
        [SerializeField] private float rippleSpeed = 3f;

        private float rippleTimer = 0f;
        private bool rippling = false;

        public void TriggerRipple()
        {
            // world => screen
            Vector3 screenPos = Camera.main.WorldToScreenPoint(spriteRoot.position);
            Vector2 uv = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);

            rippleMaterial.SetVector("_Center", uv);
            rippleMaterial.SetFloat("_Strength", rippleStrength);
            rippleMaterial.SetFloat("_TimeMultiplier", rippleSpeed);

            rippleTimer = rippleDuration;
            rippling = true;
        }

        private void Update()
        {
            if (!rippling) return;

            rippleTimer -= Time.deltaTime;

            rippleMaterial.SetFloat("_Progress", 1f - (rippleTimer / rippleDuration));

            if (rippleTimer <= 0f)
            {
                rippleMaterial.SetFloat("_Progress", 1f);
                rippling = false;
            }
        }
    }
}
