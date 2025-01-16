using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    
    public List<Transform> SpawnPositions;
    public static GameManager Instance { get; set; }

    [SerializeField] private ProjectileController _projectilePrefab;
    
    private List<PlayerClientController> _playerClientsOnServer;
    private int _currentNumberOfPlayersConnected;
    private int _latestFreeSpawnIndex;
    
    private const int _maxPlayers = 2;
    private const int waitTimeForClients = 100;
    
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
        if(IsServer)
            SubscribeToEvents();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _playerClientsOnServer.Clear();
        UnsubscribeFromEvents();
    }

    public PlayerClientController GetEnemyPlayerClient(ulong networkObjectID)
    {
        foreach (PlayerClientController playerClientController in _playerClientsOnServer)
        {
            if(playerClientController.NetworkObjectId != networkObjectID)
                return playerClientController;
        }
        return null;
    }

    private void CheckIfAllPlayersHaveConnected(ulong clientId)
    {
        _currentNumberOfPlayersConnected++;
        if (_currentNumberOfPlayersConnected == _maxPlayers)
        {
            _playerClientsOnServer = FindObjectsByType<PlayerClientController>(FindObjectsSortMode.None).ToList();
            InitializePlayersAsync();   
        }
    }

    private async void InitializePlayersAsync()
    {
        await Task.Delay(waitTimeForClients);
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
            playerClients[i].EnablePlayer();
    }
}
