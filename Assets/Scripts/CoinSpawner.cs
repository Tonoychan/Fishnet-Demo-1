using FishNet;
using FishNet.Object;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinCount = 10;
    [SerializeField] private float spawnRadius = 8f;

    private void OnEnable()
    {
        // Subscribe to FishNet's ServerManager event
        // This fires EXACTLY when the server finishes starting
        // Much more reliable than checking in Start()
        InstanceFinder.ServerManager.OnServerConnectionState += OnServerConnectionState;
    }

    private void OnDisable()
    {
        // Always unsubscribe to avoid memory leaks or ghost calls
        if (InstanceFinder.ServerManager != null)
            InstanceFinder.ServerManager.OnServerConnectionState -= OnServerConnectionState;
    }

    private void OnServerConnectionState(FishNet.Transporting.ServerConnectionStateArgs args)
    {
        // FishNet fires this event for multiple state changes
        // We only want to spawn when the server has fully STARTED
        // LocalConnectionState.Started = server is fully up and ready
        if (args.ConnectionState == FishNet.Transporting.LocalConnectionState.Started)
        {
            SpawnCoins();
        }
    }

    private void SpawnCoins()
    {
        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0.5f,
                Random.Range(-spawnRadius, spawnRadius)
            );

            GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            InstanceFinder.ServerManager.Spawn(coin);
        }
    }
}