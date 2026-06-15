using System;
using Unity.VisualScripting;
using UnityEngine;

public class LazySusan : MonoBehaviour
{
    private SpellShooter spellShooter;
    private float speedMultiplier;
    [Range(0,100)]
    [SerializeField] private float maxSpeedMultiplier = 100f;
    [Range(0,100)]
    [SerializeField] private float minSpeedMultiplier = 0f;
    [SerializeField] private bool spinClockwise = true;
    [Space]
    public bool spinEnabled = false;
    [SerializeField] private bool spinWithTime = false;
    [SerializeField] private bool spinWithPosition = false;
    [SerializeField] private bool spinWithPlayer = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spellShooter = GetComponentInParent<SpellShooter>();
    }

    // Update is called once per frame
    void Update()
    {
        if (spinEnabled == true)
        {
            if (spinClockwise == true)
            {
                speedMultiplier = -1f*Mathf.Lerp(minSpeedMultiplier, maxSpeedMultiplier, spellShooter.spellCharge);
            }
            else
            {
                speedMultiplier = Mathf.Lerp(minSpeedMultiplier, maxSpeedMultiplier, spellShooter.spellCharge);
            }
            
            if (spinWithTime == true)
            {
                gameObject.transform.Rotate(Vector3.up * speedMultiplier * Time.deltaTime);
            }
            
            if (spinWithPlayer == true)
            {
                gameObject.transform.rotation = new Quaternion(Time.deltaTime, Time.deltaTime, Time.deltaTime, Time.deltaTime);
            }
        }
    }
}
