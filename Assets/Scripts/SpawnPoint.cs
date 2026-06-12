using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool onlyActivateOnce = true;

    public bool hasBeenActivated { get; private set; }
    public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;
    public Quaternion SpawnRotation => spawnPoint != null ? spawnPoint.rotation : transform.rotation;

    // No more OnTriggerEnter

    public bool TryActivate()
    {
        if (onlyActivateOnce && hasBeenActivated) return false;
        hasBeenActivated = true;
        OnActivated();
        return true;
    }

    public void OnActivated() { }
    public void OnDeactivated() { }
}