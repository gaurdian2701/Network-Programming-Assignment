using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerClientController : NetworkBehaviour
{
    [SerializeField] private Transform _aimTransform;
    [Range(1, 5)] [SerializeField] private float _offsetRadius;
    [Range(5, 20)] [SerializeField] private float _deadzoneValue;
    [Range(3, 20)] [SerializeField] private float _movementSpeed;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private CharacterController _characterController;

    private Vector3 _aimDirection;
    private void Awake()
    {
        _characterController.enabled = false;
    }

    public void EnablePlayer()
    {
        if (!IsLocalPlayer)
        {
            _characterController.enabled = false;
            enabled = false;
        }
        else
        {
            GoToSpawnPoint();
            _characterController.enabled = true;
        }
    }
    private void Update()
    {
        if (!_characterController.enabled)
            return;

        AimGun();
        ShootProjectile();
        DoPlayerMovement();
    }

    private void AimGun()
    {
        _aimDirection = Camera.main.ScreenToWorldPoint(GetCamPos() * _deadzoneValue) - transform.position;
        float angle = Mathf.Atan2(_aimDirection.y, _aimDirection.x);
        Vector3 offsetVector = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * _offsetRadius;
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
        ProjectileController projectileController = GameManager.Instance.ProjectilePool.GetProjectileFromPool();
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
        if (!IsOwner)
            return;
        if (IsHost)
            transform.position = GameManager.Instance.SpawnPositions[0].position;
        else
            transform.position = GameManager.Instance.SpawnPositions[1].position;
    }
}
