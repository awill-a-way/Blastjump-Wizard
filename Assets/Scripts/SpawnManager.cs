using Unity.VisualScripting;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] public Transform defaultSpawnPoint;
    [SerializeField] private float respawnDelay = 1.5f;

    [SerializeField] private SpawnPoint _activeSpawnPoint;
    public SpawnPoint ActiveSpawnPoint => _activeSpawnPoint;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    public Vector3 RespawnPosition => _activeSpawnPoint != null
        ? _activeSpawnPoint.SpawnPosition
        : defaultSpawnPoint.position;

    public Quaternion RespawnRotation => _activeSpawnPoint != null
        ? _activeSpawnPoint.SpawnRotation
        : defaultSpawnPoint.rotation;

    //
    public void SetActiveSpawnPoint(SpawnPoint newSpawnPoint)
    {
        if (newSpawnPoint == _activeSpawnPoint) return;
        if (!newSpawnPoint.TryActivate()) return; // respects onlyActivateOnce

        _activeSpawnPoint?.OnDeactivated();
        _activeSpawnPoint = newSpawnPoint;

        Debug.Log($"Checkpoint set: {newSpawnPoint.name}");
    }
}
