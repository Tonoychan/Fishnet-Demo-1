using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float smoothSpeed = 15f;

    private Vector3 _targetPosition;
    private Vector3 _serverInput = Vector3.zero;
    
    [SerializeField] private Material[] playerMaterials;
    
    private readonly SyncVar<int> _colorIndex =  new SyncVar<int>();
    private readonly SyncVar<int> _score = new SyncVar<int>();
    
    public int Score => _score.Value;
    public int GameOwnerID => OwnerId;
    
    private MeshRenderer _renderer;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _targetPosition = transform.position;
        _renderer = GetComponent<MeshRenderer>();
        TimeManager.OnTick += OnTick;
        
        _colorIndex.OnChange += OnColorChange;
        _score.OnChange += OnScoreChanged;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if (TimeManager != null)
            TimeManager.OnTick -= OnTick;
        
        _colorIndex.OnChange -= OnColorChange;
        _score.OnChange -= OnScoreChanged;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        _colorIndex.Value = OwnerId % playerMaterials.Length;
        _score.Value = 0;
    }

    private void OnTick()
    {
        if (IsOwner)
        {
            // Read input and send to server every tick
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector3 input = new Vector3(h, 0f, v).normalized;
            SendInputServerRpc(input);
        }

        if (IsServerInitialized)
        {
            // Server moves the object using latest input at fixed tick rate
            transform.position += _serverInput * moveSpeed * (float)TimeManager.TickDelta;

            // Tell ALL clients (including owner) the authoritative position
            SyncPositionToAllRpc(transform.position);
        }
    }

    [ServerRpc]
    private void SendInputServerRpc(Vector3 input)
    {
        _serverInput = input;
    }

    // No ExcludeOwner this time — owner ALSO needs to know server position
    [ObserversRpc]
    private void SyncPositionToAllRpc(Vector3 serverPos)
    {
        // Owner: smoothly correct toward server position
        // Others: smoothly follow server position
        _targetPosition = serverPos;
    }

    void Update()
    {
        // Everyone lerps toward the server's authoritative position
        // This means owner gets corrected if they drift
        // Remote players get smooth interpolation between ticks
        transform.position = Vector3.Lerp(
            transform.position,
            _targetPosition,
            Time.deltaTime * smoothSpeed
        );
    }
    
    // SyncVar calls this automatically on EVERY client when _colorIndex changes
    // prev = old value, next = new value, asServer = did this run on server?
    private void OnColorChange(int prev, int next, bool asServer)
    {
        if (_renderer == null)
            _renderer = GetComponentInChildren<MeshRenderer>();

        if (playerMaterials != null && next < playerMaterials.Length)
            _renderer.material = playerMaterials[next];
    }

    [Server]
    public void AddScore(int score)
    {
        _score.Value += score;
    }
    
    // Fires on ALL clients automatically when _score.Value changes on server
    private void OnScoreChanged(int prev, int next, bool asServer)
    {
        // For now just log it — Step 4 will show this in proper UI
        Debug.Log($"Player {GameOwnerID} score changed: {prev} → {next}");

        // If this is OUR player, show it on screen
        if (IsOwner)
            Debug.Log($"YOUR SCORE: {next}");
    }
    
    [Server]
    public void ResetScore()
    {
        _score.Value = 0;
    }
}