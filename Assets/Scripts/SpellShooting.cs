using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;
using System;

public class SpellShooting : MonoBehaviour
{
    private PlayerCharacter _playerCharacter;
    private PlayerInputActions _inputActions;
    private SpellInput _inputSpells;
    private Melee melee;
    
    [SerializeField] private GameObject knockblastPrefab;
    [SerializeField] private GameObject manaCubePrefab;
    [SerializeField] private GameObject volatilePrefab;
    [SerializeField] private GameObject iceShardPrefab;
    [SerializeField] private GameObject floatingDiscPrefab;
    [SerializeField] private GameObject snowballPrefab;
    
    public Transform LHFirePoint, RHFirePoint;
    private bool canShoot = true;
    //private bool _requestedShoot = false;
    private bool requestingAltCast = false;
    public float timeSinceShot = 0f;
    //private float shootCoyoteTimer = 0;
    public float spellCharge = 1f;
    public Camera playerCamera;
    private float currentSpellTimer;
    public float shieldTimer;
    [Range(0, 100)]
    public float mana = 100;
    private float manaMax = 100;
    private float baseManaRegenRate = 5;
    private float manaRegenModifier = 0f;
    public bool manaRegenEnabled = true;
    private float manaRegenStunTimer = 0f;
    public string currentSpell = "";
    private string previousSpell = "";
    private Vector3 projectileDestination;
    private bool rightHand = true;
    public static float floatingDiscCount = 0f;
    
    void Start()
    {
        _playerCharacter = PlayerCharacter.Instance;
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        _inputSpells = GetComponent<SpellInput>();

        melee = GetComponent<Melee>();

        spellCharge = 1f;
    }

    void OnDestroy()
    {
        _inputActions.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        ShootCheck();
        
        mana = Mathf.Min(mana, manaMax);

        if (timeSinceShot > 5f)
        {
            rightHand = true;
        }

        Mathf.Clamp(spellCharge, 1, 10);
        Mathf.Clamp(timeSinceShot, 0, 50);
    }

    void LateUpdate()
    {
        if (_inputSpells.currentSpell != previousSpell)
        {
            spellCharge = 1f;
            previousSpell = _inputSpells.currentSpell;
        }
    }

    void FixedUpdate()
    {
        //Regen mana if below max
        if (mana < manaMax && manaRegenEnabled == true)
        {
            mana += (baseManaRegenRate + manaRegenModifier) * Time.deltaTime; // regenerate mana over time
        }

        //Measure time since the last successful shot
        timeSinceShot += Time.deltaTime;

        //Decrease knockblast charge
        if (spellCharge > 0 && requestingAltCast == false)
        {
            spellCharge -= Time.deltaTime*0.5f;
        }
        


        if (manaRegenStunTimer > 0)
        {
            manaRegenEnabled = false;
            manaRegenStunTimer -= Time.deltaTime;
        }
        else
        {
            manaRegenStunTimer = Mathf.Max(manaRegenStunTimer, 0);
            manaRegenEnabled = true;
        }
    }

    void ShootCheck()
    {
        var input = _inputActions.Gameplay;
        
        if ( input.QuickMelee.IsPressed())
        {
            if (melee.canMelee == true)
            {
                melee.Strike();
                _inputSpells.inputStunTimer = _inputSpells.inputStunTime;
            }
        }
        
        if (input.Fire1.IsPressed() && _inputSpells.currentSpell != "No Spell")
        {
            if (canShoot == true)
            {
                canShoot = false; // prevent shooting again until the stun duration is over (may have to move for charged attacks)
                CastSpell();
                
                _inputSpells.inputStunTimer = _inputSpells.inputStunTime;
                StopCoroutine(melee.resetRoutine);
                melee.PutMeleeOnCooldown(0.25f);
            }
        }
        
        if (input.Fire2.IsPressed())
        {
            if (_inputSpells.currentSpell == "No Spell")
            {
                
            }
            else
            {
                AltCastSpell();
            }
            requestingAltCast = true;
        }
        else
        {
            requestingAltCast = false;
        }

        if ((input.Fire1.IsPressed() && _inputSpells.currentSpell == "No Spell") || input.QuickMelee.IsPressed())
        {
            if (melee.canMelee == true)
            {
                melee.Strike();
                _inputSpells.inputStunTimer = _inputSpells.inputStunTime;
            }
        }
    }

