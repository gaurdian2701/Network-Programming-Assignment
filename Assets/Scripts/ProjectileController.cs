using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ProjectileController : NetworkBehaviour
{
    [SerializeField] private float _projectileSpeed;
    private PlayerClientController _playerFriendly;
    private const int _damage = 20;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            enabled = false;
    }

    private void Update()
    { 
        CheckForEnemyHit();
        Vector3 newPos = _projectileSpeed * Time.deltaTime * transform.right;
        newPos.z = 0;
        transform.position += newPos;
    }

    private void CheckForEnemyHit()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f, LayerMask.GetMask("Player"));
        if (colliders.Length > 0)
        {
            PlayerClientController player = colliders[0].GetComponent<PlayerClientController>();
            if (player && !player.Equals(_playerFriendly))
            {
                player.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }
    }

    public void SetFriendly(PlayerClientController player) => _playerFriendly = player;

    private void OnCollisionEnter(Collision other)
    {
        if (!IsServer)
            return;
        
        Destroy(gameObject);
    }
}