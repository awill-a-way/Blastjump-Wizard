using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SplatterEffects : MonoBehaviour
{
    private PlayerCharacter _playerCharacter;
    //[SerializeField] private float alteredSlideFriction = 0.2f;
    [Range(0f,2f)]
    public float frictionOnSplatter = 0f;
    [SerializeField] private float lifespan = 15f;
    [SerializeField] private float timeMultiplier = 1f;
    [Range(1f,3f)]
    [SerializeField] private float ignitedTimeMultiplier = 3f;
    [SerializeField] private bool _canIgnite = true;
    private bool _isIgnited = false;
    private bool canCheckBurn = false;


    void Start()
    {
        _playerCharacter = PlayerCharacter.Instance;
        var health = GetComponent<HealthSystem>();
            if (health != null && _canIgnite == true)
                health.OnDied.AddListener(() => Ignite());
    }

    void Ignite()
    {
        _isIgnited = true;
    }

    void Update()
    {
        if (_isIgnited == false)
        {
            timeMultiplier = 1f;
        }
        else if (_isIgnited == true)
        {
            timeMultiplier = ignitedTimeMultiplier;
        }

        lifespan -= Time.deltaTime*timeMultiplier;
        if (lifespan < 0f)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionStay(Collision co)
    {
        if (canCheckBurn == true && _isIgnited == true && co.gameObject.TryGetComponent<HealthSystem>(out var health))
        {
            canCheckBurn = false;
            StartCoroutine(ApplyBurn(health, 0.5f));
        }

        if (co.gameObject.tag == "Enemy")
        {
            
        }
    }

    IEnumerator ApplyBurn(HealthSystem health, float checksPerSecond)
    {
        health.TakeDamageOverTime(1f, 1f, 3f, "burn");

        yield return new WaitForSeconds(1/checksPerSecond);
        canCheckBurn = true;
    }

    void OnDestroy()
    {
        
    }
}