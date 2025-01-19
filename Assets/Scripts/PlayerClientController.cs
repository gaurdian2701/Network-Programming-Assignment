using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerClientController : NetworkBehaviour
{
    [SerializeField] private Transform _aimTransform;
    [Range(3, 20)] [SerializeField] private float _movementSpeed;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private ProjectileController _projectilePrefab;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private TextMeshProUGUI _emoteText;

    private Vector3 _aimDirection;
    private HealthSystem _playerHealth;
    private bool _isUpdated;
    private bool _isAlive = true;
    
    public Vector3 SpawnPosition;

    private const float _aimOffsetRadius = 1.28f;
    private const float _aimDeadzoneValue = 18f;
    private string[] _aliveEmotes = new string[]
    {
        "COME AT ME",
        "I AM GONNA BEAT YOU",
        "SHOW ME WHAT YA GOT"
    };

    private string[] _deadEmotes = new string[]
    {
        "I AM ALREADY DEAD, STUPID",
        "YOU CAN'T KILL A DEAD CUBE",
        "THE GAME IS OVER"
    };

    private void Awake()
    {
        _characterController.enabled = false;
        _isUpdated = false;
    }

    public override void OnNetworkSpawn()
    {
        _playerHealth = new HealthSystem(NetworkObjectId);
        UpdateHealthUI(_playerHealth.CurrentHealth);
        GameManager.Instance.EventService.OnPlayerDiedEvent += OnPlayerDeath;
    }

    private void OnDestroy()
    {
        GameManager.Instance.EventService.OnPlayerDiedEvent -= OnPlayerDeath;
    }
    public void EnablePlayer()
    {
        if (!IsLocalPlayer)
        {
            _isUpdated = false;
            _characterController.enabled = true;
            enabled = false;
        }
        else
        {
            GoToSpawnPoint();
            _isUpdated = true;
            _characterController.enabled = true;
        }
    }

    private void Update()
    {
        DoEmote();
        if (!_isUpdated)
            return;

        AimGun();
        ShootProjectile();
        DoPlayerMovement();
    }

    private void DoEmote()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            string textToBeShown;
            if(_isAlive)
                textToBeShown = _aliveEmotes[Random.Range(0, _aliveEmotes.Length)];
            else
                textToBeShown = _deadEmotes[Random.Range(0, _deadEmotes.Length)];
            UpdateEmoteRpc(NetworkObjectId, textToBeShown);
        }
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateEmoteRpc(ulong networkObjectID, string emoteText)
    {
        if(NetworkObjectId != networkObjectID)
            return;
        _emoteText.text = emoteText;
    }
    
    public void TakeDamage(int amount)
    {
        UpdateHealthRpc(amount, NetworkObjectId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateHealthRpc(int amount, ulong networkObjectID)
    {
        if (NetworkObjectId != networkObjectID)
            return;
        _playerHealth.DecreaseHealth(amount);
        _healthText.text = _playerHealth.CurrentHealth.ToString();
    }
    
    private void UpdateHealthUI(int health) => _healthText.text = health.ToString();

    private void OnPlayerDeath(ulong networkObjectID)
    {
        if (!IsServer || networkObjectID != NetworkObjectId)
            return;
        _isAlive = false;
        OnPlayerDeathRpc(networkObjectID);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OnPlayerDeathRpc(ulong networkObjectID)
    {
        if(networkObjectID != NetworkObjectId)
            return;
        _isUpdated = false;
        _isAlive = false;
    }
    private void AimGun()
    {
        _aimDirection = Camera.main.ScreenToWorldPoint(GetCamPos() * _aimDeadzoneValue) - transform.position;
        float angle = Mathf.Atan2(_aimDirection.y, _aimDirection.x);
        Vector3 offsetVector = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * _aimOffsetRadius;
        _aimTransform.position = transform.position + offsetVector;
    }

    private void ShootProjectile()
    {
        if (Input.GetMouseButtonDown(0))
            ShootProjectileOnServerRpc(_aimTransform.position, _aimDirection);
    }

    [Rpc(SendTo.Server)]
    private void ShootProjectileOnServerRpc(Vector3 aimPosition, Vector3 aimDirection)
    {
        InstantiateProjectile(aimPosition, aimDirection);
    }

    private void InstantiateProjectile(Vector3 aimPosition, Vector3 aimDirection)
    {
        ProjectileController projectileController = Instantiate(_projectilePrefab);
        projectileController.SetFriendly(this);
        projectileController.transform.position = aimPosition;
        projectileController.transform.right = aimDirection;
        projectileController.GetComponent<NetworkObject>().Spawn();
    }

    private void DoPlayerMovement()
    {
        Vector3 movementVector = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        _characterController.Move(_movementSpeed * Time.deltaTime * movementVector);
    }
    private Vector3 GetCamPos() => new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);

    private void GoToSpawnPoint()
    {
        GoToSpawnPointServerRpc();
    }
    [Rpc(SendTo.Server)]
    private void GoToSpawnPointServerRpc()
    {
        transform.position = SpawnPosition;
        GoToSpawnPointClientRpc(SpawnPosition);
    }

    [Rpc(SendTo.NotServer)]
    private void GoToSpawnPointClientRpc(Vector3 position)
    {
        if (!IsOwner)
            return;
        transform.position = position;
    }
}
