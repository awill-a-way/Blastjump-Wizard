using System.Collections.Generic;
using System.Collections;
using System;
using NUnit.Framework;
using UnityEngine;

public class Paintroller : MonoBehaviour
{
    [SerializeField] private bool destroyAtmaxLifespan = true;
    [SerializeField] private float maxLifespan = 10f;
    [SerializeField] private float splatRate = 1f;
    [SerializeField] private float selfDamagePerSplat = 1f;
    [SerializeField] private float displacementMinimum = 10f;
    [SerializeField] GameObject splatterPrefab;
    
    private Rigidbody rb;
    private bool hasCollided = false;
    private bool isColliding = false;
    private bool canSplatter = true;

    // Cache the contact and position data
    private Vector3 lastContactPoint;
    private Vector3 lastContactNormal;

    private Vector3 currentPosition;
    private Vector3 lastSplatPosition;
    private float displacementFromLastSplat;
    
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        if (destroyAtmaxLifespan == true)
        {
            Destroy(gameObject, maxLifespan);
        }
    }

    void OnCollisionEnter(Collision co)
    {
        if(co.gameObject.tag != "Bullet" && co.gameObject.tag != "Player")
        {
            hasCollided = true;
            isColliding = true;
        }
    }

    void OnCollisionStay(Collision co)
    {
        if (canSplatter == true && displacementFromLastSplat > displacementMinimum)
        {
            canSplatter = false;
            Splatter(co);
            StartCoroutine(ResetCanSplatter(splatRate));
        }
    }

    void OnCollisionExit()
    {
        isColliding = false;
    }

    void Splatter(Collision co)
    {
        // Use the first contact point from the actual collision
        ContactPoint contact = co.GetContact(0);

        // Align the decal to the surface normal so it lies flat on any surface
        Quaternion splatterRotation = Quaternion.LookRotation(-contact.normal);

        Instantiate(splatterPrefab, contact.point, splatterRotation);

        lastSplatPosition = currentPosition;

        if (TryGetComponent<HealthSystem>(out var health))
        {
            health.TakeDamage(selfDamagePerSplat);
        };
    }

    void Update()
    {
        currentPosition = rb.transform.position;
        displacementFromLastSplat =  Vector3.Distance(currentPosition, lastSplatPosition);
    }

    IEnumerator ResetCanSplatter(float splatRate)
    {
        yield return new WaitForSeconds(1/splatRate); // wait for 1/firerate seconds
        canSplatter = true; // allow shooting again after the stun duration
    }
}
