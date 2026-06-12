using System;
using System.Collections;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool startAtMaxHealth = true;
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private bool isPlayer = true;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }
    public bool IsInvulnerable = false;
    private Coroutine burningRoutine;
    private Coroutine poisonedRoutine;

    public event Action<HealthSystem> OnHealthChanged;
    public UnityEvent OnDied;

    void Awake()
    {
        if (startAtMaxHealth)
        {
            CurrentHealth = maxHealth;
        }
        else
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f || IsInvulnerable)
            return;
        Debug.Log("Damage:" + damage);
        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0f, maxHealth);
        OnHealthChanged?.Invoke(this);
        if (isPlayer)
        {
            //if (HealthBar.Fill)
            {
            //    HealthBar.Fill.value = CurrentHealth / maxHealth;
            }
        }

        if (CurrentHealth <= 0f)
        {
            Debug.Log("Death");
            Die();
        }
    }

    public void Kill()
    {
        if (IsDead) return;

        CurrentHealth = 0f;
        OnHealthChanged?.Invoke(this);
        Die();
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;
        OnDied?.Invoke();
        if (destroyOnDeath && !isPlayer)
        {
            Destroy(gameObject);
        }
        else
        {
            // Optional: disable components here if you dont destroy on death
        }
    }

    public void ResetDeadState()
    {
        IsDead = false;
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return;
        
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(this);
    }

    public void RestoreToFull()
    {
        if (IsDead) return;

        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(this);
    }

    public void SetMaxHealth(float newMax, bool refill = true)
    {
        maxHealth = Mathf.Max(1f,newMax);
        if (refill)
            CurrentHealth = maxHealth;
        else
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);
        
        OnHealthChanged?.Invoke(this);
    }

    public void MakeInvulnerableTemporarily(float duration)
    {
        IsInvulnerable = true;
        StartCoroutine(MakeVulnerable(duration));
    }

    IEnumerator MakeVulnerable(float afterThisLong)
    {
        yield return new WaitForSeconds(afterThisLong);
        IsInvulnerable = false;
    }

    public void TakeDamageOverTime(float damagePerTick, float ticksPerSecond, float duration, string statusEffectName)
    {
        Coroutine statusEffectDOT = statusEffectName switch
        {
            "burn" => burningRoutine,
            "poison" => poisonedRoutine,
            _ => null
        };
        
        var ticksLeft = ticksPerSecond * duration;
        
        if (statusEffectName == null || statusEffectName == "null")
        {
            StartCoroutine(RepeatDamageEveryTick(damagePerTick, ticksPerSecond, ticksLeft));
        }
        else
        {
            if (statusEffectDOT != null) 
            {
                StopCoroutine(statusEffectDOT);
            }

            var newCoroutine = StartCoroutine(RepeatDamageEveryTick(damagePerTick, ticksPerSecond, ticksLeft));

            switch (statusEffectName)
            {
                case "burn": burningRoutine = newCoroutine; break;
                case "poison": poisonedRoutine = newCoroutine; break;
            }
        }
    }

    IEnumerator RepeatDamageEveryTick(float damagePerTick, float ticksPerSecond, float ticksLeft)
    {   
        if (ticksLeft > 0f)
        {
            TakeDamage(damagePerTick);
            ticksLeft -= 1f;
            
            yield return new WaitForSeconds(1/ticksPerSecond);
            StartCoroutine(RepeatDamageEveryTick(damagePerTick, ticksPerSecond, ticksLeft));
        }
    }
}
