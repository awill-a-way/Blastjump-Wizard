using System.Collections;
using UnityEngine;

public class Baggable : MonoBehaviour
{
    public bool bagged;
    public bool canBeGrappled;
    [HideInInspector] public MeshRenderer mr;
    [HideInInspector] public Collider c;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public bool _originalRigidbodyUseGravity;
    [HideInInspector] public bool _originalRigidbodyIsKinematic;
    
    void Awake()
    {
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

        if (rb != null)
        {
            _originalRigidbodyUseGravity = rb.useGravity;
            _originalRigidbodyIsKinematic = rb.isKinematic;
        }
    }
    void Start()
    {
        bagged = false;
        canBeGrappled = true;
    }

    void Update()
    {
        if (bagged == true)
        {
            canBeGrappled = false;
        }
    }

    public void ResetCanBeGrappled(float delay)
    {
        if (delay <= 0f)
        {
            canBeGrappled = true;
        }
        else
        {
            StartCoroutine(CanBeGrappledCooldown(delay));
        }
    }

    IEnumerator CanBeGrappledCooldown(float AfterThisManySeconds)
    {
        yield return new WaitForSeconds(AfterThisManySeconds);
        canBeGrappled = true;
    }
}