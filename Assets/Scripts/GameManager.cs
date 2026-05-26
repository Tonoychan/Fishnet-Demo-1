using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using FishNet.Managing.Server;
using UnityEngine;
using TMPro;

public class GameManager : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private int minPlayersToStart = 2;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI announcementText;

    // 0 = waiting for players, 1 = playing, 2 = game over
    private readonly SyncVar<int> _gameState = new SyncVar<int>();
    private readonly SyncVar<float> _timeRemaining = new SyncVar<float>();
    private readonly SyncVar<int> _connectedPlayers = new SyncVar<int>();

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        _gameState.OnChange += OnGameStateChanged;
        _timeRemaining.OnChange += OnTimeChanged;
        _connectedPlayers.OnChange += OnConnectedPlayersChanged;

        if (announcementText != null)
        {
            announcementText.gameObject.SetActive(true);
            announcementText.text = "Waiting for players...";
        }

        // Show initial timer
        if (timerText != null)
            timerText.text = "01:00";
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        _gameState.OnChange -= OnGameStateChanged;
        _timeRemaining.OnChange -= OnTimeChanged;
        _connectedPlayers.OnChange -= OnConnectedPlayersChanged;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _timeRemaining.Value = gameDuration;
        _gameState.Value = 0;
        _connectedPlayers.Value = 0;

        // Listen for players connecting and disconnecting
        ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
    }

    // Fires on server whenever any remote client connects or disconnects
    private void OnRemoteConnectionState(NetworkConnection conn,
        FishNet.Transporting.RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == FishNet.Transporting.RemoteConnectionState.Started)
        {
            // A new client connected
            _connectedPlayers.Value++;
            Debug.Log($"Player connected. Total: {_connectedPlayers.Value}");

            // Auto start game when minimum players reached
            if (_connectedPlayers.Value >= minPlayersToStart
                && _gameState.Value == 0)
            {
                // Small delay so player finishes spawning before game starts
                StartCoroutine(StartGameWithDelay(2f));
            }
        }
        else if (args.ConnectionState == FishNet.Transporting.RemoteConnectionState.Stopped)
        {
            // A client disconnected
            _connectedPlayers.Value--;
            Debug.Log($"Player disconnected. Total: {_connectedPlayers.Value}");

            // If not enough players during a game — pause it
            if (_connectedPlayers.Value < minPlayersToStart 
                && _gameState.Value == 1)
            {
                StopAllCoroutines();
                _gameState.Value = 0;
                AnnounceRpc("A player left.\nWaiting for players...");
            }
        }
    }

    private IEnumerator StartGameWithDelay(float delay)
    {
        // Show countdown to players
        AnnounceRpc("Game starting in 3...");
        yield return new WaitForSeconds(1f);
        AnnounceRpc("Game starting in 2...");
        yield return new WaitForSeconds(1f);
        AnnounceRpc("Game starting in 1...");
        yield return new WaitForSeconds(1f);

        StartGame();
    }

    [Server]
    private void StartGame()
    {
        if (_gameState.Value == 1) return;

        _gameState.Value = 1;
        _timeRemaining.Value = gameDuration;

        // Reset all scores when game starts fresh
        PlayerController[] players =
            FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController p in players)
            p.ResetScore();

        HideAnnouncementRpc();
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        while (_timeRemaining.Value > 0f)
        {
            yield return new WaitForSeconds(1f);
            _timeRemaining.Value -= 1f;
        }

        _timeRemaining.Value = 0f;
        _gameState.Value = 2;
        DetermineWinner();
    }

    [Server]
    private void DetermineWinner()
    {
        PlayerController[] players =
            FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        if (players.Length == 0)
        {
            AnnounceRpc("No players!");
            return;
        }

        PlayerController winner = players[0];
        bool isTie = false;

        foreach (PlayerController player in players)
        {
            if (player.Score > winner.Score)
            {
                winner = player;
                isTie = false;
            }
            else if (player.Score == winner.Score && player != winner)
            {
                isTie = true;
            }
        }

        string message = isTie
            ? $"IT'S A TIE!\nBoth scored {winner.Score} points!"
            : $"PLAYER {winner.GameOwnerID} WINS!\nScore: {winner.Score} points!";

        AnnounceRpc(message);
        StartCoroutine(ResetAfterDelay(5f));
    }

    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Only reset if we still have enough players
        if (_connectedPlayers.Value >= minPlayersToStart)
            StartGame();
        else
        {
            _gameState.Value = 0;
            AnnounceRpc("Waiting for players...");
        }
    }

    // -------------------------------------------------------
    // Coin collection gate — called by Coin.cs
    // Returns true only if game is currently playing
    // -------------------------------------------------------
    public bool IsGamePlaying()
    {
        return _gameState.Value == 1;
    }

    // -------------------------------------------------------
    // RPCs
    // -------------------------------------------------------
    [ObserversRpc]
    private void AnnounceRpc(string message)
    {
        if (announcementText == null) return;
        announcementText.gameObject.SetActive(true);
        announcementText.text = message;
    }

    [ObserversRpc]
    private void HideAnnouncementRpc()
    {
        if (announcementText == null) return;
        announcementText.gameObject.SetActive(false);
    }

    // -------------------------------------------------------
    // SyncVar callbacks
    // -------------------------------------------------------
    private void OnTimeChanged(float prev, float next, bool asServer)
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(next);
        int mins = seconds / 60;
        int secs = seconds % 60;
        timerText.text = $"{mins:00}:{secs:00}";
        timerText.color = next <= 10f ? Color.red : Color.white;
    }

    private void OnGameStateChanged(int prev, int next, bool asServer)
    {
        Debug.Log($"Game state: {prev} → {next}");
    }

    private void OnConnectedPlayersChanged(int prev, int next, bool asServer)
    {
        Debug.Log($"Connected players: {next}");

        // Update waiting message with player count
        if (_gameState.Value == 0)
            AnnounceRpc($"Waiting for players...\n{next}/{minPlayersToStart}");
    }
}