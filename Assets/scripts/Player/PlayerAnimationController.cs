using UnityEngine;

namespace Player
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
            bool faceRight = controller.ctx.faceRight;    // invert sign
            spriteRoot.localScale = new Vector3(faceRight ? 1f : -1f, 1f, 1f); ;
        }
    }
}
