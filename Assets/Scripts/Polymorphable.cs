using System.Collections;
using UnityEngine;

public class Polymorphable : MonoBehaviour
{
    private PlayerInputActions _inputActions;
    private HealthSystem health;
    public bool _canPolymorph = true;
    public bool _isPolymorphed = false;
    private Coroutine _revertRoutine;
    private GameObject _spawnedPolymorph;
    [SerializeField] private bool useOriginalMaterials = true;

    //
    private MeshFilter mf;
    private MeshRenderer mr;
    private Collider c;
    private Rigidbody rb;
    
    // Cached original state
    private Mesh _originalMesh;
    private Material[] _originalMaterials;
    // private bool _cachedDisableRigidbody;
    private bool _originalRigidbodyUseGravity;
    private bool _originalRigidbodyIsKinematic;
    private Vector3 _originalScale;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        mf = GetComponent<MeshFilter>();
        if (mf == null)
        {
            mf = GetComponentInChildren<MeshFilter>();
        }

        mr = GetComponent<MeshRenderer>();
        if (mr == null)
        {
            mr = GetComponentInChildren<MeshRenderer>();
        }

        c = GetComponent<Collider>();
        if (c == null)
        {
            c = GetComponentInChildren<Collider>();
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = GetComponentInChildren<Rigidbody>();
        }
        _originalRigidbodyUseGravity = rb.useGravity;
        _originalRigidbodyIsKinematic = rb.isKinematic;

        if (mf) _originalMesh = mf.sharedMesh;
        if (mr) _originalMaterials = mr.sharedMaterials;
        //if (rb) _originalRigidbody = rb;

        _originalScale = transform.localScale;
    }

    void Start()
    {
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        health = GetComponent<HealthSystem>();
    }

    public void TriggerPolymorphation(GameObject prefab, float duration, bool spawnUpright)
    {
        if (_canPolymorph == true)
        {
            if (!_isPolymorphed)
            {
                ApplyPolymorphation(prefab, spawnUpright);
            }
            else if (_revertRoutine != null)
            {
                StopCoroutine(_revertRoutine);
            }

            _revertRoutine = StartCoroutine(RevertAfter(duration));
        }
    }
    
    private void ApplyPolymorphation(GameObject prefab, bool spawnUpright)
    {
        _isPolymorphed = true;
        _canPolymorph = false;
        if (health != null)
        {
            health.IsInvulnerable = false;
        }
        
        if (mr) mr.enabled = false;
        if (c) c.enabled = false;
        if (rb)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        _spawnedPolymorph = Instantiate(prefab, transform.position, Quaternion.identity, transform);
        if (useOriginalMaterials == true && _originalMaterials != null)
        {
            var pmr = _spawnedPolymorph.GetComponent<MeshRenderer>();
            pmr.sharedMaterials = _originalMaterials;
        }
    }

    IEnumerator RevertAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        RevertPolymorphation();
    }

    private void RevertPolymorphation()
    {
        _isPolymorphed = false;
        if (health != null)
        {
            health.IsInvulnerable = false;
        }

        if (mr) mr.enabled = true;
        if (c) c.enabled = true;
        if (rb)
        {
            rb.useGravity = _originalRigidbodyUseGravity;
            rb.isKinematic = _originalRigidbodyIsKinematic;
        }

        Destroy(_spawnedPolymorph);
        StartCoroutine(ResetCanPolymorph(5f));
    }

    IEnumerator ResetCanPolymorph(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        _canPolymorph = true;
    }

    void Update()
    {
        if (_isPolymorphed)
        {
            var input = _inputActions.Gameplay;
            if (input.Fire2.WasPressedThisFrame())
            {
                StopCoroutine(_revertRoutine);
                RevertPolymorphation();

                if (health != null)
                {
                    health.TakeDamage(5f);
                }
            }
        }
    }

    void OnDestroy()
    {
        _inputActions.Dispose();
    }
}
