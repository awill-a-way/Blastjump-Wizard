using System.Collections.Generic;
using System.Collections;
using KinematicCharacterController;
using UnityEngine;
using Unity.VisualScripting;
using System.Linq;

public class Melee : MonoBehaviour
{
    private PlayerCharacter _playerCharacter;
    public Camera playerCamera;
    private Vector3 projectileDestination;
    private float parryMultiplier = 2f;
    public bool canMelee = true;
    public Coroutine resetRoutine;
    private bool hitNonPlayer;
    private bool hitOnlyBullets; // was gonna use for parry bounce, may remove
    [Range(0.1f,5)]
    [SerializeField] private float meleeFireRate = 1f;
    [Range(1,10)]
    [SerializeField] private float bounceMultiplier = 1f;
    [Range(1,10)]
    [SerializeField] private float meleeRange = 5f;
    [Range(1,50)]
    [SerializeField] private float bounceVelocityThreshold = 20f;
    [Range(1,50)]
    [SerializeField] private float minMeleeDamage = 5f;

    
    void Start()
    {
        _playerCharacter = PlayerCharacter.Instance;
    }

    public void Strike()
    {
        if(canMelee == true)
        {
            canMelee = false;
            hitNonPlayer = false;
            hitOnlyBullets = true; // using for parry bounce compatibility with normal bounce, may remove
            Collider[] hits = Physics.OverlapSphere(playerCamera.transform.position, meleeRange);

            foreach (Collider hit in hits)
            {
                if (!hit.CompareTag("Player"))
                {
                    hitNonPlayer = true;
                    if (hit.TryGetComponent<HealthSystem>(out var health))
                    {
                        health.TakeDamage(Mathf.Max(_playerCharacter.GetState().Velocity.magnitude/minMeleeDamage, minMeleeDamage));
                    }
                }

                if(hit.CompareTag("Bullet"))
                {
                    Parry(hit.gameObject);
                }
                else if(!hit.CompareTag("Player")) 
                {
                    hitOnlyBullets = false;

                    if (hit.TryGetComponent<Rigidbody> (out var rb))
                    {
                        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                        var forceDirection = ray.GetPoint(1000);
                        rb.AddForce(500f*forceDirection.normalized);
                    }
                }
            }
            if (hitNonPlayer == true && hitOnlyBullets == false)
            {
                BounceOff();
            }

            resetRoutine = StartCoroutine(ResetCanMelee(1f/meleeFireRate));
            hitNonPlayer = false;
            Debug.Log("Performing melee attack! Melee hits: " + hits.Length);
            foreach (Collider hit in hits)
            {
                Debug.Log("Hit: " + hit.name + " | Tag: " + hit.tag);
            }
        }
    }

    private void Parry(GameObject bullet)
    {
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null) return;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        projectileDestination = ray.GetPoint(1000);
        var initialBulletVelocity = rb.linearVelocity;
        
        //Parry Bullet
        rb.linearVelocity = (projectileDestination - playerCamera.transform.position).normalized*(initialBulletVelocity.magnitude*parryMultiplier + _playerCharacter.GetState().Velocity.magnitude);
        
        //Parry bounce player
        _playerCharacter.UnstickFromGround();
        _playerCharacter.AddVelocity((parryMultiplier*initialBulletVelocity.magnitude + _playerCharacter.GetState().Velocity.magnitude)*-ray.direction.normalized);
    }

    private void BounceOff() //Bounce converts ALL velocity into vertical velocity (downwards or upwards based on current velocity)
    {
        var initialVelocity = _playerCharacter.GetState().Velocity;
        if (initialVelocity.magnitude > bounceVelocityThreshold*bounceMultiplier)
        {
            var initialHorizontalVelocity = new Vector3(initialVelocity.x, 0f, initialVelocity.z);
            var bounceChange = new Vector3(-initialVelocity.x, bounceMultiplier*initialHorizontalVelocity.magnitude, -initialVelocity.z);  
            _playerCharacter.UnstickFromGround();
            _playerCharacter.AddVelocity(bounceChange);
            Debug.Log("Melee bounce! Bounce added " +bounceMultiplier*initialHorizontalVelocity.magnitude+ " to vertical velocity!"); //PROBLEM: seems to always be applying whenever melee
        }
    }

    public void PutMeleeOnCooldown(float cooldown)
    {
        canMelee = false;
        resetRoutine = StartCoroutine(ResetCanMelee(cooldown));
    }

    IEnumerator ResetCanMelee(float cooldown)
    {
        yield return new WaitForSeconds(cooldown); // wait for [cooldown] seconds
        canMelee = true; // allow shooting again after the stun duration
    }
}
