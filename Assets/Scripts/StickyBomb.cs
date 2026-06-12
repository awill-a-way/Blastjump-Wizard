using UnityEngine;
using UnityEngine.UIElements;

public class Stickybomb : MonoBehaviour
{
    [Tooltip("Place the appropriate explosion prefab for the spell into this slot")]
    [SerializeField] private GameObject explosionVisual;
    [Space]
    [SerializeField] private bool canDetonateRemotely;
    [SerializeField] private bool canDetonateReactively;
    private PlayerInputActions _inputActions;
    private Blast _blast;
    private bool hasCollided;
    private bool hasDetonated;
    [SerializeField] private float explosionRadius = 15f;
    [SerializeField] private float explosionForce = 75f; 
    [SerializeField] private float damage = 25f;
    //private Rigidbody rb;

    void Awake()
    {
       // rb = GetComponent<Rigidbody>();
       // rb.useGravity = false;
       // rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Start()
    {
        _blast = GetComponent<Blast>();
        Destroy (gameObject, 100);

        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        if (canDetonateReactively)
        {
            var health = GetComponent<HealthSystem>();
            if (health != null)
                health.OnDied.AddListener(() => Detonate());
        }
    }

    void OnCollisionEnter(Collision co)
    {
        if(co.gameObject.tag != "Bullet" && co.gameObject.tag != "Player" && !hasCollided)
        {
            hasCollided = true;
            //rb.linearVelocity = Vector.0;
        }
    }

    void Update()
    {
        var input = _inputActions.Gameplay;
        
        if (canDetonateRemotely == true)
        {
            if (input.Fire2.WasPressedThisFrame() && !hasDetonated)
            {
                hasDetonated = true;
                Detonate();
            }
        }
    }

    void OnDestroy()
    {
        _inputActions.Dispose();
    }

    public void Detonate()
    {
        if(explosionVisual != null)
        {
            Destroy(Instantiate(explosionVisual, gameObject.transform.position, Quaternion.identity), 5);
        }

        _blast.BlastEverything(gameObject.transform.position, explosionRadius, explosionForce, damage);
        Destroy(gameObject);
    }
}