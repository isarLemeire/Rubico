using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{
    private IEnemyController controller;
    [SerializeField] private bool flipX;

    private Transform targetTransform;

    private void Awake()
    {
        controller = GetComponentInParent<IEnemyController>();
        if (controller is MonoBehaviour mb)
            targetTransform = mb.transform;
        else
            targetTransform = transform; // fallback
    }

    public void FlipCharacter()
    {
        bool faceRight = controller.Ctx.faceRight;
        controller.Ctx.moveRight = controller.Ctx.faceRight;
        targetTransform.localScale = new Vector3((faceRight ? 1f : -1f) * (flipX ? -1f : 1f), 1f, 1f);
    }

    public void SetAnimationBlockToTrue()
    {
        controller.Ctx.AnimationBlock = true;
    }

    public void SetAnimationBlockToFalse()
    {
        controller.Ctx.AnimationBlock = false;
    }
}