using System;
using UnityEngine;

public class Paintball : MonoBehaviour
{
    [SerializeField] GameObject puddlePrefab;
    private Rigidbody rb;
    private bool hasCollided = false;

    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision co)
    {
        if(co.gameObject.tag != "Bullet" && co.gameObject.tag != "Player" && !hasCollided)
        {
            hasCollided = true;
            Splatter(co);
            Destroy(gameObject);
        }
    }

    void Splatter(Collision co)
    {
        // Use the first contact point from the actual collision
        ContactPoint contact = co.GetContact(0);

        // Align the decal to the surface normal so it lies flat on any surface
        Quaternion splatterRotation = Quaternion.LookRotation(-contact.normal);

        Instantiate(puddlePrefab, contact.point, splatterRotation);
    }
}
