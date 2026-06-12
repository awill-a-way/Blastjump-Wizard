using Unity.VisualScripting;
using UnityEngine;

public class FloatingDisc : MonoBehaviour
{
    private PlayerInputActions _inputActions;
    public float floatingDiscCount;
    private float floatingDiscID;
    [Range(0,50)]
    [SerializeField] private float lifespan = 15f;

    void Awake()
    {
        
    }

    void Start()
    {
        floatingDiscCount += 1;
        floatingDiscID = floatingDiscCount;

        _inputActions = new PlayerInputActions();
        _inputActions.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        var input = _inputActions.Gameplay;
        lifespan -= Time.deltaTime;
        if (input.Fire2.WasPressedThisFrame())
        {
            floatingDiscID -= 1f;
        }
        if (lifespan <= 0 || floatingDiscID <= 0)
        {
            Destroy(gameObject);
        }

        //var pos = gameObject.transform.position;
        //pos.y -= Time.deltaTime;
    }

    void OnDestroy()
    {
        _inputActions.Dispose();
        floatingDiscCount -= 1;
    }
}
