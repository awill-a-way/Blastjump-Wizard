using UnityEngine;
using UnityEngine.UIElements;

public class Rocket : MonoBehaviour
{
    private Blast _blast;
    private bool hasCollided;
    private Rigidbody rb;
    public bool isPlayerRocket;
    [SerializeField] private float playerSelfDamageMultiplier = 0.5f;
    [SerializeField] private float airshotBonus = 0.5f;
    [Space]
    [Tooltip("Place the appropriate explosion prefab for the spell into this slot")]
    [SerializeField] private GameObject explosionVisual;
    [Space]
    [SerializeField] private float collisionDamage = 10f;
    [SerializeField] private float baseExplosionRadius = 10f;
    [SerializeField] private float baseExplosionForce = 50f;
    [SerializeField] private float baseExplosiveDamage = 2.5f;
    [HideInInspector] public float rocketStrength = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _blast = GetComponent<Blast>();
    }

    void Start()
    {
        Destroy (gameObject, 15f);
    }

    void OnCollisionEnter(Collision co)
    {
        if(co.gameObject.tag != "Bullet" && !hasCollided)
        {
            if(co.gameObject.tag != "Player" || isPlayerRocket == false)
            {
                hasCollided = true;

                if (co.gameObject.TryGetComponent<HealthSystem>(out var health))
                {
                    health.TakeDamage(collisionDamage*rocketStrength);
                    
                    if (isPlayerRocket == true && co.gameObject.tag == "Enemy")
                    {
                        rocketStrength += airshotBonus;
                    }
                }
                
                Detonate(gameObject.transform.position);
            }
        }
    }

    public void Detonate(Vector3 centre)
    {
        if(explosionVisual != null)
        {
            Destroy(Instantiate(explosionVisual, centre, Quaternion.identity), 5);
        }

        var explosionRadius = baseExplosionRadius*rocketStrength;
        var explosionForce = baseExplosionForce*rocketStrength;
        var explosiveDamage = baseExplosiveDamage*rocketStrength;

        var rocketStrengthAlt = rocketStrength-1f;

        if (isPlayerRocket == true)
        {
            _blast.BlastPlayer(centre, explosionRadius, baseExplosionForce*(1+0.5f*rocketStrengthAlt), explosiveDamage*playerSelfDamageMultiplier);
            _blast.BlastNPCs(centre, explosionRadius, explosionForce, explosiveDamage);
            _blast.BlastRigidbodies(centre, explosionRadius, explosionForce);
        }
        else
        {
            _blast.BlastEverything(centre, explosionRadius, explosionForce, explosiveDamage);
        }
        
        Debug.Log("Rocket strength =" +rocketStrength);
        Destroy(gameObject);
    }
}