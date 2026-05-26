using FishNet.Object;
using UnityEngine;

public class Coin : NetworkBehaviour
{
    [SerializeField] private float spinSpeed = 90f;
    [SerializeField] private int coinValue = 10;

    // Reference to GameManager to check if game is active
    private GameManager _gameManager;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        // Find the GameManager once when coin spawns
        _gameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized) return;

        // Block collection if game is not actively playing
        if (_gameManager == null || !_gameManager.IsGamePlaying()) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        player.AddScore(coinValue);
        Despawn();
    }
}