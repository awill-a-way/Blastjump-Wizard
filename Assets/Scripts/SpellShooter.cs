using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;
using System;

public class SpellShooter : MonoBehaviour
{
    private PlayerCharacter _playerCharacter;
    private PlayerInputActions _inputActions;
    private SpellSelector _selectSpells;
    private Melee melee;
    private HUDController HUD;
    
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
    public float currentMana = 100;
    private float maxMana = 100;
    private float baseManaRegenRate = 5;
    private float manaRegenModifier = 0f;
    public bool manaRegenEnabled = true;
    private float manaRegenStunTimer = 0f;
    public string currentSpell = "";
    private string tempPreviousSpell = "";
    private Vector3 projectileDestination;
    private bool rightHand = true;
    public static float floatingDiscCount = 0f;
    
    void Start()
    {
        _playerCharacter = PlayerCharacter.Instance;
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        _selectSpells = GetComponent<SpellSelector>();

        melee = GetComponent<Melee>();

        spellCharge = 1f;

        HUD = GetComponentInChildren<HUDController>();
    }

    void OnDestroy()
    {
        _inputActions.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        if (_inputActions.Gameplay.Spellbook.IsPressed() == false)
        {
            AttackCheck();
        }
        
        currentMana = Mathf.Min(currentMana, maxMana);

        if (timeSinceShot > 5f)
        {
            rightHand = true;
        }

        Mathf.Clamp(spellCharge, 1, 10);
        Mathf.Clamp(timeSinceShot, 0, 50);

        HUD.UpdateManaUI(currentMana, maxMana);
    }

    void LateUpdate()
    {
        if (_selectSpells.currentSpell != tempPreviousSpell)
        {
            spellCharge = 0f;
            tempPreviousSpell = _selectSpells.currentSpell;
        }
    }

