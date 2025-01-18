using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    
    public List<Transform> SpawnPositions;
    [Range(2, 4)]
    public int _maxPlayers;
    public static GameManager Instance { get; set; }

    [SerializeField] private ProjectileController _projectilePrefab;
    [SerializeField] private GameObject _statusPanel;
    [SerializeField] private TextMeshProUGUI _statusText;
    
    private List<PlayerClientController> _playerClientsOnServer;
    private int _currentNumberOfPlayersConnected;
    private int _latestFreeSpawnIndex;
    private const int _waitTimeForClients = 500;
    private const string _waitingMessage = "WAITING FOR PLAYERS";
    
    public EventService EventService;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this);
        
        EventService = new EventService();
        _playerClientsOnServer = new List<PlayerClientController>();
    }
    
    private void SubscribeToEvents()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += CheckIfAllPlayersHaveConnected;
    }
    
    private void UnsubscribeFromEvents()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= CheckIfAllPlayersHaveConnected;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
            SubscribeToEvents();
        UpdateUIStatusRpc(GameStatus.WAITING);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _playerClientsOnServer.Clear();
        UnsubscribeFromEvents();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateUIStatusRpc(GameStatus gameStatus)
    {
        string text = String.Empty;
        _statusPanel.SetActive(true);
        switch (gameStatus)
        {
            case GameStatus.WAITING:
                text = _waitingMessage;
                break;
            case GameStatus.STARTED:
                _statusPanel.SetActive(false);
                break;
        }
        _statusText.text = text;
    }
    private void CheckIfAllPlayersHaveConnected(ulong clientId)
    {
        _currentNumberOfPlayersConnected++;
        if (_currentNumberOfPlayersConnected == _maxPlayers)
        {
            _playerClientsOnServer = FindObjectsByType<PlayerClientController>(FindObjectsSortMode.None).ToList();
            InitializePlayersAsync(); 
            UpdateUIStatusRpc(GameStatus.STARTED);
        }
    }

    private async void InitializePlayersAsync()
    {
        await Task.Delay(_waitTimeForClients);
        EnableAllClientsRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void EnableAllClientsRpc()
    {
        List<PlayerClientController> playerClients = new List<PlayerClientController>();

        if (IsServer)
            playerClients = _playerClientsOnServer;
        else
            playerClients = FindObjectsByType<PlayerClientController>(FindObjectsSortMode.None).ToList();

        for (int i = 0; i < playerClients.Count; i++)
        {
            playerClients[i].SpawnPosition = SpawnPositions[_latestFreeSpawnIndex++].position;
            playerClients[i].EnablePlayer();  
        }
    }
}
