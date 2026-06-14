using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;
    [Space]
    [SerializeField] private Image manaFill;
    [SerializeField] private TMP_Text manaText;
    [Space]
    [SerializeField] private TMP_Text spellInputText;
    //private HealthSystem health;
    //private SpellShooter mana;
    //private UnityEvent OnPlayerHealthChanged;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //health = GetComponentInParent<HealthSystem>();
        //mana = GetComponentInParent<SpellShooter>();
    }

    public void UpdateHealthUI(float current, float max)
    {
        healthFill.fillAmount = current / max;
        healthText.text = $"{Mathf.CeilToInt(current)}";
    }

    public void UpdateManaUI(float current, float max)
    {
        manaFill.fillAmount = current / max;
        manaText.text = $"{Mathf.CeilToInt(current)}";
    }

    public void UpdateSpellInputUI(string spellInput, string currentSpell)
    {
        if (spellInput == "N/A")
        {
            spellInputText.text = "";
        }
        else
        {
            spellInputText.text = $"{spellInput}";
        }
    }
}