    void FixedUpdate()
    {
        //Regen currentMana if below max
        if (currentMana < maxMana && manaRegenEnabled == true)
        {
            currentMana += (baseManaRegenRate + manaRegenModifier) * Time.deltaTime; // regenerate currentMana over time
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

    void AttackCheck()
    {
        var input = _inputActions.Gameplay;
        
        if ( input.QuickMelee.IsPressed())
        {
            if (melee.canMelee == true)
            {
                melee.Strike();
                _selectSpells.inputStunTimer = _selectSpells.inputStunTime;
            }
        }
        
        if (input.Fire1.IsPressed() && _selectSpells.currentSpell != "No Spell")
        {
            if (canShoot == true)
            {
                canShoot = false; // prevent shooting again until the stun duration is over (may have to move for charged attacks)
                CastSpell();
                
                _selectSpells.inputStunTimer = _selectSpells.inputStunTime;
                StopCoroutine(melee.resetRoutine);
                melee.PutMeleeOnCooldown(0.25f);
            }
        }
        
        if (input.Fire2.IsPressed())
        {
            if (_selectSpells.currentSpell == "No Spell")
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

        if ((input.Fire1.IsPressed() && _selectSpells.currentSpell == "No Spell") || input.QuickMelee.IsPressed())
        {
            if (melee.canMelee == true)
            {
                melee.Strike();
                _selectSpells.inputStunTimer = _selectSpells.inputStunTime;
            }
        }
    }

    void CastSpell()
    {
        if (_selectSpells.currentSpell == "Knockblast" && currentMana >= 10)
            {
                StartCoroutine(ShootProjectile("Knockblast", 50f+5f*spellCharge));
                EveryCastDoesThis(10,1.5f);
            }
        else if (_selectSpells.currentSpell == "Mana Cube" && currentMana >= 25)
            {
                StartCoroutine(ShootProjectile("Mana Cube", 5f));
                EveryCastDoesThis(25,0.5f);
            }
        else if (_selectSpells.currentSpell == "Glyph of Volatility" && currentMana >= 20)
            {
                StartCoroutine(ShootProjectile("Glyph of Volatility", 0));
                EveryCastDoesThis(20,2);
            }
        else if (_selectSpells.currentSpell == "Lightning Bolt" && currentMana >= 10)
            {
                EveryCastDoesThis(10,2);
            }
        else if (_selectSpells.currentSpell == "Ice Shard" && currentMana >= 15)
            {
                StartCoroutine(ShootProjectile("Ice Shard", 50f));
                EveryCastDoesThis(15,2);
            }
        else if (_selectSpells.currentSpell == "Snowball" && currentMana >= 15)
            {
                StartCoroutine(ShootProjectile("Snowball", 30f));
                EveryCastDoesThis(15,2);
            }
        else if (_selectSpells.currentSpell == "Polymorph: Disc" && currentMana >= 15)
            if (floatingDiscCount < 3)
            {
                Polymorph("Disc", 20f, 15f+spellCharge);
                EveryCastDoesThis(0f,2); //the manacost is on the polymorph() method
            }
            else
            {
                Debug.Log(floatingDiscCount+" is too many floating discs to create another, cast failed!");
            }
        else if (_selectSpells.currentSpell == "Heal" && currentMana >= 30)
            {
                if (TryGetComponent<HealthSystem>(out var health) && health.CurrentHealth != 100f) //max health check
                {
                    health.Heal(10*spellCharge); //may change
                }
                EveryCastDoesThis(0f,0f);
            }
        else if (_selectSpells.currentSpell == "Shield" && currentMana >= 20)
            {
                shieldTimer = 5f; // shield lasts for 5 seconds
                EveryCastDoesThis(20,2);
            }
        else if (_selectSpells.currentSpell == "Vine Grapple" && currentMana >= 25)
            {
                EveryCastDoesThis(25,2);
            }
        else if (_selectSpells.currentSpell == "Psionic Grasp" && currentMana >=1)
            {
                //Put in the half life phys gun basically
                currentMana -= Time.deltaTime;
                manaRegenStunTimer = 0.5f;
                timeSinceShot = 0f;
                _selectSpells.inputStunTimer = _selectSpells.inputStunTime;
                _selectSpells.spellResetTimer = 10f;
            }
        else if (_selectSpells.currentSpell != "No Spell")
            {
                Debug.Log("Not enough currentMana to cast " + currentSpell + "! Remaining currentMana: " + (currentMana));
                _selectSpells.spellResetTimer = 10f;
                StartCoroutine(ResetCanShoot(2));
            }
    }

    void AltCastSpell()
    {
        if  (currentMana > 0f)
        {
            if (_selectSpells.currentSpell == "Knockblast" || _selectSpells.currentSpell == "Mana Cube" || _selectSpells.currentSpell == "Polymorph: Disc" || _selectSpells.currentSpell == "Heal")
            {
                currentMana -= 5f*Time.deltaTime;
                spellCharge += Time.deltaTime;
                manaRegenStunTimer = 0.5f;
            }
        }
    }

    void EveryCastDoesThis(float manaCost, float fireRate)
    {
        StartCoroutine(ResetCanShoot(fireRate));
        currentMana -= manaCost;
        timeSinceShot = 0f;
        _selectSpells.inputStunTimer = _selectSpells.inputStunTime;
        _selectSpells.spellResetTimer = 10f;
        if (HUD != null)
        {
            HUD.UpdateManaUI(currentMana, maxMana);
        }
        Debug.Log("Casting "+ _selectSpells.currentSpell + "! Remaining currentMana: " + (currentMana));
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
                currentMana -= manaCost;
            }
            else if (target.CompareTag("Polymorph form"))
            {
                currentMana -= manaCost;
                Debug.Log("Polymorph duration extended!");
                polymorpher = target.GetComponentInParent<Polymorphable>();
                polymorpher.TriggerPolymorphation(prefab, duration, spawnUpright);
            }
            else
            {
                Debug.Log("Can't polymorph "+target+"! currentMana was not deducted!");
            }
        }
    }

    IEnumerator ResetCanShoot(float fireRate)
    {
        if (fireRate > 0f)
        {
            yield return new WaitForSeconds(1/fireRate); // wait for 1/firerate seconds
            canShoot = true; // allow shooting again after the stun duration
        }
        else if (fireRate <= 0f)
        {
            yield return new WaitForSeconds(0f);
            canShoot = true;
        }
    }
}