    void CastSpell()
    {
        if (_inputSpells.currentSpell == "Knockblast" && mana >= 10)
            {
                StartCoroutine(ShootProjectile("Knockblast", 50f+5f*spellCharge));
                EveryCastDoesThis(10,1.5f);
            }
        else if (_inputSpells.currentSpell == "Mana Cube" && mana >= 25)
            {
                StartCoroutine(ShootProjectile("Mana Cube", 25f));
                EveryCastDoesThis(5,5);
            }
        else if (_inputSpells.currentSpell == "Glyph of Volatility" && mana >= 20)
            {
                StartCoroutine(ShootProjectile("Glyph of Volatility", 0));
                EveryCastDoesThis(20,2);
            }
        else if (_inputSpells.currentSpell == "Lightning Bolt" && mana >= 10)
            {
                EveryCastDoesThis(10,2);
            }
        else if (_inputSpells.currentSpell == "Ice Shard" && mana >= 15)
            {
                StartCoroutine(ShootProjectile("Ice Shard", 50f));
                EveryCastDoesThis(15,2);
            }
        else if (_inputSpells.currentSpell == "Snowball" && mana >= 15)
            {
                StartCoroutine(ShootProjectile("Snowball", 30f));
                EveryCastDoesThis(15,2);
            }
        else if (_inputSpells.currentSpell == "Polymorph: Disc" && mana >= 15)
            if (floatingDiscCount < 3)
            {
                Polymorph("Disc", 20f, 15f);
                EveryCastDoesThis(0f,2); //the manacost is on the polymorph() method
            }
            else
            {
                Debug.Log(floatingDiscCount+" is too many floating discs to create another, cast failed!");
            }
        else if (_inputSpells.currentSpell == "Heal" && mana >= 30)
            {
                var health = GetComponentInParent<HealthSystem>();
                if (health != null && health.CurrentHealth != 100f) //max health check
                {
                    health.Heal(Time.deltaTime); //may change
                    mana -= Time.deltaTime*5f;
                    manaRegenStunTimer = 0.5f;
                }
                StartCoroutine(ResetCanShoot(2));
            }
        else if (_inputSpells.currentSpell == "Shield" && mana >= 20)
            {
                shieldTimer = 5f; // shield lasts for 5 seconds
                EveryCastDoesThis(20,2);
            }
        else if (_inputSpells.currentSpell == "Vine Grapple" && mana >= 25)
            {
                EveryCastDoesThis(25,2);
            }
        else if (_inputSpells.currentSpell == "Psionic Grasp" && mana >=1)
            {
                //Put in the half life phys gun basically
                mana -= Time.deltaTime;
                manaRegenStunTimer = 0.5f;
                timeSinceShot = 0f;
                _inputSpells.inputStunTimer = _inputSpells.inputStunTime;
                _inputSpells.spellResetTimer = 10f;
            }
        else if (_inputSpells.currentSpell != "No Spell")
            {
                Debug.Log("Not enough mana to cast " + currentSpell + "! Remaining Mana: " + (mana));
                _inputSpells.spellResetTimer = 10f;
                StartCoroutine(ResetCanShoot(2));
            }
    }

    void AltCastSpell()
    {
        if (_inputSpells.currentSpell == "Knockblast" || _inputSpells.currentSpell == "Mana Cube")
        {
            mana -= 5f*Time.deltaTime;
            spellCharge += Time.deltaTime;
            manaRegenStunTimer = 0.5f;
        }
    }

    void EveryCastDoesThis(float manaCost, float fireRate)
    {
        StartCoroutine(ResetCanShoot(fireRate));
        mana -= manaCost;
        timeSinceShot = 0f;
        _inputSpells.inputStunTimer = _inputSpells.inputStunTime;
        _inputSpells.spellResetTimer = 10f;
        Debug.Log("Casting "+ _inputSpells.currentSpell + "! Remaining Mana: " + (mana));
    }

    IEnumerator ShootProjectile(string spellName, float launchForce)
    {
        yield return new WaitForFixedUpdate();

        // Aim using raycast
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if(Physics.Raycast(ray, out hit))
            projectileDestination = hit.point;
        else
            projectileDestination = ray.GetPoint(1000);

        // Decide which hand to fire from
        if(rightHand)
        {
            rightHand = false;
            SpawnProjectile(spellName, launchForce, RHFirePoint);
        }
        else
        {
            rightHand = true;
            SpawnProjectile(spellName, launchForce, LHFirePoint);
        }
    }

    void SpawnProjectile(string spellName, float launchForce, Transform firepoint)
    {
        GameObject prefab = spellName switch
        {
            "Knockblast" => knockblastPrefab,
            "Mana Cube" => manaCubePrefab,
            "Glyph of Volatility" => volatilePrefab,
            "Ice Shard" => iceShardPrefab,
            "Snowball" => snowballPrefab,
            _ => null
        };

        if (prefab == null)
        {
            Debug.LogWarning($"No prefab assigned for {spellName}!");
            return;
        }

        if(spellName == "Glyph of Volatility")
        {
            var projectileObj = Instantiate(prefab, projectileDestination, Quaternion.identity) as GameObject;
        }
        else
        {
            var projectileObj = Instantiate(prefab, firepoint.position, Quaternion.identity) as GameObject;
            projectileObj.GetComponent<Rigidbody>().linearVelocity = (projectileDestination - firepoint.position).normalized * (launchForce + _playerCharacter.GetState().Velocity.magnitude);
            if (projectileObj.TryGetComponent<HealthSystem>(out var health))
            {
                health.SetMaxHealth((100f+spellCharge*10f), true);
            }
            
        }

        spellCharge = 0f;
    }

    private void Polymorph(string polymorphName, float manaCost, float duration)
    {
        GameObject prefab = polymorphName switch
        {
            "Disc" => floatingDiscPrefab,
            _ => null
        };

        bool spawnUpright = polymorphName switch
        {
            "Disc" => true,
            _ => false
        };
        
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 50f))
        {
            var target = hit.collider.gameObject;
            var polymorpher = target.GetComponent<Polymorphable>();
            
            if (polymorpher != null && polymorpher._canPolymorph == true)
            {
                polymorpher.TriggerPolymorphation(prefab, duration, spawnUpright);
                mana -= manaCost;
            }
            else if (target.CompareTag("Polymorph form"))
            {
                mana -= manaCost;
                Debug.Log("Polymorph duration extended!");
                polymorpher = target.GetComponentInParent<Polymorphable>();
                polymorpher.TriggerPolymorphation(prefab, duration, spawnUpright);
            }
            else
            {
                Debug.Log("Can't polymorph "+target+"! Mana was not deducted!");
            }
        }
    }

    IEnumerator ResetCanShoot(float fireRate)
    {
        yield return new WaitForSeconds(1/fireRate); // wait for 1/firerate seconds
        canShoot = true; // allow shooting again after the stun duration
    }
}