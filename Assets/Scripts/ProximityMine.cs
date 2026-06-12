using UnityEngine;

public class ProximityMine : MonoBehaviour
{
    private float armingTimer = 0f;
    [Range(0f,5f)]
    [SerializeField] private float timeToArm = 2f;
    private Stickybomb _stickybomb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _stickybomb = GetComponent<Stickybomb>();
    }

    // Update is called once per frame
    void Update()
    {
        armingTimer += Time.deltaTime;
        if (armingTimer > timeToArm)
        {
            Collider[] hits = Physics.OverlapSphere(gameObject.transform.position, armingTimer);

            foreach (Collider hit in hits)
            {
                if (!hit.CompareTag("Enemy"))
                {
                    _stickybomb.Detonate();
                }
            }
        }
    }
}
