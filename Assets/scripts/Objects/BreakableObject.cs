using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AttackableObject))]
public class BreakableObject : MonoBehaviour
{
    [SerializeField] private float maxHP = 3f;
    private float _currentHP;

    [Header("Hit Flash Settings")]
    [SerializeField] private Material hitFlashMaterial; // Optional override
    [SerializeField] private float flashDuration = 0.1f;

    private AttackableObject _attackableComponent;
    private Renderer _rend;
    private Material _originalMaterial;
    private Coroutine _flashRoutine;

    void Start()
    {
        _currentHP = maxHP;
        _attackableComponent = GetComponent<AttackableObject>();
        _rend = GetComponent<Renderer>();

        if (_rend != null)
        {
            _originalMaterial = _rend.material;
        }
    }

    void Update()
    {
        if (_currentHP <= 0) return;

        if (_attackableComponent.IsHitThisFrame)
        {
            TakeDamage(_attackableComponent.Damage);
            _attackableComponent.ClearHit();
        }
    }

    public void TakeDamage(float damageAmount)
    {
        _currentHP =  _currentHP - damageAmount;
        FlashHit();
    }

    public void Destroy()
    {
        if (_rend != null) _rend.enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }

    private void FlashHit()
    {
        if (_rend == null || _originalMaterial == null) return;
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        // Set flash material
        if (hitFlashMaterial != null)
            _rend.material = hitFlashMaterial;
        else
            _rend.material.color = Color.white; // fallback white flash

        yield return new WaitForSeconds(flashDuration);

        // Restore original
        _rend.material = _originalMaterial;
        if (_currentHP <= 0)
        {
            Destroy();
        }
    }
}
