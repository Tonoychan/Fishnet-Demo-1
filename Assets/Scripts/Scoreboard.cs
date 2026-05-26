using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    [SerializeField] 
    private GameObject scoreRowPrefab;
    
    [SerializeField]
    private Transform rowContainer;
    
    private Dictionary<int, GameObject> _rows = new Dictionary<int, GameObject>() ;
    private Dictionary<int, TextMeshProUGUI> _scoreTexts = new Dictionary<int, TextMeshProUGUI>();
    private Dictionary<int, Image> _colorDots = new Dictionary<int, Image>();

    private float _refreshTimer = 0f;

    private float _refreshRate = 0.5f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _refreshTimer -= Time.deltaTime;
        if (_refreshTimer <= 0f)
        {
            _refreshTimer = _refreshRate;
            RefreshPlayerList();
        }

        UpdateScores();
    }

    private void RefreshPlayerList()
    {
        // Find all PlayerControllers currently in the scene
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        // Add rows for new players
        foreach (PlayerController player in players)
        {
            int id = player.GameOwnerID;

            if (!_rows.ContainsKey(id))
                AddRow(player);
        }

        // Remove rows for players who left
        List<int> toRemove = new List<int>();
        foreach (int id in _rows.Keys)
        {
            bool stillExists = false;
            foreach (PlayerController p in players)
            {
                if (p.GameOwnerID == id)
                {
                    stillExists = true;
                    break;
                }
            }
            if (!stillExists)
                toRemove.Add(id);
        }

        foreach (int id in toRemove)
            RemoveRow(id);
    }
    
    private void AddRow(PlayerController player)
    {
        int id = player.GameOwnerID;

        // Instantiate a new row under the container
        GameObject row = Instantiate(scoreRowPrefab, rowContainer);
        _rows[id] = row;

        // Grab references to the color dot and score text
        Image dot = row.GetComponentInChildren<Image>();
        TextMeshProUGUI text = row.GetComponentInChildren<TextMeshProUGUI>();

        _colorDots[id] = dot;
        _scoreTexts[id] = text;

        // Set the dot color to match the player's material
        MeshRenderer playerRenderer = player.GetComponentInChildren<MeshRenderer>();
        if (playerRenderer != null)
            dot.color = playerRenderer.material.color;

        // Set initial score text
        text.text = $"Player {id}: {player.Score}";
    }
    
    private void RemoveRow(int id)
    {
        if (_rows.ContainsKey(id))
        {
            Destroy(_rows[id]);
            _rows.Remove(id);
            _scoreTexts.Remove(id);
            _colorDots.Remove(id);
        }
    }

    private void UpdateScores()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController player in players)
        {
            int id = player.GameOwnerID;

            if (_scoreTexts.ContainsKey(id))
                _scoreTexts[id].text = $"Player {id}: {player.Score}";
        }
    }
}
