using System;
using System.ComponentModel.Design;
using Unity.VisualScripting;
using UnityEngine;

public class BagOfHolding : MonoBehaviour
{
    private PlayerInputActions _inputActions;
    public Camera playerCamera;
    public Transform holdPoint;
    private int numSlots = 5;
    [SerializeField] private int selectedSlot;
    public bool selectedSlotEmpty;
    [SerializeField] private GameObject[] baggedObjects = new GameObject[5];
    private float digestDamage = 0.1f;



    void Start()
    {
        baggedObjects = new GameObject[numSlots];
        selectedSlot = 0;

        _inputActions = new PlayerInputActions();
    }

    void Update()
    {   
        var input = _inputActions.Gameplay;

        if (baggedObjects[selectedSlot] == null) selectedSlotEmpty = true;
        else if (baggedObjects[selectedSlot] != null) selectedSlotEmpty = false;

        Digest();
    }

    public void SelectSlot(int selectSlotNum)
    {
        selectedSlot = selectSlotNum;
    }

    public void Grab(GameObject target)
    {
        if (target.TryGetComponent<Baggable>(out var baggable) && baggable.bagged == false && baggable.canBeGrappled == true)
        {
            CheckSlots(target, baggable, selectedSlot);
        }
        else
        {
            Debug.Log("Couldn't bag "+target+"!");
        }
    }

    private void CheckSlots(GameObject target, Baggable baggable, int slotNum)
    {
        if (slotNum <= numSlots-1)
        {
            if (baggedObjects[slotNum] == null)
            {
                baggedObjects[slotNum] = target;
                Debug.Log("Stored "+target+" in bag slot " +slotNum+"!");

                Pocket(target, baggable);
            }
            else
            {
                Debug.Log("Checked slots and couldn't pocket "+target+"!");
            }
        }
    }

    private void Pocket(GameObject baggedObject, Baggable baggable)
    {
        baggedObject.transform.rotation = Quaternion.identity;
        baggable.bagged = true;

        if (baggable.mr != null) baggable.mr.enabled = false;
        if (baggable.c != null) baggable.c.enabled = false;
        if (baggable.rb != null)
        {
            baggable.rb.linearVelocity = Vector3.zero;
            baggable.rb.useGravity = false;
            baggable.rb.isKinematic = true;
        }
            
        baggedObject.transform.SetParent(holdPoint, true);
        baggedObject.transform.localPosition = Vector3.zero;
    }
    
    public void Chuck(Vector3 playerVelocity)
    {
        var cachedSelectNum = selectedSlot;
        GameObject chuckedObject = baggedObjects[cachedSelectNum];
        if (chuckedObject != null)
        {
            Vector3 chuckDirection;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            chuckDirection = ray.GetPoint(100f).normalized;

            Release(cachedSelectNum);
            chuckedObject.transform.position = ray.GetPoint(2f);
            if (chuckedObject.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = 10f*chuckDirection+playerVelocity;
            }
        }
    }

    private void Release(int slotNum)
    {
        if (baggedObjects[slotNum] != null)
        {
            baggedObjects[slotNum].transform.SetParent(null, true);

            var baggable = baggedObjects[slotNum].GetComponent<Baggable>();
            baggable.bagged = false;
            
            if (baggable.mr != null) baggable.mr.enabled = true;
            if (baggable.c != null) baggable.c.enabled = true;
            if (baggable.rb != null)
            {
                baggable.rb.useGravity = baggable._originalRigidbodyUseGravity;
                baggable.rb.isKinematic = baggable._originalRigidbodyIsKinematic;
            }

            baggable.transform.rotation = playerCamera.transform.rotation;
            baggable.ResetCanBeGrappled(0f);
            Debug.Log("Released "+baggedObjects[slotNum]+"!");

            baggedObjects[slotNum] = null;
        }
    }

    private void Digest()
    {
        for (int i = 0; i < baggedObjects.Length; i++)
        {
            if (baggedObjects[i] == null) continue;

            if (baggedObjects[i].TryGetComponent<HealthSystem>(out var health))
            {
                if (health.IsDead == false)
                {
                    health.TakeDamage(digestDamage*health.MaxHealth);
                }
                else if (health.IsDead == true)
                {
                    Release(i);
                }
            }
        }
    }
}
