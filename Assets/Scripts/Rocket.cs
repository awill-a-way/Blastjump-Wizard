using UnityEngine;
using UnityEngine.UIElements;

public class Rocket : MonoBehaviour
{
    [Tooltip("Place the appropriate explosion prefab for the spell into this slot")]
    [SerializeField] private GameObject explosionVisual;
    [Space]

    private Blast _blast;
    private bool hasCollided;
    private Rigidbody rb;
    public float explosionRadius = 10f;
    public float explosionForce = 50f;
    public float explosiveDamage = 10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Start()
    {
        _blast = GetComponent<Blast>();
        Destroy (gameObject, 10);
    }

    void OnCollisionEnter(Collision co)
    {
        if(co.gameObject.tag != "Bullet" && co.gameObject.tag != "Player" && !hasCollided)
        {
            hasCollided = true;
            Explode(gameObject.transform.position);
            Destroy(gameObject);
        }
    }

    void Explode(Vector3 centre)
    {
        if(explosionVisual != null)
        {
            Destroy(Instantiate(explosionVisual, centre, Quaternion.identity), 5);
        }

        _blast.BlastEverything(centre, explosionRadius, explosionForce, explosiveDamage);
    }
}
