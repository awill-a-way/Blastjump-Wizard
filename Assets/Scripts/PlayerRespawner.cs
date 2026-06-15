using System;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(Player))] // swap for Rigidbody if needed
public class PlayerRespawner : MonoBehaviour
{
    [SerializeField] private GameObject deathEffectPrefab;

    private HealthSystem _health;
    private Player _player;
    private SpawnManager _spawnManager;
    public UnityEvent OnPlayerRespawned;

    private void Awake()
    {
        _health = GetComponent<HealthSystem>();
        _player = GetComponent<Player>();
    }

    private void Start()
    {
        // All Awakes have run by now, so Instance is guaranteed to be set
        _spawnManager = SpawnManager.Instance;
        if (_spawnManager == null)
            Debug.LogError("SpawnManager not found in scene!");
        if (_spawnManager.defaultSpawnPoint == null)
        {
            var PlayerTransformAtStart = _player.transform;
            _spawnManager.defaultSpawnPoint = PlayerTransformAtStart;
        }
    }

    private void OnEnable()
    {
        _health.OnDied.AddListener(OnPlayerDied);
    }

    private void OnDisable()
    {
        _health.OnDied.RemoveListener(OnPlayerDied);
    }

    private void OnPlayerDied()
    {
        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        // Disable player input/movement here
        // e.g. GetComponent<PlayerController>().enabled = false;

        RespawnPlayer();
        _health.ResetDeadState();
        _health.RestoreToFull();
        // Reset IsDead so the player can take damage again
        // You need to expose this — see HealthSystem fix below
    }
    public void RespawnPlayer()
    {
        Debug.Log($"_spawnManager: {_spawnManager}, _activeSpawnPoint: {_spawnManager?.ActiveSpawnPoint}");
        
        var spawnPos = SpawnManager.Instance.ActiveSpawnPoint != null
            ? SpawnManager.Instance.ActiveSpawnPoint.transform.position
            : SpawnManager.Instance.defaultSpawnPoint.position;
        
        if (_player != null)
        {
            if (_spawnManager.ActiveSpawnPoint != null)
            {
                _player.Teleport(_spawnManager.ActiveSpawnPoint.transform.position);
            }
            else if (_spawnManager.ActiveSpawnPoint == null)
            {
                _player.Teleport(_spawnManager.defaultSpawnPoint.position);
            }
        }

        
        OnPlayerRespawned?.Invoke();
    }

    void Update()
    {
        #if UNITY_EDITOR
        if (UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
        {
            _health.Kill();
        }
        if (UnityEngine.InputSystem.Keyboard.current.hKey.wasPressedThisFrame)
        {
            _health.RestoreToFull();
        }
        #endif
    }
}