using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AttackableObject))]
public class HitFlashObject : MonoBehaviour
{
    [Header("Hit Flash Settings")]
    [SerializeField] private JuiceStats JuiceStats;

    private SpriteRenderer _rend;
    private Material _originalMat;
    private Material _flashMat;
    private Coroutine _flashRoutine;

    void Start()
    {        
        _rend = GetComponent<SpriteRenderer>();
        if (_rend == null)
        {
            _rend = GetComponentInChildren<SpriteRenderer>(true);
        }

        if (_rend != null)
        {
            _originalMat = _rend.material;
            CreateFlashMaterial();
        }
    }

    private void CreateFlashMaterial()
    {
        // This shader ignores textures and just draws the color you provide
        Shader flatColorShader = Shader.Find("Hidden/Internal-Colored");
        
        if (flatColorShader != null)
        {
            _flashMat = new Material(flatColorShader);
            _flashMat.SetColor("_Color", Color.white);
            _flashMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            
            // Ensure transparency is handled
            _flashMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _flashMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _flashMat.SetInt("_ZWrite", 0);
        }
    }

    public void FlashHit()
    {
        if (_rend == null || _originalMat == null) return;
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        _rend.material = _flashMat;
        yield return new WaitForSeconds(JuiceStats.HitFlashDuration);
        _rend.material = _originalMat;
    }

    private void OnDestroy()
    {
        if (_flashMat != null)
        {
            Destroy(_flashMat);
        }
    }
}
