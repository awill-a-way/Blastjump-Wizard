using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class SpellSelector : MonoBehaviour
{
    private PlayerInputActions _inputActions;
    private HUDController HUD;
    //private SpellShooting _shootSpells;

    public List<string> inputBuffer = new List<string>();
    [Space]
    private float inputBufferResetTimer;
    [SerializeField] private float inputBufferResetTime = 1f;
    [Space]
    private bool enableSpellInput = true;
    public float inputStunTimer;
    [Range(0f,2f)]
    [SerializeField] public float inputStunTime = 0.5f;
    [Space]
    public float spellResetTimer = 1f;
    [Range(0.1f,1)] // A value of 1 essentially disables the gameslow mechanic
    [SerializeField] private float gameSpeedDuringSpellInput = 0.5f;
    [Space]
    public string currentSpell = "";
    [SerializeField] private string previousSpell = "No Spell";
    [SerializeField] private bool prevSpellEnabled;
    [SerializeField] private bool altSpellInputsEnabled;
    [Space]
    [SerializeField] private GameObject knockblastHandVisual;
    [SerializeField] private GameObject manaCubeHandVisual;
    [SerializeField] private GameObject discHandVisual;




    void Start()
    {   
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        HUD = GetComponentInChildren<HUDController>();
        //_shootSpells = GetComponent<SpellShooting>();
    }

    void OnDestroy()
    {
        _inputActions.Dispose();
    }
    
    void Update()
    {        
        var input = _inputActions.Gameplay;

        if (enableSpellInput == true)
        {
            if (input.SpellUp.WasPressedThisFrame()) AddRune("U");
            if (input.SpellDown.WasPressedThisFrame()) AddRune("D");
            if (input.SpellLeft.WasPressedThisFrame()) AddRune("L");
            if (input.SpellRight.WasPressedThisFrame()) AddRune("R");

            if (input.Spellbook.IsPressed() == true || altSpellInputsEnabled == true)
            {
                if (input.AltSpellUp.WasPressedThisFrame()) AddRune("U");
                if (input.AltSpellDown.WasPressedThisFrame()) AddRune("D");
                if (input.AltSpellLeft.WasPressedThisFrame()) AddRune("L");
                if (input.AltSpellRight.WasPressedThisFrame()) AddRune("R");
            }
        }

        if (input.ResetSpell.WasPressedThisFrame()) // reset input buffer and current spell when the "reset spell" button (R) is pressed
        {
            inputBufferResetTimer = 0f;
            currentSpell = "";
            HUD.UpdateSpellInputUI(string.Join(" ", inputBuffer), currentSpell);
            UpdateSpellHandVisual();
        }

        //Reset spell if you havent casted in a little bit  (currently removed)
        if (spellResetTimer > 0f)
        {
            //spellResetTimer -= Time.deltaTime;
            //Debug.Log("Current Spell: " + currentSpell + " Time Remaining: " + spellResetTimer);
            //May add back
        }

        if (inputStunTimer > 0f)
        {
            inputStunTimer -= Time.deltaTime/Time.timeScale;
            enableSpellInput = false;
            //Debug.Log("Spell Inputs Disabled! Time Remaining:" + spellResetTimer);
        }
        else
        {
            enableSpellInput = true;
        }
        
        if (currentSpell == "")
        {
            //Debug.Log("No spell selected");
            currentSpell = "No Spell";
        }



        if (inputBufferResetTimer > 0f && Time.timeScale != 0f) inputBufferResetTimer -= Time.deltaTime/Time.timeScale;
    
        if (inputBufferResetTimer <= 0f && inputBuffer.Count > 0)
        {
            inputBuffer.Clear();
            HUD.UpdateSpellInputUI(string.Join(" ", inputBuffer), currentSpell);
            UpdateSpellHandVisual();
            Debug.Log("Input Timer reached zero, Input Buffer Cleared");
        }

        if (inputBuffer.Count > 0 || input.Spellbook.IsPressed() == true)
        {
            Time.timeScale = gameSpeedDuringSpellInput;
        }
        else
        {
            Time.timeScale = 1;
        }

        if (input.PrevSpell.WasPressedThisFrame() == true && prevSpellEnabled == true)
        {
            SelectSpell(previousSpell);
            Debug.Log("Swapped to previous spell!");
        }
    }

    void AddRune(string _input)
    {
        if (enableSpellInput == true)
        {
            inputBuffer.Add(_input);
            inputBufferResetTimer = inputBufferResetTime; // reset the timer whenever a new input is added
            
            HUD.UpdateSpellInputUI(string.Join(" ", inputBuffer), currentSpell);
            Debug.Log("Input Buffer: " + string.Join(", ", inputBuffer));

            if (currentSpell != "No Spell" && currentSpell != "")
            {
                previousSpell = currentSpell;
            }
            currentSpell = "No Spell";

            CheckSpell();
            UpdateSpellHandVisual();
        }
    }

    void CheckSpell()
    {
        string runes = string.Join("", inputBuffer);

        if(runes == "RRUU")
        {
            SelectSpell("Knockblast");
        }
        else if(runes == "DRRR")
        {
            SelectSpell("Mana Cube");
        }
        else if (runes == "LLRR")
        {
            SelectSpell("Polymorph: Disc");
        }
        else if (runes == "RRDU")
        {
            SelectSpell("Glyph of Volatility");
        }
        else if (runes == "URURUR")
        {
            SelectSpell("Lightning Bolt");
        }
        else if (runes == "RRRR")
        {
            SelectSpell("Ice Shard");
        }
        else if (runes == "URDL")
        {
            SelectSpell("Snowball");
        }
        else if (runes == "LUUR")
        {
            SelectSpell("Heal");
        }
        else if (runes == "LRLR")
        {
            SelectSpell("Shield");
        }
        else if (runes == "RRRL")
        {
            SelectSpell("Vine Grapple");
        }
        else if (runes == "UULR")
        {
            SelectSpell("Psionic Grasp");
        }
        else if (runes.Length >= 6)
        {
            SelectSpell("No Spell");
            Debug.Log("No spell matched!");
        }
    }

    void SelectSpell(string spellName)
    {
        if (currentSpell != "No Spell" && currentSpell != "")
        {
            previousSpell = currentSpell;
        }
        
        currentSpell = spellName;
        Debug.Log(currentSpell+" Selected! Previous spell was "+previousSpell);

        inputBuffer.Clear();
        inputStunTimer = inputStunTime;
        //spellResetTimer = 10f;

        UpdateSpellHandVisual();

        StartCoroutine(RuneTextLinger(0.5f));
    }

    void UpdateSpellHandVisual()
    {
        GameObject spellHandVisual = currentSpell switch
        {
            "Knockblast" => knockblastHandVisual,
            "Mana Cube" => manaCubeHandVisual,
            "Polymorph: Disc" => discHandVisual,
            _ => null
        };

        //Disable whatever is active first
        DisableSpellHandVisual(knockblastHandVisual);
        DisableSpellHandVisual(manaCubeHandVisual);
        DisableSpellHandVisual(discHandVisual);

        //Then activate the right visual
        EnableSpellHandVisual(spellHandVisual);
    }

    void EnableSpellHandVisual(GameObject spellHandVisual)
    {
        if (spellHandVisual != null)
        {
            if (spellHandVisual.TryGetComponent<MeshRenderer>(out var mr) && mr.enabled == false) mr.enabled = true;
        }
    }

    void DisableSpellHandVisual(GameObject spellHandVisual)
    {
        if (spellHandVisual != null)
        {
            if (spellHandVisual.TryGetComponent<MeshRenderer>(out var mr) && mr.enabled == true) mr.enabled = false;
        }
    }
    
    IEnumerator RuneTextLinger(float duration)
    {
        yield return new WaitForSeconds(duration);

        string runes = string.Join("", inputBuffer);
        if (runes.Length <= 0f)
        {
            HUD.UpdateSpellInputUI(string.Join(" ", inputBuffer), currentSpell);
        }
    }
}