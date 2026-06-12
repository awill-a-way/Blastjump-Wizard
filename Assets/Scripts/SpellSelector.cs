using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class SpellSelector : MonoBehaviour
{
    private PlayerInputActions _inputActions;
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




    void Start()
    {   
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

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
            if (input.SpellUp.WasPressedThisFrame()) AddInput("U");
            if (input.SpellDown.WasPressedThisFrame()) AddInput("D");
            if (input.SpellLeft.WasPressedThisFrame()) AddInput("L");
            if (input.SpellRight.WasPressedThisFrame()) AddInput("R");

            if (input.Spellbook.IsPressed() == true || altSpellInputsEnabled == true)
            {
                if (input.AltSpellUp.WasPressedThisFrame()) AddInput("U");
                if (input.AltSpellDown.WasPressedThisFrame()) AddInput("D");
                if (input.AltSpellLeft.WasPressedThisFrame()) AddInput("L");
                if (input.AltSpellRight.WasPressedThisFrame()) AddInput("R");
            }
        }

        if (input.ResetSpell.WasPressedThisFrame()) // reset input buffer and current spell when the "reset spell" button (R) is pressed
        {
            inputBufferResetTimer = 0f;
            currentSpell = "";
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
            inputStunTimer -= Time.deltaTime;
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

    void AddInput(string _input)
    {
        if (enableSpellInput == true)
        {
            inputBuffer.Add(_input);
            inputBufferResetTimer = inputBufferResetTime; // reset the timer whenever a new input is added
            Debug.Log("Input Buffer: " + string.Join(", ", inputBuffer));

            if (currentSpell != "No Spell" && currentSpell != "")
            {
                previousSpell = currentSpell;
            }
            currentSpell = "No Spell";

            CheckSpell();
        }
    }

    void CheckSpell()
    {
        string spell = string.Join("", inputBuffer);

        if(spell == "RRUU")
        {
            SelectSpell("Knockblast");
        }
        else if(spell == "DRRR")
        {
            SelectSpell("Mana Cube");
        }
        else if (spell == "LLRR")
        {
            SelectSpell("Polymorph: Disc");
        }
        else if (spell == "RRDU")
        {
            SelectSpell("Glyph of Volatility");
        }
        else if (spell == "URURUR")
        {
            SelectSpell("Lightning Bolt");
        }
        else if (spell == "RRRR")
        {
            SelectSpell("Ice Shard");
        }
        else if (spell == "URDL")
        {
            SelectSpell("Snowball");
        }
        else if (spell == "LUUR")
        {
            SelectSpell("Heal");
        }
        else if (spell == "LRLR")
        {
            SelectSpell("Shield");
        }
        else if (spell == "RRRL")
        {
            SelectSpell("Vine Grapple");
        }
        else if (spell == "UULR")
        {
            SelectSpell("Psionic Grasp");
        }
        else if (spell.Length >= 6)
        {
            Debug.Log("No spell matched!");
            inputBuffer.Clear();
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
    }
}