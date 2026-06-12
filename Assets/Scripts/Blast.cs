using UnityEngine;
using KinematicCharacterController;

public class Blast : MonoBehaviour
{
    private PlayerCharacter _playerCharacter;

    void Awake()
    {
        _playerCharacter = PlayerCharacter.Instance;
    }

    public void BlastPlayer(Vector3 centre, float explosionRadius, float explosionForce, float damage)
    {
        var explosionToPlayer = _playerCharacter.transform.position - centre;
        if (explosionToPlayer.magnitude < explosionRadius)
        {
            var distanceFactor = Mathf.Clamp((explosionRadius-explosionToPlayer.magnitude)/explosionRadius,0,1);
            var explosionForceDirection = explosionToPlayer.normalized;
            var damageToPlayer = damage*distanceFactor;
            _playerCharacter.UnstickFromGround();
            _playerCharacter.AddVelocity(explosionForceDirection*explosionForce*distanceFactor);
            

            var health = _playerCharacter.GetComponentInParent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damageToPlayer);
            }

            Debug.Log("Player blasted! Player distance to explosion centre = "+explosionToPlayer.magnitude+", Distance Force Multiplier = "+distanceFactor+", Player took "+damageToPlayer+" damage!");
        }
        else
        {
            Debug.Log("Player not blasted! Player distance to explosion centre = "+explosionToPlayer.magnitude);
        }
    }

    public void BlastNPCs(Vector3 centre, float explosionRadius, float explosionForce, float damage)
    {
        Collider[] hits = Physics.OverlapSphere(centre, explosionRadius);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Player") && hit.TryGetComponent<HealthSystem>(out var health))
            {
                health.TakeDamage(damage);
            }
        }
    }

    public void BlastRigidbodies(Vector3 centre, float explosionRadius, float explosionForce)
    {
        Collider[] hits = Physics.OverlapSphere(centre, explosionRadius);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Player") && !hit.CompareTag("Bullet") && hit.TryGetComponent<Rigidbody>(out var rb))
            {
                var centreToRigidbody = rb.transform.position - centre;
                rb.AddForce(50f*explosionForce*(centreToRigidbody.normalized));
            }
        }
    }

    public void BlastEverything(Vector3 centre, float explosionRadius, float explosionForce, float damage)
    {
        BlastPlayer(centre, explosionRadius, explosionForce, damage);
        BlastNPCs(centre, explosionRadius, explosionForce, damage);
        BlastRigidbodies(centre, explosionRadius, explosionForce);
    }
}
