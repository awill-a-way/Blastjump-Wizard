using UnityEngine;

public class MeltingButterStick : MonoBehaviour
{
    private HealthSystem health;
    [SerializeField] private float minHeight = 0.1f;
    [SerializeField] private float maxHeight = 1f;
    [SerializeField] private float groundOffset = -0.5f;
    private float cachedHealth;
    private float currentHeight;
    private Transform target;

    void Start()
    {
        target = GetComponentInParent<Rigidbody>().transform;
        health = GetComponentInParent<HealthSystem>();

        if (health != null)
        {
            maxHeight = 3f*(health.MaxHealth / 100f);
            currentHeight = maxHeight;

            cachedHealth = health.CurrentHealth;

            UpdateVisual();
        }
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;

        if (health != null && health.CurrentHealth != cachedHealth)
            UpdateVisual();

        // Single source of truth for position — all in world space
        float groundedY = target.position.y + groundOffset + (currentHeight / 2f);
        transform.position = new Vector3(target.position.x, groundedY, target.position.z);
    }

    void UpdateVisual()
    {
        if (health != null)
        {
            float t = health.CurrentHealth / 100;
            currentHeight = Mathf.Lerp(minHeight, maxHeight, t);

            transform.localScale = new Vector3(transform.localScale.x, currentHeight, transform.localScale.z);

            cachedHealth = health.CurrentHealth;
        }
    }
}